using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics
{
    public static class TutorialManagerEditorWebHandler
    {
        //Web variables
        //REST API paths
        private const string k_BasePath = "https://analytics.cloud.unity3d.com/";
        private const string k_APIPath = k_BasePath + "api/v2/projects/";
        //private const string k_RemoteSettingsPath = k_APIPath + "{0}/configurations/{1}/remotesettings";
        private const string k_RemoteSettingsPath = k_APIPath + "{0}/tutorial/remote_settings";

        //Event for receiving RS data
        public delegate void TMRSReceivedHandler(List<TutorialManagerEditor.RemoteSettingsKeyValueType> remoteSettings);
        public static event TMRSReceivedHandler TMRSDataReceived;

        public delegate void TMRSWriteResponseHandler(bool succes);
        public static event TMRSWriteResponseHandler TMRSWriteResponseReceived;

        public static IEnumerator<AsyncOperation> Read(string appId)
        {
            var settingsRequest = Authorize(UnityWebRequest.Get(GetUrl(appId)));
            if (settingsRequest == null)
            {
                Debug.LogError("Failed getting authentication token. In editor versions before 2018.1, this is done through internal APIs which may be subject to undocumented changes, thus your plugin may need to be updated. Please contact tutorialmanager@unity3d.com for help.");
                ReadErrorEvent();
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
                ReadErrorEvent();
                yield break;
            }

            string remoteSettingsJson = settingsRequest.downloadHandler.text;
            LoadRemoteSettings(remoteSettingsJson);
        }

        //TODO: Implement write once endpoint is ready
        public static IEnumerator<AsyncOperation> Write(string appId)
        {
            var settingsRequest = Authorize(UnityWebRequest.Post(appId, "test payload"));
            if(settingsRequest == null)
            {
                if (settingsRequest == null)
                {
                    Debug.LogError("Failed getting authentication token. In editor versions before 2018.1, this is done through internal APIs which may be subject to undocumented changes, thus your plugin may need to be updated. Please contact tutorialmanager@unity3d.com for help.");
                    WriteEvent(false);
                    yield break;
                }
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
                Debug.LogWarningFormat("Failed to write remote settings: {0}: {1}", settingsRequest.responseCode, settingsRequest.error);
                WriteEvent(false);
                yield break;
            }

            WriteEvent(true);
        }

        private static string GetUrl (string appId)
        {
            return string.Format(k_RemoteSettingsPath, appId);
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
            Debug.Log(remoteSettingsResult);

            //string remoteSettingsJson = "{ \"list\": " + remoteSettingsResult + "}";

            List<TutorialManagerEditor.RemoteSettingsKeyValueType> remoteSettings;
            try
            {
                remoteSettings = JsonUtility.FromJson<TutorialManagerEditor.RemoteSettingsData>(remoteSettingsResult).remoteSettings;
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Remote Settings response was not valid JSON:\n {0} \n {1}", remoteSettingsResult, e);
                ReadErrorEvent();
                return;
            }

            if (TMRSDataReceived != null)
            {
                TMRSDataReceived(remoteSettings);
            }
        }

        static void WriteEvent (bool success)
        {
            if (TMRSWriteResponseReceived != null)
            {
                TMRSWriteResponseReceived(success);
            }
        }

        static void ReadErrorEvent()
        {
            if (TMRSDataReceived != null)
            {
                TMRSDataReceived(null);
            }
        }
    }
}