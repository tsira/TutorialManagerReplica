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

        public delegate void TMRSReadRetryHandler(bool success);
        public static event TMRSReadRetryHandler TMRSReadRetry;

        public delegate void TMRSWriteRetryHandler(bool success);
        public static event TMRSWriteRetryHandler TMRSWriteRetry;

        public static IEnumerator<AsyncOperation> Read(string appId)
        {
            var settingsRequest = Authorize(UnityWebRequest.Get(GetUrl(appId)));
            if (IsAuthError(settingsRequest)) {
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
                if (SuspectBadToken(settingsRequest) && TMRSReadRetry != null) {
                    AccessToken.OnTokenRefresh += OnTokenRefresh;
                    IEnumerator<AsyncOperation> innerLoopEnumerator = AccessToken.RefreshAccessToken().GetEnumerator();
                    while (innerLoopEnumerator.MoveNext())
                        yield return innerLoopEnumerator.Current;
                } else {
                    ReadErrorEvent();
                }
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

            if (IsAuthError(settingsRequest)) {
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
                if (SuspectBadToken(settingsRequest) && TMRSWriteRetry != null) {
                    AccessToken.OnTokenRefresh += OnTokenRefresh;
                    IEnumerator<AsyncOperation> innerLoopEnumerator = AccessToken.RefreshAccessToken().GetEnumerator();
                    while (innerLoopEnumerator.MoveNext())
                        yield return innerLoopEnumerator.Current;
                } else {
                    WriteEvent(false);
                }
                yield break;
            }

            WriteEvent(true);
        }

        private static bool SuspectBadToken(UnityWebRequest settingsRequest)
        {
            return (settingsRequest.responseCode == 400 ||
                    settingsRequest.responseCode == 401 ||
                    settingsRequest.responseCode == 403);
        }

        private static string RemoveWrappingBracesFromString(string stringToParse)
        {
            return stringToParse.Substring(1, stringToParse.Length - 2);
        }

        private static bool IsAuthError(UnityWebRequest request)
        {
            if (request == null) {
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
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", AccessToken.GetAccessToken()));
            return request;
        }

        static void LoadRemoteSettings(string remoteSettingsResult)
        {
            string emptyRS = "{\"remoteSettings\":[]}";
            List<RemoteSettingsKeyValueType> remoteSettings = new List<RemoteSettingsKeyValueType>();

            // Check for empty Remote Settings response
            if (emptyRS.Contains(remoteSettingsResult)) {
                Debug.LogWarningFormat("No Remote Settings were found:\n {0}", remoteSettingsResult);
                if (TMRSReadResponseReceived != null) {
                    TMRSReadResponseReceived(remoteSettings);
                }
                return;
            } else {
                try {
                    remoteSettings = JsonUtility.FromJson<RemoteSettingsData>(remoteSettingsResult).remoteSettings;
                } catch (Exception e) {
                    Debug.LogWarningFormat("Remote Settings response was not valid JSON:\n {0} \n {1}", remoteSettingsResult, e);
                    ReadErrorEvent();
                    return;
                }

                if (TMRSReadResponseReceived != null) {
                    TMRSReadResponseReceived(remoteSettings);
                }
            };
        }

        static void OnTokenRefresh(bool success)
        {
            if (TMRSReadRetry != null) {
                TMRSReadRetry(success);
            }
            if (TMRSWriteRetry != null) {
                TMRSWriteRetry(success);
            }
        }

        static void WriteEvent(bool success)
        {
            if (TMRSWriteResponseReceived != null) {
                TMRSWriteResponseReceived(success);
            }
        }

        static void ReadErrorEvent()
        {
            if (TMRSReadResponseReceived != null) {
                TMRSReadResponseReceived(null);
            }
        }
    }
}