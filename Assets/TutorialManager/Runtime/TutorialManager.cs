using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Tutorial manager configures the ABTester for the Tutorial test,
/// and encapsulates some key functionality.
/// </summary>
public class TutorialManager
{
    static string tutorialKey = "show_tutorial";
    static int tutorialStep = 0;

    const string tutorialIdKey = "tutorial_id";
    const string tutorialStepPlayerPrefsKey = "unity_analytics_tutorial_test_current_step";
    const string adaptiveOnboardingEventName = "adaptive_onboarding";
    const string tutorialOnKey = "tutorial_on";
    const string tutorialTestGroupKey = "test_group";
    const string testGroupValue = "test";
    const string controlGroupValue = "control";
    const string adaptiveOnboardingSentPrefsKey = "adaptive_onboarding_event_sent";
    static int adaptiveOnboardingEventSent = 0;
    const string adaptiveOnboardingShowTutorialPrefsKey = "adaptive_onboarding_show_tutorial";
    //const string url = "https://stg-adaptive-onboarding.uca.cloud.unity3d.com/tutorial";
    const string url = "https://prd-adaptive-onboarding.uca.cloud.unity3d.com/tutorial";

    static GameObject webHandlerGO;

    public class DeviceInfo
    {
        public string model;
        public int ram;
        public string cpu;
        public string gfx_name;
        public string gfx_vendor;
        public string deviceid;
        public int cpu_count;
        public int dpi;
        public string screen;
        public string appid;
        public string platform;
        public int platformid;
        public string os_ver;
        public int gfx_shader;
        public string gfx_ver;
        public int max_texture_size;
        public string app_ver;
        public bool in_editor;
        public string network;
        public string screen_orientation;
        public float realtime_since_startup;
        public float battery_level;
        public string battery_status;
        public string adsid;
        public bool ads_tracking;
        public string lang;
        public string build_guid;
        public string app_install_mode;
        public string app_install_store;
        public string userid;
        public long sessionid;
        public string device_name;

        public DeviceInfo()
        {
            this.appid = Application.cloudProjectId;
            this.app_ver = Application.version;
            this.model = GetDeviceModel();
            this.deviceid = SystemInfo.deviceUniqueIdentifier;
            this.ram = SystemInfo.systemMemorySize;
            this.cpu = SystemInfo.processorType;
            this.cpu_count = SystemInfo.processorCount;
            this.gfx_name = SystemInfo.graphicsDeviceName;
            this.gfx_vendor = SystemInfo.graphicsDeviceVendor;
            this.screen = Screen.currentResolution.ToString();
            this.dpi = (int)Screen.dpi;
            this.in_editor = Application.isEditor;
            this.platform = Application.platform.ToString();
            this.platformid = (int)Application.platform;
            this.os_ver = SystemInfo.operatingSystem;
            this.gfx_shader = SystemInfo.graphicsShaderLevel;
            this.gfx_ver = SystemInfo.graphicsDeviceVersion;
            this.max_texture_size = SystemInfo.maxTextureSize;
            this.network = Application.internetReachability.ToString();
            this.screen_orientation = Screen.orientation.ToString();
            this.realtime_since_startup = Time.realtimeSinceStartup;
            this.battery_level = SystemInfo.batteryLevel;
            this.battery_status = SystemInfo.batteryStatus.ToString();
            this.lang = Application.systemLanguage.ToString();
            this.build_guid = Application.buildGUID;
            this.app_install_mode = Application.installMode.ToString();
            this.app_install_store = Application.installerName;
#if UNITY_2017_2_OR_NEWER
            this.userid = AnalyticsSessionInfo.userId;
            this.sessionid = AnalyticsSessionInfo.sessionId;
#else
            this.userid = PlayerPrefs.GetString("unity.cloud_userid");
            if(PlayerPrefs.HasKey("unity.player_sessionid"))
            {
                this.sessionid = System.Convert.ToInt64(PlayerPrefs.GetString("unity.player_sessionid"));
            }
            else
            {
                this.sessionid = 0;
            }
#endif
            this.adsid = "";
            this.ads_tracking = false;
            this.device_name = SystemInfo.deviceName;
        }

