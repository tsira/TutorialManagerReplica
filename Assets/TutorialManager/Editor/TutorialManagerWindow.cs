using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics.TutorialManagerEditor
{
    public class TutorialManagerWindow : EditorWindow
    {
        private const string k_TabTitle = "Tutorial Editor";

        // Actual state
        private string m_AppId = "";

        private IEnumerator<AsyncOperation> m_WebRequestEnumerator;

        private TutorialManagerModelMiddleware TMModel {
            get {
                return TutorialManagerModelMiddleware.GetInstance();
            }
        }

        const string k_MustHaveAnalyticsMessage = "Unity Analytics is not enabled. This toll will not function without it. Please go to Window > Services and enable.";
        const string k_GenreTooltip = "Select the genre which best categorizes this game. Analytics uses this information to optimize your tutorial.";
        const string k_TutorialIdTooltip = "The name by which to identify this tutorial. Must be unique. Only alpha-numerics characters are allowed.";
        const string k_StepIdTooltip = "The name by which to identify each step. Each name must be unique within a tutorial. Only alpha-numerics characters are allowed.";
        const string k_AddTutorialTooltip = "Add a new tutorial.";
        const string k_DeleteTutorialTooltip = "Delete this tutorial. WARNING: deleting a tutorial without cleaning up associated components can cause problems!";
        const string k_AddStepTooltip = "Add a step to this tutorial. You can have up to 10 steps per tutorial.";
        const string k_DeleteStepTooltip = "Delete this step. WARNING: deleting a step without cleaning up associated components can cause problems!";
        const string k_GoToDashboardTooltip = "Go to the Tutorial Manager page on the Analytics dashboard, where you can remotely control this game's tutorials and view reports.";
        const string k_PullDataTooltip = "Retrieve the latest tutorial data from the server.";
        const string k_PushDataTooltip = "Upload local tutorial data to the server. This operation also informs the server that Tutorial Manager integration is complete.";
        const string k_TMTextTooLongMessage = "Some step text is too long; these fields will be truncated! Max text length is {0}. The following text should be shortened: {1}";

        const int k_MaxSteps = 10;
        const float k_ColumnWidth = 100f;

        GUIContent genreGUIContent = new GUIContent("Game Genre", k_GenreTooltip);
        GUIContent tutorialIdGUIContent = new GUIContent("Tutorial Key", k_TutorialIdTooltip);
        GUIContent stepIdGUIContent = new GUIContent("Step Keys", k_StepIdTooltip);
        GUIContent addStepButtonGUIContent = new GUIContent("+", k_AddStepTooltip);
        GUIContent addTutorialButtonGUIContent = new GUIContent("Add Tutorial", k_AddTutorialTooltip);
        GUIContent deleteTutorialButtonGUIContent = new GUIContent("-", k_DeleteTutorialTooltip);
        GUIContent deleteStepButtonGUIContent = new GUIContent("-", k_DeleteStepTooltip);
        GUIContent goToDashboardButtonGUIContent = new GUIContent("Go to Dashboard", k_GoToDashboardTooltip);
        GUIContent pullButtonGUIContent = new GUIContent("Pull Data", k_PullDataTooltip);
        GUIContent pushButtonGUIContent = new GUIContent("Push Data", k_PushDataTooltip);

        const string k_NeedsGenre = "Please select a genre before attempting to push.";
        const string k_TransactionError = "Error when pushing/pulling data. Please try again.";
        const string k_TransactionSuccess = "Success!";
        const string k_TransactionMessage = "In progress...";

        bool isGenreProblem = false;
        bool isTransacting = false;
        bool isTransactionSuccess = false;
        bool isTransactionError = false;



        int transactionTimeoutCounter = 0;
        const int transactionTimeoutMax = 300;

        int genreId;
        string tutorialMarkedForDeletion;
        GUIStyle addButtonStyle;
        Vector2 m_ScrollPosition;

        [MenuItem("Window/Unity Analytics/Tutorial Editor")]
        static void TutorialManagerMenuOption()
        {
            EditorWindow.GetWindow(typeof(TutorialManagerWindow), false, k_TabTitle);
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
            bool markCreateFirst = false;
            if (TMModel.TMData.tutorials.Count == 0 && Event.current.type == EventType.Repaint) {
                markCreateFirst = true;
            }
            if (!Analytics.enabled) {
                EditorGUILayout.HelpBox(k_MustHaveAnalyticsMessage, MessageType.Warning, true);
                return;
            }
            if (addButtonStyle == null) {
                DefineStyles();
            }

            GUILayout.Space(5f);

            RenderHeader();

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            int tutorialCount = TMModel.TMData.tutorials.Count;
            for (int a = 0; a < tutorialCount; a++) {

                var tutorial = TMModel.TMData.tutorials[a];
                RenderTutorial(tutorial, a == 0);
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(5f);

            RenderFooter();

            if (string.IsNullOrEmpty(tutorialMarkedForDeletion) == false)
            {
                DestroyTutorial(tutorialMarkedForDeletion);
                tutorialMarkedForDeletion = string.Empty;
            }
            if (markCreateFirst && Event.current.type == EventType.Repaint) {
                CreateTutorial();
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

        private void RenderWarnings()
        {
            // Text too long
            var tooLongStepText = TestIfStepTextTooLong();
            if (tooLongStepText.Count > 0) {
                string stringiFiedList = string.Join(", ", tooLongStepText.ToArray());
                string message = string.Format(k_TMTextTooLongMessage, 
                                               TutorialManagerModelMiddleware.k_MaxTextlength, stringiFiedList);
                EditorGUILayout.HelpBox(message, MessageType.Warning, true);
            }

            if (isGenreProblem) {
                EditorGUILayout.HelpBox(k_NeedsGenre, MessageType.Warning, true);
            }

            if (isTransacting) {
                EditorGUILayout.HelpBox(k_TransactionMessage, MessageType.Info, true);
            }

            if (isTransactionSuccess) {
                EditorGUILayout.HelpBox(k_TransactionSuccess, MessageType.Info, true);

                transactionTimeoutCounter ++;
                if (transactionTimeoutCounter > transactionTimeoutMax) {
                    isTransactionSuccess = false;
                }
            }

            if (isTransactionError) {
                EditorGUILayout.HelpBox(k_TransactionError, MessageType.Error, true);
            }
        }

        private void RenderFooter()
        {
            RenderWarnings();
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
        }

        private void RenderGenre()
        {
            EditorGUILayout.LabelField(genreGUIContent, EditorStyles.miniLabel, GUILayout.Width(k_ColumnWidth));
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

        private void RenderTutorial(TutorialEntity tutorial, bool deleteDisabled)
        {
            using (new GUILayout.VerticalScope("box")) {
                using (new GUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField(tutorialIdGUIContent, EditorStyles.miniLabel, GUILayout.Width(k_ColumnWidth));

                    RestrictInputCharacters();
                    string tutorialId = EditorGUILayout.TextField(tutorial.id);
                    if (tutorialId != tutorial.id) {
                        UpdateTutorial(tutorial.id, tutorialId);
                    }

                    var prevColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.clear;
                    EditorGUI.BeginDisabledGroup(deleteDisabled);
                    if (GUILayout.Button(deleteTutorialButtonGUIContent, addButtonStyle, GUILayout.MaxWidth(20f))) {
                        MarkTutorialForDeletion(tutorialId);
                    }
                    EditorGUI.EndDisabledGroup();
                    GUI.backgroundColor = prevColor;
                }
                GUILayout.Space(3f);

                for (int a = 0; a < tutorial.steps.Count; a++) {
                    string stepId = tutorial.steps[a];
                    if (TMModel.TMData.stepTable.ContainsKey(stepId)) {
                        RenderStep(tutorial, a, TMModel.TMData.stepTable[stepId]);
                    }
                }
                GUILayout.Space(3f);
            }
        }

        private void RenderStep(TutorialEntity tutorial, int index, StepEntity step)
        {
            using (new GUILayout.HorizontalScope()) {
                string displayName = ParseDisplayNameFromStep(tutorial.id, step.id);
                string newDisplayName = displayName;
                if (index == 0) {
                    EditorGUILayout.LabelField(stepIdGUIContent, EditorStyles.miniLabel, GUILayout.Width(k_ColumnWidth));
                } else {
                    EditorGUILayout.LabelField("", GUILayout.Width(k_ColumnWidth));
                }
                RestrictInputCharacters();
                newDisplayName = EditorGUILayout.TextField(displayName);
                if (newDisplayName != displayName) {
                    UpdateStep(step.id, ConstructStepIdFromDisplayName(tutorial.id, newDisplayName));
                }
                if (index == tutorial.steps.Count - 1) {
                    bool plusEnabled = index < k_MaxSteps - 1;
                    bool minusEnabled = index > 0;

                    var prevColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.clear;
                    EditorGUI.BeginDisabledGroup(!plusEnabled);

                    if (GUILayout.Button(addStepButtonGUIContent, addButtonStyle, GUILayout.MaxWidth(20f))) {
                        CreateStep(tutorial.id);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(!minusEnabled);
                    if (GUILayout.Button(deleteStepButtonGUIContent, addButtonStyle, GUILayout.MaxWidth(20f))) {
                        DestroyStep(step.id);
                    }
                    EditorGUI.EndDisabledGroup();
                    GUI.backgroundColor = prevColor;
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

        List<string> TestIfStepTextTooLong()
        {
            List<string> tooLongStepText = new List<string>();
            foreach (ContentEntity c in TMModel.TMData.content) {
                if (string.IsNullOrEmpty(c.violationText) == false) {
                    tooLongStepText.Add(c.id);
                }
            }
            return tooLongStepText;
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
            if (id != 0) {
                isGenreProblem = false;
            }
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
            CreateStep(id);
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
            isTransactionSuccess = false;
            isTransactionError = false;
            if (genreId == 0) {
                isGenreProblem = true;
            } else {
                isTransacting = true;
                TutorialManagerEditorWebHandler.TMRSWriteResponseReceived += WriteResponseReceived;
                m_WebRequestEnumerator = TutorialManagerEditorWebHandler.Write(m_AppId, TMModel.TMData);
            }
        }

        void WriteResponseReceived(bool success)
        {
            isTransacting = false;
            if (success) {
                isTransactionSuccess = true;
                transactionTimeoutCounter = 0;
            } else {
                isTransactionError = true;
            }
        }

        private void PullData()
        {
            isTransacting = true;
            isTransactionError = false;
            isTransactionSuccess = false;


            TutorialManagerEditorWebHandler.TMRSReadResponseReceived += RemoteSettingsReadDataReceived;
            m_WebRequestEnumerator = TutorialManagerEditorWebHandler.Read(m_AppId);
        }

        private void RemoteSettingsReadDataReceived(List<RemoteSettingsKeyValueType> remoteSettings)
        {
            isTransacting = false;
            TutorialManagerEditorWebHandler.TMRSReadResponseReceived -= RemoteSettingsReadDataReceived;

            if(m_WebRequestEnumerator != null)
            {
                m_WebRequestEnumerator = null;
            }
            if (remoteSettings == null) {
                isTransactionError = true;
                return;
            } else {
                isTransactionSuccess = true;
                transactionTimeoutCounter = 0;

                if (remoteSettings.Count > 0) {
                    TMUpdaterUtility.ConvertRemoteSettingsToModel(TMModel, remoteSettings);
                    TMUpdaterUtility.ForceUpdate();
                }
            }
        }

        private void DefineStyles()
        {
            addButtonStyle = new GUIStyle(GUI.skin.button);
            addButtonStyle.normal.background = null;
            addButtonStyle.fontStyle = FontStyle.Bold;
            addButtonStyle.alignment = TextAnchor.MiddleLeft;
            addButtonStyle.fixedWidth = 30f;
        }
    }
}