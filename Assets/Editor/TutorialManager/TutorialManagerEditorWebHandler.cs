using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Analytics.TutorialManagerRuntime;
using System.Linq;

namespace UnityEngine.Analytics
{
    public static class TutorialManagerEditorWebHandler
    {
        //Web variables
        //REST API paths
        private const string k_BasePath = "https://analytics.cloud.unity3d.com/";
        private const string k_APIPath = k_BasePath + "api/v2/projects/";
        private const string k_RemoteSettingsPath = k_APIPath + "{0}/tutorial/remote_settings";

        //Event for receiving RS data
        public delegate void TMRSReceivedHandler(List<TutorialManagerEngine.RemoteSettingsKeyValueType> remoteSettings);
        public static event TMRSReceivedHandler TMRSDataReceived;

        public delegate void TMRSWriteResponseHandler(bool succes);
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

        public static IEnumerator<AsyncOperation> Write(string appId)
        {
            var model = TutorialManagerModelMiddleware.GetInstance().TMData;
            //TODO: Remove when endpoint is live
            var tutorialName = "tutorial 1";
            var stepName = "step 1";
            var stepLookupName = ConstructID(tutorialName, stepName);
            var textLookupName = ConstructID(stepLookupName, "text");
            var tutorial = new TutorialEntity(tutorialName);
            tutorial.steps.Add(stepName);
            var step = new StepEntity(stepLookupName);
            step.messaging.isActive = true;
            var text = new ContentEntity(textLookupName, "text", "yooo what's up! I work!");
            step.messaging.content.Add(text.id);
            model.tutorials.Add(tutorial);
            model.steps.Add(step);
            model.content.Add(text);
            //END REMOVE CODE

            var tutsArr = model.tutorials.Select(t => t.id).ToArray();

            var tutorials = new TutorialJSON(tutsArr);

            var stepsArr = model.tutorials.Select(t => t.steps).ToArray();
            var stepObj = new StepsJSON(stepsArr[0].ToArray());

            var tutsJson = JsonUtility.ToJson(tutorials);
            //Debug.Log(tutsJson);
            tutsJson = RemoveWrappingBracesFromString(tutsJson);
            var stepObjJson = JsonUtility.ToJson(stepObj);
            stepObjJson = RemoveWrappingBracesFromString(stepObjJson);

            var contentsArr = model.content.Select(c => c.text).ToArray();
            var contentObj = new TextJSON(contentsArr[0]);
            var contentJson = JsonUtility.ToJson(contentObj);
            contentJson = contentJson.Replace("TMContent", model.content[0].id);

            Debug.Log(contentJson);

            Debug.Log(tutsJson);
            stepObjJson = stepObjJson.Replace("TMStep", tutsArr[0]);
            Debug.Log(stepObjJson);
            var stringList = new List<string>();
            stringList.Add(tutsJson);
            stringList.Add(stepObjJson);
            string jsonData = CreateWritePayload(stringList);
            Debug.Log(jsonData);
            var settingsRequest = Authorize(UnityWebRequest.Post(appId, "test payload"));
            if(IsAuthError(settingsRequest))
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

        private static string RemoveWrappingBracesFromString (string stringToParse)
        {
            return stringToParse.Substring(1, stringToParse.Length - 2);
        }

        private static string CreateWritePayload (List<string> payload)
        {
            string retString = "{\"remoteSettings\":{";

            for (int i = 0; i < payload.Count; i++)
            {
                retString += payload[i];
                //if this isn't the last string, add a comma after
                if(i < payload.Count - 1)
                {
                    retString += ",";
                }
            }

            return retString + "}}";
        }

        //TODO: Remove when endpoint is live
        private static string ConstructID(string tutorialId, string stepId)
        {
            return string.Format("{0}-{1}", tutorialId, stepId);
        }

        private static bool IsAuthError(UnityWebRequest request)
        {
            if(request == null)
            {
                Debug.LogError("Failed getting authentication token. In editor versions before 2018.1, this is done through internal APIs which may be subject to undocumented changes, thus your plugin may need to be updated. Please contact tutorialmanager@unity3d.com for help.");
                return true;
            }
            return false;
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
            //string remoteSettingsJson = "{ \"list\": " + remoteSettingsResult + "}";

            List<TutorialManagerEngine.RemoteSettingsKeyValueType> remoteSettings;
            try
            {
                remoteSettings = JsonUtility.FromJson<TutorialManagerEngine.RemoteSettingsData>(remoteSettingsResult).remoteSettings;
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

    public struct TutorialJSON 
    {
        public string[] tutorials;

        public TutorialJSON (string[] tuts)
        {
            tutorials = tuts;
        }
    }

    public struct StepsJSON 
    {
        public string[] TMStep;

        public StepsJSON (string [] s)
        {
            TMStep = s;
        }
    }

    public struct TextJSON
    {
        public string TMContent;

        public TextJSON (string contentid)
        {
            TMContent = contentid;
        }
    }
}