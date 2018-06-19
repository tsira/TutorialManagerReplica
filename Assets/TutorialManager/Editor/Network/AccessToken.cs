using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.Analytics
{


    public class AccessToken
    {

        public delegate void OnTokenRefreshHandler(bool success);
        public static event OnTokenRefreshHandler OnTokenRefresh;

        static void LogAccessToken()
        {
            Debug.LogFormat("Access Token: {0}", GetAccessToken());
        }

        static double s_RefreshTokenStartTime = 0;
        static string s_OldAccessToken;

        public static IEnumerable<AsyncOperation> RefreshAccessToken()
        {
            Debug.LogFormat("Old Access Token: {0}", GetAccessToken());

            var unityConnectObj = Type.GetType("UnityEditor.Connect.UnityConnect, UnityEditor");
            if (unityConnectObj == null) {
                Debug.LogError("Failed to get \"UnityConnect\" class!");
                yield return null;
            }

            var instanceProp = unityConnectObj.GetProperty("instance");
            if (instanceProp == null) {
                Debug.LogError("Failed to get method \"instance\" on UnityConnect class!");
                yield return null;
            }

            var instanceOfConnect = instanceProp.GetValue(null, null);
            if (instanceOfConnect == null) {
                Debug.LogError("Failed to get value of \"instance\" on UnityConnect class!");
                yield return null;
            }

            var refreshAccessTokenMethod = unityConnectObj.GetMethod("RefreshProject");
            if (refreshAccessTokenMethod == null) {
                Debug.LogError("Failed to get method \"RefreshProject\" on UnityConnect class!");
                yield return null;
            }

            s_OldAccessToken = GetAccessToken();
            refreshAccessTokenMethod.Invoke(instanceOfConnect, null);

            s_RefreshTokenStartTime = EditorApplication.timeSinceStartup;

            IEnumerator<AsyncOperation> innerLoopEnumerator = CheckForRefreshedToken().GetEnumerator();
            while (innerLoopEnumerator.MoveNext())
                yield return innerLoopEnumerator.Current;
        }

        static IEnumerable<AsyncOperation> CheckForRefreshedToken()
        {
            // check for time out
            if (EditorApplication.timeSinceStartup - s_RefreshTokenStartTime > 10.0f) {
                Debug.LogErrorFormat("Failed to refresh access token: {0}", GetAccessToken());
                if (OnTokenRefresh != null) {
                    OnTokenRefresh(false);
                }
                yield return null;
            }

            // still waiting
            if (GetAccessToken() == s_OldAccessToken) {
                yield return null;
            }

            Debug.LogFormat("New Access Token: {0}", GetAccessToken());
            if (OnTokenRefresh!= null) {
                OnTokenRefresh(true);
                yield return null;
            }
        }

        public static string GetAccessToken()
        {
#if UNITY_2018_1_OR_NEWER
            return CloudProjectSettings.accessToken;
#else
            var unityConnectObj = Type.GetType("UnityEditor.Connect.UnityConnect, UnityEditor");
            if (unityConnectObj == null)
            {
                Debug.LogError("Failed to get \"UnityConnect\" class!");
                return null;
            }

            var instanceProp = unityConnectObj.GetProperty("instance");
            if (instanceProp == null)
            {
                Debug.LogError("Failed to get method \"instance\" on UnityConnect class!");
                return null;
            }

            var instanceOfConnect = instanceProp.GetValue(null, null);
            if (instanceOfConnect == null)
            {
                Debug.LogError("Failed to get value of \"instance\" on UnityConnect class!");
                return null;
            }

            var getTokenMethod = unityConnectObj.GetMethod("GetAccessToken");
            if (getTokenMethod == null)
            {
                Debug.LogError("Failed to get method \"GetAccessToken\" on UnityConnect class!");
                return null;
            }

            return (string)getTokenMethod.Invoke(instanceOfConnect, null);
#endif
        }
    }
}
