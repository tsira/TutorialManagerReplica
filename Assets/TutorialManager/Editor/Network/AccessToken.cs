using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.Analytics
{


    public class AccessToken
    {

        public delegate void OnTokenRefreshHandler(bool success);
        public static event OnTokenRefreshHandler OnTokenRefresh;

        [MenuItem("Access Token/Log Access Token")]
        static void LogAccessToken()
        {
            Debug.LogFormat("Access Token: {0}", GetAccessToken());
        }

        static double s_RefreshTokenStartTime = 0;
        static string s_OldAccessToken;

        [MenuItem("Access Token/Refresh Access Token")]
        public static void RefreshAccessToken()
        {
            Debug.LogFormat("Old Access Token: {0}", GetAccessToken());

            var unityConnectObj = Type.GetType("UnityEditor.Connect.UnityConnect, UnityEditor");
            if (unityConnectObj == null) {
                Debug.LogError("Failed to get \"UnityConnect\" class!");
                return;
            }

            var instanceProp = unityConnectObj.GetProperty("instance");
            if (instanceProp == null) {
                Debug.LogError("Failed to get method \"instance\" on UnityConnect class!");
                return;
            }

            var instanceOfConnect = instanceProp.GetValue(null, null);
            if (instanceOfConnect == null) {
                Debug.LogError("Failed to get value of \"instance\" on UnityConnect class!");
                return;
            }

            var refreshAccessTokenMethod = unityConnectObj.GetMethod("RefreshProject");
            if (refreshAccessTokenMethod == null) {
                Debug.LogError("Failed to get method \"RefreshProject\" on UnityConnect class!");
                return;
            }

            s_OldAccessToken = GetAccessToken();
            refreshAccessTokenMethod.Invoke(instanceOfConnect, null);

            s_RefreshTokenStartTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += CheckForRefreshedToken;
        }

        static void CheckForRefreshedToken()
        {
            // check for time out
            if (EditorApplication.timeSinceStartup - s_RefreshTokenStartTime > 10.0f) {
                Debug.LogErrorFormat("Failed to refresh access token: {0}", GetAccessToken());
                EditorApplication.update -= CheckForRefreshedToken;
                if (OnTokenRefresh != null) {
                    OnTokenRefresh(false);
                }
                return;
            }

            // still waiting
            if (GetAccessToken() == s_OldAccessToken) {
                return;
            }

            Debug.LogFormat("New Access Token: {0}", GetAccessToken());
            EditorApplication.update -= CheckForRefreshedToken;
            if (OnTokenRefresh!= null) {
                OnTokenRefresh(true);
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
