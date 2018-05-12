using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Analytics
{
    public static class TutorialManagerEditorWebHandler
    {
        private static readonly string m_CurrentEnvironment = "Development";

        //Web variables
        //REST API paths
        private const string k_BasePath = "https://analytics.cloud.unity3d.com/";
        private const string k_APIPath = k_BasePath + "api/v2/projects/";
        private const string k_RemoteSettingsPath = k_APIPath + "{0}/configurations/{1}/remotesettings";

        //Event for receiving RS data
        public delegate void DataReceivedHandler(List<TutorialManagerEditor.RemoteSettingsKeyValueType> remoteSettings);
        public static event DataReceivedHandler DataReceived;

        public static IEnumerator<AsyncOperation> Read(string appId)
        {
            var currentId = m_CurrentEnvironment;

            string remoteSettingsUrl = string.Format(k_RemoteSettingsPath, appId, currentId);
            var settingsRequest = Authorize(UnityWebRequest.Get(remoteSettingsUrl));
            if (settingsRequest == null)
            {
                Debug.LogError("Failed getting authentication token. In editor versions before 2018.1, this is done through internal APIs which may be subject to undocumented changes, thus your plugin may need to be updated. Please contact tutorialmanager@unity3d.com for help.");
                FireErrorEvent();
                yield break;
            }

#if UNITY_2017_2_OR_NEWER
            yield return settingsRequest.SendWebRequest();
#else
            yield return settingsRequest.Send();
#endif

#if UNITY_2017_1_OR_NEWER
            if (settingsRequest.isNetworkError || settingsRequest.isHttpError)
#else
            if (settingsRequest.isError || settingsRequest.responseCode >= 400)
#endif
            {
                Debug.LogWarningFormat("Failed to fetch remote settings: {0}: {1}", settingsRequest.responseCode, settingsRequest.error);
                FireErrorEvent();
                yield break;
            }

            string remoteSettingsJson = settingsRequest.downloadHandler.text;
            LoadRemoteSettings(remoteSettingsJson);
        }

        //TODO: Implement write once endpoint is ready
        public static void Write()
        {

        }

        private static UnityWebRequest Authorize(UnityWebRequest request)
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("User-Agent", "Unity Editor " + Application.unityVersion + " TM " + TutorialManager.k_PluginVersion);

#if UNITY_2018_1_OR_NEWER
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", CloudProjectSettings.accessToken));
#else
            var unityConnectObj = Type.GetType("UnityEditor.Connect.UnityConnect, UnityEditor");
            if (unityConnectObj == null) return null;

            var instanceProp = unityConnectObj.GetProperty("instance");
            if (instanceProp == null) return null;

            var instanceOfConnect = instanceProp.GetValue(null, null);
            if (instanceOfConnect == null) return null;

            var getTokenMethod = unityConnectObj.GetMethod("GetAccessToken");
            if (getTokenMethod == null) return null;

            var token = getTokenMethod.Invoke(instanceOfConnect, null);
            if (token == null) return null;
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", token));
#endif
            return request;
        }

        static void LoadRemoteSettings(string remoteSettingsResult)
        {
            string remoteSettingsJson = "{ \"list\": " + remoteSettingsResult + "}";

            List<TutorialManagerEditor.RemoteSettingsKeyValueType> remoteSettings;
            try
            {
                remoteSettings = JsonUtility.FromJson<TutorialManagerEditor.RemoteSettingsData>(remoteSettingsJson).list;
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Remote Settings response was not valid JSON:\n {0} \n {1}", remoteSettingsResult, e);
                FireErrorEvent();
                return;
            }

            if (DataReceived != null)
            {
                DataReceived(remoteSettings);
            }
        }

        static void FireErrorEvent()
        {
            if (DataReceived != null)
            {
                DataReceived(null);
            }
        }
    }
}