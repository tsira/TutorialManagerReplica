using System;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics
{
    public static class TutorialManagerEditorWebHandler
    {
        //Web variables
        //REST API paths

        // Staging
        // private const string k_BasePath = "https://cloud-staging.uca.cloud.unity3d.com/";
        // Production
        private const string k_BasePath = "https://analytics.cloud.unity3d.com/";

        private const string k_APIPath = k_BasePath + "api/v2/projects/";
        private const string k_RemoteSettingsPath = k_APIPath + "{0}/tutorial/remote_settings";

        //Event for receiving RS data
        public delegate void TMRSReadResponseHandler(List<RemoteSettingsKeyValueType> remoteSettings);
        public static event TMRSReadResponseHandler TMRSReadResponseReceived;

        public delegate void TMRSWriteResponseHandler(bool success);
        public static event TMRSWriteResponseHandler TMRSWriteResponseReceived;

        public static IEnumerator<AsyncOperation> Read(string appId)
        {
            var settingsRequest = Authorize(UnityWebRequest.Get(GetUrl(appId)));
            if (IsAuthError(settingsRequest))
            {
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

        public static IEnumerator<AsyncOperation> Write(string appId, TutorialManagerModel model)
        {
            var json = TMModelToJsonInterpreter.ProcessModelToJson(model);

            //Given UnityWebRequest applies URL encoding to POST message payloads, we're using a PUT instead
            var settingsRequest = Authorize(UnityWebRequest.Put(GetUrl(appId), json));

            if (IsAuthError(settingsRequest))
            {
                WriteEvent(false);
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
                Debug.LogWarningFormat("Failed to write remote settings: {0}: {1}", settingsRequest.responseCode, settingsRequest.error);

                WriteEvent(false);
                yield break;
            }

            WriteEvent(true);
        }

        private static string RemoveWrappingBracesFromString(string stringToParse)
        {
            return stringToParse.Substring(1, stringToParse.Length - 2);
        }

        private static bool IsAuthError(UnityWebRequest request)
        {
            if (request == null)
            {
                Debug.LogError("Failed getting authentication token. In editor versions before 2018.1, this is done through internal APIs which may be subject to undocumented changes, thus your plugin may need to be updated. Please contact tutorialmanager@unity3d.com for help.");
                return true;
            }
            return false;
        }

        private static string GetUrl(string appId)
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
            string emptyRS = "{\"remoteSettings\":[]}";
            List<RemoteSettingsKeyValueType> remoteSettings = new List<RemoteSettingsKeyValueType>();

            // Check for empty Remote Settings response
            if (emptyRS.Contains(remoteSettingsResult))
            {
                Debug.LogWarningFormat("No Remote Settings were found:\n {0}", remoteSettingsResult);
                if (TMRSReadResponseReceived != null) {
                    TMRSReadResponseReceived(remoteSettings);
                }
                return;
            }
            else {
                try
                {
                    remoteSettings = JsonUtility.FromJson<RemoteSettingsData>(remoteSettingsResult).remoteSettings;
                }
                catch (Exception e)
                {
                    Debug.LogWarningFormat("Remote Settings response was not valid JSON:\n {0} \n {1}", remoteSettingsResult, e);
                    ReadErrorEvent();
                    return;
                }

                if (TMRSReadResponseReceived != null)
                {
                    TMRSReadResponseReceived(remoteSettings);
                }  
            };
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
            if (TMRSReadResponseReceived != null)
            {
                TMRSReadResponseReceived(null);
            }
        }
    }
}