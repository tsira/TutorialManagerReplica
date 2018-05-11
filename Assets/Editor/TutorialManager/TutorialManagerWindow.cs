using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics.TutorialManagerEditor
{
    [Serializable]
    public struct RemoteSettingsKeyValueType
    {
        public string key;
        public string value;
        public string type;
    }

    [Serializable]
    public struct RemoteSettingsData
    {
        public List<RemoteSettingsKeyValueType> remoteSettings;
    }

    public class TutorialManagerWindow : EditorWindow
    {
        private const string m_TabTitle = "Tutorial Manager";

        // Actual state
        private string m_AppId = "";

        private IEnumerator<AsyncOperation> m_WebRequestEnumerator;

        private TutorialManagerModelMiddleware TMModel {
            get {
                return TutorialManagerModelMiddleware.GetInstance();
            }
        }

        const string k_GenreTooltip = "Select the genre which best categorizes this game. Analytics uses this information to optimize your tutorial.";
        const string k_TutorialIdTooltip = "The name by which to identify this tutorial. Must be unique. Only alpha-numerics characters are allowed.";
        const string k_StepIdTooltip = "The name by which to identify this step. Must be unique within this tutorial. Only alpha-numerics characters are allowed.";
        const string k_AddTutorialTooltip = "Add a new tutorial.";
        const string k_DeleteTutorialTooltip = "Delete this tutorial. WARNING: deleting a tutorial without cleaning up associated components can cause problems!";
        const string k_AddStepTooltip = "Add a step to this tutorial. You can have up to 10 steps per tutorial.";
        const string k_DeleteStepTooltip = "Delete this step. WARNING: deleting a step without cleaning up associated components can cause problems!";
        const string k_GoToDashboardTooltip = "Go to the Tutorial Manager page on the Analytics dashboard, where you can remotely control this game's tutorials and view reports.";
        const string k_PullDataTooltip = "Retrieve the latest tutorial data from the server.";
        const string k_PushDataTooltip = "Upload local tutorial data to the server. This operation also informs the server that Tutorial Manager integration is complete.";

        GUIContent genreGUIContent = new GUIContent("Genre", k_GenreTooltip);
        GUIContent tutorialIdGUIContent = new GUIContent("ID", k_TutorialIdTooltip);
        GUIContent stepIdGUIContent = new GUIContent("ID", k_StepIdTooltip);
        GUIContent addStepButtonGUIContent = new GUIContent("+ Step", k_AddStepTooltip);
        GUIContent addTutorialButtonGUIContent = new GUIContent("+ Tutorial", k_AddTutorialTooltip);
        GUIContent deleteTutorialButtonGUIContent = new GUIContent("-", k_DeleteTutorialTooltip);
        GUIContent deleteStepButtonGUIContent = new GUIContent("-", k_DeleteStepTooltip);
        GUIContent goToDashboardButtonGUIContent = new GUIContent("Go to Dashboard", k_GoToDashboardTooltip);
        GUIContent pullButtonGUIContent = new GUIContent("Pull Data", k_PullDataTooltip);
        GUIContent pushButtonGUIContent = new GUIContent("Push Data", k_PushDataTooltip);

        int genreId;
        string tutorialMarkedForDeletion;


        [MenuItem("Window/Unity Analytics/TutorialManager")]
        static void TutorialManagerMenuOption()
        {
            EditorWindow.GetWindow(typeof(TutorialManagerWindow), false, m_TabTitle);
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        private void OnFocus()
        {
            RestoreAppId();
        }

        protected void RestoreAppId()
        {
#if UNITY_5_3_OR_NEWER
            if (string.IsNullOrEmpty(m_AppId) && !string.IsNullOrEmpty(Application.cloudProjectId) || !m_AppId.Equals(Application.cloudProjectId))
            {
                m_AppId = Application.cloudProjectId;
            }
#endif
        }

        private void OnGUI()
        {
            RenderHeader();

            GUILayout.Space(10f);

            int tutorialCount = TMModel.TMData.tutorials.Count;
            for (int a = 0; a < tutorialCount; a++) {

                var tutorial = TMModel.TMData.tutorials[a];
                RenderTutorial(tutorial);
            }

            GUILayout.Space(10f);

            RenderFooter();

            if (string.IsNullOrEmpty(tutorialMarkedForDeletion) == false)
            {
                DestroyTutorial(tutorialMarkedForDeletion);
                tutorialMarkedForDeletion = string.Empty;
            }
        }

        void Update()
        {
            EnumerateWebRequest();
        }

        void EnumerateWebRequest()
        {
            if (m_WebRequestEnumerator != null)
            {
                if (m_WebRequestEnumerator.Current == null)
                {
                    if(m_WebRequestEnumerator.MoveNext() == false)
                    {
                        m_WebRequestEnumerator = null;
                        return;
                    }
                }
                if (m_WebRequestEnumerator.Current.isDone && !m_WebRequestEnumerator.MoveNext())
                {
                    m_WebRequestEnumerator = null;
                }
            }
        }

        private void RenderHeader()
        {
            using (new GUILayout.HorizontalScope()) {
                RenderGenre();
            }
        }

        private void RenderFooter()
        {
            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button(addTutorialButtonGUIContent)) {
                    CreateTutorial();
                }
                if (GUILayout.Button(pullButtonGUIContent)) {
                    PullData();
                }
                if (GUILayout.Button(pushButtonGUIContent)) {
                    PushData();
                }
                RenderDashboardLink();
            }

            if (GUILayout.Button("Test Auth")) {
                PullData();
            }

            if (GUILayout.Button("Clear Model")) {
                TMModel.Clear();
            }
        }

        private void RenderGenre()
        {
            EditorGUILayout.LabelField(genreGUIContent, GUILayout.Width(40f));
            int id = genreId;
            id = EditorGUILayout.IntPopup(id, TMGenre.genres, TMGenre.genreIds);

            if (id != genreId) {
                SetGenre(id);
            }
        }

        private void RenderDashboardLink()
        {
            if (GUILayout.Button(goToDashboardButtonGUIContent)) {
                string appId = Application.cloudProjectId;
                string url = "https://analytics.cloud.unity3d.com/projects/" + appId + "/tutorial";
                Application.OpenURL(url);
            }
        }

        private void RenderTutorial(TutorialEntity tutorial)
        {
            using (new GUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField(tutorialIdGUIContent, GUILayout.Width(40f));
                RestrictInputCharacters();
                string tutorialId = EditorGUILayout.TextField(tutorial.id);
                if (tutorialId != tutorial.id) {
                    UpdateTutorial(tutorial.id, tutorialId);
                }
                if (GUILayout.Button(deleteTutorialButtonGUIContent, GUILayout.Width(20f))) {
                    MarkTutorialForDeletion(tutorialId);
                }
                if (GUILayout.Button(addStepButtonGUIContent)) {
                    CreateStep(tutorial.id);
                }
            }

            for (int a = 0; a < tutorial.steps.Count; a++) {
                string stepId = tutorial.steps[a];
                if (TMModel.TMData.stepTable.ContainsKey(stepId)) {
                    RenderStep(tutorial, a, TMModel.TMData.stepTable[stepId]);
                }
            }
        }

        private void RenderStep(TutorialEntity tutorial, int index, StepEntity step)
        {
            using (new GUILayout.HorizontalScope()) {
                string displayName = ParseDisplayNameFromStep(tutorial.id, step.id);
                string newDisplayName = displayName;
                GUIContent gUIContent = stepIdGUIContent;
                gUIContent.text = (index + 1).ToString();
                EditorGUILayout.LabelField(gUIContent, GUILayout.Width(40f));
                RestrictInputCharacters();
                newDisplayName = EditorGUILayout.TextField(displayName);
                if (newDisplayName != displayName) {
                    UpdateStep(step.id, ConstructStepIdFromDisplayName(tutorial.id, newDisplayName));
                }

                if (GUILayout.Button(deleteStepButtonGUIContent, GUILayout.Width(20f))) {
                    DestroyStep(step.id);
                }
            }
        }

        string ParseDisplayNameFromStep(string tutorialId, string stepId)
        {
            return stepId.Substring(tutorialId.Length + 1);
        }

        string ConstructStepIdFromDisplayName(string tutorialId, string stepId)
        {
            return string.Format("{0}-{1}", tutorialId, stepId);
        }

        void RestrictInputCharacters()
        {
            char chr = Event.current.character;
            if ((chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9')) {
                Event.current.character = '\0';
            }
        }

        private void SetGenre(int id)
        {
            genreId = id;
            TMModel.SaveGenre(TMGenre.genres[id]);
        }

        private void CreateTutorial()
        {
            string id = "Tutorial" + (TMModel.TMData.tutorials.Count + 1);

            // If the auto-generated key exists, attempt to append a letter
            if (TMModel.TMData.tutorialTable.ContainsKey(id)) {
                string[] alphabet = new string[] { "a", "b", "c", "d", "e", "f", "g" };
                int a = 0;
                while (TMModel.TMData.tutorialTable.ContainsKey(id) && a < alphabet.Length) {
                    id += alphabet[a];
                    a++;
                }
            }
            TMModel.CreateTutorialEntity(id);
        }

        private void UpdateTutorial(string oldId, string newId)
        {
            TMModel.UpdateTutorialEntity(oldId, newId);
        }

        private void MarkTutorialForDeletion(string tutorialId)
        {
            tutorialMarkedForDeletion = tutorialId;
        }

        private void DestroyTutorial(string id)
        {
            TMModel.DestroyTutorialEntity(id);
        }

        private void CreateStep(string tutorialId)
        {
            TutorialEntity tutorial = TMModel.TMData.tutorialTable[tutorialId];
            string id = "Step" + (tutorial.steps.Count + 1);

            // If the auto-generated key exists, attempt to append a letter
            string concatenatedId = ConstructStepIdFromDisplayName(tutorialId, id);
            if (TMModel.TMData.stepTable.ContainsKey(concatenatedId)) {
                string[] alphabet = new string[] { "a", "b", "c", "d", "e", "f", "g" };
                int a = 0;
                while (TMModel.TMData.stepTable.ContainsKey(concatenatedId) && a < alphabet.Length) {
                    id += alphabet[a];
                    concatenatedId = ConstructStepIdFromDisplayName(tutorialId, id);
                    a++;
                }
            }

            TMModel.CreateStepEntity(id, tutorialId);
        }

        private void UpdateStep(string oldId, string newId)
        {
            TMModel.UpdateStepEntity(oldId, newId);
        }

        private void DestroyStep(string id)
        {
            TMModel.DestroyStepEntity(id);
        }

        private void PushData()
        {
            //TODO: Figure out write flow
            TutorialManagerEditorWebHandler.TMRSWriteResponseReceived += GetWriteResponseReceived();
            m_WebRequestEnumerator = TutorialManagerEditorWebHandler.Write(m_AppId);

            //TutorialManagerEditorWebHandler.Write(m_AppId, TMModel.TMData);
        }

        private TutorialManagerEditorWebHandler.TMRSWriteResponseHandler GetWriteResponseReceived()
        {
            return null;
        }

        private void PullData()
        {
            TutorialManagerEditorWebHandler.TMRSDataReceived += RemoteSettingsReadDataReceived;
            m_WebRequestEnumerator = TutorialManagerEditorWebHandler.Read(m_AppId);
        }

        private void RemoteSettingsReadDataReceived(List<RemoteSettingsKeyValueType> remoteSettings)
        {
            TutorialManagerEditorWebHandler.TMRSDataReceived -= RemoteSettingsReadDataReceived;
            if(m_WebRequestEnumerator != null)
            {
                m_WebRequestEnumerator = null;
            }
            if (remoteSettings == null)
            {
                //something went wrong - do nothing
                return;
            }

            //Temp logging to ensure it worked
            Debug.Log(remoteSettings.Count);
            Debug.Log(remoteSettings.First().key);
        }
    }
}