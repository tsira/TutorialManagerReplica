using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Analytics;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UnityEditor.CDP
{
    public enum TMEditorEvent
    {
        editorOpened,
        pullData,
        pushData,
        addTutorial,
        addStep,
        addAdaptiveContent,
        addAdaptiveText,
        addBinding,
        removeTutorial,
        removeStep,
        removeBinding,
        clearProgress,
        pickGenre,
        gotoDashboard
    }

    public class CDPEvent
    {
        const string endpointUrl = "https://prd-lender.cdp.internal.unity3d.com/v1/events";
        const string eventPrefix = "tutorialManager.";
        const string eventVersion = ".v1";
        const string stringPattern = "\"{0}\":\"{1}\"";
        const string nonStringPattern = "\"{0}\":{1}";
        const string payloadPattern = "{{\"type\":\"{0}\", \"msg\": {1}}}";


        public static void Send(TMEditorEvent tMEvent, Dictionary<string, object> dictionary = null)
        {
            var dict = dictionary;
            if (dict == null) {
                dict = new Dictionary<string, object>();
            }

            AddCommonValues(ref dict);

            string eventName = string.Format("{0}{1}{2}", eventPrefix, tMEvent.ToString(), eventVersion);
            string msg = DictToJson(dict);
            string payload = string.Format(payloadPattern, eventName, msg);

            // Debug.Log(payload);
            UnityWebRequest request = UnityWebRequest.Post(endpointUrl, payload);
            var uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(payload));
            uploadHandler.contentType = "application/json";
            request.uploadHandler = uploadHandler;

#if UNITY_2018_1_OR_NEWER
            request.SendWebRequest();
#else
            request.Send();
#endif
        }

        static void AddCommonValues(ref Dictionary<string, object> dictionary)
        {
            string userId = GetUserId();
#if UNITY_2018_1_OR_NEWER
            string projectId = CloudProjectSettings.projectId;
#else
            string projectId = Application.cloudProjectId;
#endif

            dictionary.Add("ts", (double)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            dictionary.Add("cloud_user_id", userId);
            dictionary.Add("app_id", projectId);
            dictionary.Add("unity_version_name", Application.unityVersion);
#if UNITY_ANALYTICS
            bool isAnalyticsEnabled = true;
#else
            bool isAnalyticsEnabled = false;
#endif
            dictionary.Add("analytics_enabled", isAnalyticsEnabled);
            dictionary.Add("tm_version", TutorialManager.k_VersionNumber);
        }

        static string DictToJson(Dictionary<string, object> dictionary)
        {
            string s = String.Empty;
            int a = 0;
            foreach (KeyValuePair<string, object> kv in dictionary) {
                string kvs = string.Empty;
                Type t = kv.Value.GetType();
                if (t == typeof(string)) {
                    kvs = string.Format(stringPattern, kv.Key, kv.Value);
                } else if (t == typeof(bool)) {
                    kvs = string.Format(nonStringPattern, kv.Key, kv.Value.ToString().ToLower());
                } else {
                    kvs = string.Format(nonStringPattern, kv.Key, kv.Value);
                }
                s += kvs;

                if (a < dictionary.Count - 1) {
                    s += ",";
                }
                a++;
            }
            return string.Concat("{", s, "}");
        }

        static string GetUserId()
        {
#if UNITY_2018_1_OR_NEWER
            return CloudProjectSettings.userId;
#else
            var unityConnectObj = Type.GetType("UnityEditor.Connect.UnityConnect, UnityEditor");
            if (unityConnectObj == null) {
                Debug.LogError("Failed to get \"UnityConnect\" class!");
                return null;
            }

            var instanceProp = unityConnectObj.GetProperty("instance");
            if (instanceProp == null) {
                Debug.LogError("Failed to get method \"instance\" on UnityConnect class!");
                return null;
            }

            var instanceOfConnect = instanceProp.GetValue(null, null);
            if (instanceOfConnect == null) {
                Debug.LogError("Failed to get value of \"instance\" on UnityConnect class!");
                return null;
            }

            var getUserIdMethod = unityConnectObj.GetMethod("GetUserId");
            if (getUserIdMethod == null) {
                Debug.LogError("Failed to get method \"GetUserId\" on UnityConnect class!");
                return null;
            }

            return (string)getUserIdMethod.Invoke(instanceOfConnect, null);
#endif
        }
    }
}
