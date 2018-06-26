using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System.Net;
using System.Timers;
using UnityEngine.Networking;
using System.Text;

namespace UnityEditor.CDP
{
    public class CDPEvent
    {
        //const string endpointUrl = "https://stg-lender.cdp.internal.unity3d.com/v1/events";
        const string endpointUrl = "https://prd-lender.cdp.internal.unity3d.com/v1/events";

        const string stringPattern = "\"{0}\":\"{1}\"";
        const string nonStringPattern = "\"{0}\":{1}";
        const string payloadPattern = "{{\"type\":\"{0}\", \"msg\": {1}}}";

        const int maxRetries = 3;

        public static void Send(string eventName, Dictionary<string, object> dictionary = null)
        {
            new CDPEvent(eventName, dictionary);
        }

        int currentRetries = 0;
        string payload;
        private IEnumerator<AsyncOperation> m_WebRequestEnumerator;

        CDPEvent(string eventName, Dictionary<string, object> dictionary = null)
        {
            var dict = dictionary;
            if (dict == null) {
                dict = new Dictionary<string, object>();
            }
            AddCommonValues(ref dict);
            string msg = DictToJson(dict);
            payload = string.Format(payloadPattern, eventName, msg);

            ListenToEditorUpdates();
            m_WebRequestEnumerator = BeginSendSequence();
        }

        void ListenToEditorUpdates()
        {
            EditorApplication.update += EditorApplication_Update;
        }

        void EditorApplication_Update()
        {
            if (m_WebRequestEnumerator != null) {
                if (m_WebRequestEnumerator.Current == null) {
                    if (m_WebRequestEnumerator.MoveNext() == false) {
                        CleanUp();
                        return;
                    }
                }
                if (m_WebRequestEnumerator.Current == null || m_WebRequestEnumerator.Current.isDone && !m_WebRequestEnumerator.MoveNext()) {
                    CleanUp();
                }
            }
        }

        IEnumerator<AsyncOperation> BeginSendSequence()
        {
            IEnumerator<AsyncOperation> innerLoopEnumerator = Flush().GetEnumerator();
            while (innerLoopEnumerator.MoveNext())
                yield return innerLoopEnumerator.Current;
        }

        IEnumerable<AsyncOperation> Flush()
        {
            UnityWebRequest request = new UnityWebRequest(endpointUrl);
            request.uploadHandler = new UploadHandlerRaw(Encoding.ASCII.GetBytes(payload));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.method = UnityWebRequest.kHttpVerbPOST;

#if UNITY_2017_2_OR_NEWER
            yield return request.SendWebRequest();
#else
            yield return request.Send();
#endif

#if UNITY_2017_1_OR_NEWER
            if (request.isNetworkError || request.isHttpError)
#else
            if (request.isError || request.responseCode >= 400)
#endif
            {
                IEnumerator<AsyncOperation> innerLoopEnumerator = Retry().GetEnumerator();
                while (innerLoopEnumerator.MoveNext())
                    yield return innerLoopEnumerator.Current;
            } else {
                CleanUp();
            }
        }

        IEnumerable<AsyncOperation> Retry()
        {
            if (currentRetries < maxRetries) {
                currentRetries++;
                Debug.LogWarningFormat("Retrying {0}", currentRetries);
                IEnumerator<AsyncOperation> innerLoopEnumerator = Flush().GetEnumerator();
                while (innerLoopEnumerator.MoveNext())
                    yield return innerLoopEnumerator.Current;
            } else {
                Debug.LogWarningFormat("Retries failed...stopping.");
                CleanUp();
            }
        }

        void CleanUp()
        {
            EditorApplication.update -= EditorApplication_Update;
            m_WebRequestEnumerator = null;
        }

        static void AddCommonValues(ref Dictionary<string, object> dictionary)
        {
            string userId = GetUserId();
#if UNITY_2018_1_OR_NEWER
            string projectId = CloudProjectSettings.projectId;
#else
            string projectId = Application.cloudProjectId;
#endif
            dictionary.Add("ts", (double)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds);
            dictionary.Add("cloud_user_id", userId);
            dictionary.Add("app_id", projectId);
            dictionary.Add("unity_version_name", Application.unityVersion);
#if UNITY_ANALYTICS
            bool isAnalyticsEnabled = true;
#else
            bool isAnalyticsEnabled = false;
#endif
            dictionary.Add("analytics_enabled", isAnalyticsEnabled);
        }

        static string DictToJson(Dictionary<string, object> dictionary)
        {
            string s = String.Empty;
            int a = 0;
            foreach (KeyValuePair<string, object> kv in dictionary) {
                string kvs = string.Empty;
                Type t = kv.Value.GetType();
                if (t == typeof(string)) {
                    string value = kv.Value.ToString();
                    value = value.Replace("\"", "\\\"");
                    kvs = string.Format(stringPattern, kv.Key, value);
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
