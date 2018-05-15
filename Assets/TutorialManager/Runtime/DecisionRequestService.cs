using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    public class TutorialWebResponse
    {
        public bool showTutorial;
        public Dictionary<string, string> contentTable;
    }

    public static class DecisionRequestService
    {
        const string adaptiveOnboardingUrl = "https://prd-adaptive-onboarding.uca.cloud.unity3d.com/tutorial";
        static GameObject webHandlerGO;

        public delegate void TMDecisionHandler(TutorialWebResponse decision);
        public static event TMDecisionHandler OnDecisionReceived;

        internal static void RequestDecision()
        {
            var deviceInfo = new DeviceInfo();
#if UNITY_ADS
            //if game has ads, we can pull the ads id, otherwise, we can't access this info
            var advertisingSupported = Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string errorMsg) =>
            {
                deviceInfo.adsid = advertisingId;
                deviceInfo.ads_tracking = trackingEnabled;
                CallTutorialManagerService(deviceInfo);
            });
            //If advertising is not supported on this platform, callback won't get fired. Call the tutorial manager service immediately.
            if (!advertisingSupported)
            {
                CallTutorialManagerService(deviceInfo);
            }
#else
            CallTutorialManagerService(deviceInfo);
#endif   
        }

        /// <summary>
        /// Request to fetch a value from Unity Analytics Services to show or skip tutorial for this user.
        /// Returns an int val from the Unity Tutorial Manager Decision Engine to show or skip tutorial for the user.
        /// If the request fails the user will be shown the tutorial by default.
        /// </summary>
        private static void CallTutorialManagerService(DeviceInfo data)
        {
            var json = JsonUtility.ToJson(data);

            webHandlerGO = new GameObject();
            var webHandler = webHandlerGO.AddComponent<TutorialManagerWebHandler>();
            webHandler.PostRequestReturned += (webRequest) =>
            {

                var response = new TutorialWebResponse();
                bool isError = false;
#if UNITY_2017_1_OR_NEWER
                isError = webRequest.isHttpError || webRequest.isNetworkError;
#else
                isError = webRequest.responseCode >= 400 || webRequest.isError;
#endif
                if (isError)
                {
                    Debug.LogWarningFormat("Error received from server: {0}: {1}. Defaulting to true.", webRequest.responseCode, webRequest.error);
                }
                else
                {
                    try
                    {
                        // If web request was successful then proceed with tutorial manager decision
                        string text = webRequest.downloadHandler.text;
                        response.showTutorial = ParseShowTutorial(text);
                        response.contentTable = ParseTextStrings(text);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarningFormat("Tutorial Manager response parsing error: {0}. Defaulting to true.", ex);
                    }
                }
                GameObject.Destroy(webHandlerGO);
                //TODO: fire an event here
                if(OnDecisionReceived != null)
                {
                    OnDecisionReceived(response);
                }
            };
            webHandler.PostJson(adaptiveOnboardingUrl, json);
        }

        private static bool ParseShowTutorial(string text)
        {
            Regex regex = new Regex(@"false(?!showTutorial.{1,3})");
            // NB: I've deliberately turned this logic "upside down" to ensure
            // that failover favors showing the tutorial. MAT - 5/15/2018
            return regex.Match(text).Success ? false : true;
        }

        private static Dictionary<string, string> ParseTextStrings(string text)
        {
            string keyPattern = @"(?:\"")(.*?\-text)(?=\"".{1,3}\"")";
            MatchCollection keyMatches = Regex.Matches(text, keyPattern);
            List<string> keys = keyMatches.Cast<Match>().Select(match => match.Groups[1].ToString()).ToList();

            string valuePattern = @"(?:\-text\"".{1,3})\""(.*?)(?=\"",)";
            MatchCollection valueMatches = Regex.Matches(text, valuePattern);
            List<string> values = valueMatches.Cast<Match>().Select(match => match.Groups[1].ToString()).ToList();

            var dict = keys.Select((k, i) => new { k, v = values[i] }).ToDictionary(x => x.k, x => x.v);
            return dict;
        }
    }
}
