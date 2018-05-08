using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Linq;

namespace UnityEngine.Analytics.TutorialManagerEngine
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
        public List<RemoteSettingsKeyValueType> list;
    }

    public class TutorialManagerWindow : EditorWindow
    {
        private const string m_TabTitle = "Tutorial Manager";

        // Actual state
        private string m_AppId = "";

        private IEnumerator<AsyncOperation> m_WebRequestEnumerator;


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
            if (GUILayout.Button("Test Auth"))
            {
                PullData();
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

        private void SetGenre(string type)
        {

        }

        private void CreateTutorial(string id)
        {

        }

        private void UpdateTutorial(string id)
        {

        }

        private void DestroyTutorial(string id)
        {

        }

        private void CreateStep(string id)
        {

        }

        private void UpdateStep(string id)
        {

        }

        private void DestroyStep(string id)
        {

        }

        private void PushData()
        {

        }

        private void PullData()
        {
            TutorialManagerEditorWebHandler.DataReceived += RemoteSettingsDataReceived;
            m_WebRequestEnumerator = TutorialManagerEditorWebHandler.Read(m_AppId);
        }

        private void RemoteSettingsDataReceived(List<RemoteSettingsKeyValueType> remoteSettings)
        {
            TutorialManagerEditorWebHandler.DataReceived -= RemoteSettingsDataReceived;
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