        private string GetDeviceModel()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // get manufacturer/model/device
            AndroidJavaClass jc = new AndroidJavaClass("android.os.Build");
            string manufacturer = jc.GetStatic<string>("MANUFACTURER");
            string model = jc.GetStatic<string>("MODEL");
            string device = jc.GetStatic<string>("DEVICE");
            return String.Format("{0}/{1}/{2}", manufacturer, model, device);
#else
            return SystemInfo.deviceModel;
#endif
        }
    }

    public class TutorialWebResponse
    {
        public bool show_tutorial;
    }

    public class ValuesJSONParser
    {
        public bool app_installed;
    }

    [RuntimeInitializeOnLoadMethod]
    static void InitializeTutorialManager()
    {
        if (File.Exists(GetAnalyticsValuesLocation()))
        {
            if (JsonUtility.FromJson<ValuesJSONParser>(File.ReadAllText(GetAnalyticsValuesLocation())).app_installed == true)
            {
                return;
            }
        }
        if (PlayerPrefs.HasKey(adaptiveOnboardingShowTutorialPrefsKey))
        {
            return;
        }
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

    static string GetAnalyticsValuesLocation()
    {
#if UNITY_TVOS
        return Application.temporaryCachePath + "/Unity/" + Application.cloudProjectId + "/Analytics/values";
#else
        return Application.persistentDataPath + "/Unity/" + Application.cloudProjectId + "/Analytics/values";
#endif
    }

    static void CallTutorialManagerService(DeviceInfo data)
    {
        var json = JsonUtility.ToJson(data);

        webHandlerGO = new GameObject();
        var webHandler = webHandlerGO.AddComponent<TutorialManagerWebHandler>();
        TutorialManagerWebHandler.PostRequestReturned += (webRequest) => {
            var toShow = true;
            if (webRequest.isHttpError || webRequest.isHttpError)
            {
                Debug.LogWarning("Error received from server: " + webRequest.error + ". Defaulting to true.");
            }
            else
            {
                try 
                {
                    //no error - proceed
                    toShow = JsonUtility.FromJson<TutorialWebResponse>(webRequest.downloadHandler.text).show_tutorial;
                }
                catch(System.Exception ex)
                {
                    Debug.LogWarning("Tutorial Manager response parsing error: " + ex);
                }
            }
            GameObject.Destroy(webHandlerGO);
            PlayerPrefs.SetInt(adaptiveOnboardingShowTutorialPrefsKey, toShow ? 1 : 0);
            PlayerPrefs.Save();
        };


        webHandler.PostJson(url, json);
    }

    static void HandleAdaptiveOnboardingEvent(bool toShow)
    {
        adaptiveOnboardingEventSent = PlayerPrefs.GetInt(adaptiveOnboardingSentPrefsKey, adaptiveOnboardingEventSent);
        if (adaptiveOnboardingEventSent == 0)
        {
            if (toShow)
            {
                Analytics.CustomEvent("tutorial_start", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
            }
            Analytics.CustomEvent(adaptiveOnboardingEventName,
                new Dictionary<string, object>{
                    { tutorialOnKey, toShow }
                }
            );
            adaptiveOnboardingEventSent = 1;
            PlayerPrefs.SetInt(adaptiveOnboardingSentPrefsKey, adaptiveOnboardingEventSent);
            PlayerPrefs.Save();
        }
    }


    /// <summary>
    /// Determine whether to show the tutorial.
    /// </summary>
    /// <remarks>
    ///     <code>
    ///     if (TutorialManager.ShowTutorial()) {
    ///         // show the tutorial
    ///     } else {
    ///         // skip the tutorial
    ///     }
    ///     </code>
    /// </remarks>
    /// <returns><c>true</c>, if tutorial should be shown, <c>false</c> otherwise.</returns>
    public static bool ShowTutorial()
    {
        bool toShow = true;
        toShow = PlayerPrefs.GetInt(adaptiveOnboardingShowTutorialPrefsKey, 1) == 1;
        tutorialStep = 0;
        SetTutorialStep(tutorialStep);
        HandleAdaptiveOnboardingEvent(toShow);
        return toShow;
    }

    /// <summary>
    /// Overload of ShowTutorial with a bool passed in that will be returned as the decision. Used to mock
    /// the result of ShowTutorial for QA purposes. Please do not push this to 'live' builds.
    /// </summary>
    /// <remarks>
    ///     <code>
    ///     if (TutorialManager.ShowTutorial(true)) {
    ///         // show the tutorial
    ///     } else {
    ///         // skip the tutorial
    ///     }
    ///     </code>
    /// </remarks>
    /// <param name="overrideDecision">Value that will be returned by this method for QA.</param>
    /// <returns><c>true</c>, if tutorial should be shown, <c>false</c> otherwise.</returns>
    public static bool ShowTutorial(bool overrideDecision)
    {
        HandleAdaptiveOnboardingEvent(overrideDecision);
        tutorialStep = 0;
        SetTutorialStep(tutorialStep);
        return overrideDecision;
    }

    /// <summary>
    /// Call this when the player completes the tutorial.
    /// </summary>
    public static AnalyticsResult CompleteTutorial()
    {
        return Analytics.CustomEvent("tutorial_complete", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
    }

    /// <summary>
    /// Call this if the player skips the tutorial.
    /// </summary>
    public static AnalyticsResult SkipTutorial()
    {
        return Analytics.CustomEvent("tutorial_skip", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
    }

    /// <summary>
    /// Call this each time the player advances a step in the tutorial.
    /// </summary>
    public static AnalyticsResult AdvanceTutorial()
    {
        tutorialStep = GetTutorialStep();
        tutorialStep++;
        SetTutorialStep(tutorialStep);
        return Analytics.CustomEvent("tutorial_step", new Dictionary<string, object> {
            { tutorialIdKey, tutorialKey },
            {"step_index", tutorialStep}
        });
    }

    static void SetTutorialStep(int newTutorialStep)
    {
        PlayerPrefs.SetInt(tutorialStepPlayerPrefsKey, newTutorialStep);
        PlayerPrefs.Save();
    }

    static int GetTutorialStep()
    {
        if (PlayerPrefs.HasKey(tutorialStepPlayerPrefsKey))
        {
            return tutorialStep = PlayerPrefs.GetInt(tutorialStepPlayerPrefsKey);
        }
        return 0;
    }

}
