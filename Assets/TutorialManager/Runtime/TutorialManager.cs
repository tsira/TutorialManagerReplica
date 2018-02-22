using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;

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
    const string adaptiveOnboardingShowTutorialPrefsKey = "adaptive_onboarding_show_tutorial";

    private class DeviceInfo
    {
        public string model;
        public int ram;
        public string cpu;
        public string gfx_name;
        public string gfx_vendor;
        public string deviceid;
        public int cpu_count;
        public float dpi;
        public string screen;
        public string appid;
        public int platform;
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
        public string install_mode;
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
            this.dpi = Screen.dpi;
            this.in_editor = Application.isEditor;
            this.platform = (int)Application.platform;
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
            this.install_mode = Application.installMode.ToString();
            this.app_install_store = Application.installerName;
            this.userid = AnalyticsSessionInfo.userId;
            this.sessionid = AnalyticsSessionInfo.sessionId;
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

    [RuntimeInitializeOnLoadMethod]
    static void InitializeTutorialManager()
    {
        Debug.Log(Application.installMode);
        Debug.Log(Application.installerName);
        Debug.Log(AnalyticsSessionInfo.userId);
        if (PlayerPrefs.HasKey(adaptiveOnboardingShowTutorialPrefsKey))
        {
            return;
        }
        var deviceInfo = new DeviceInfo();
        var advertisingSupported = Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string errorMsg) => {
            deviceInfo.adsid = advertisingId;
            deviceInfo.ads_tracking = trackingEnabled;
            CallTutorialManagerService(deviceInfo);
        });
        //If advertising is not supported on this platform, callback won't get fired. Call the tutorial manager service immediately.
        if (!advertisingSupported)
        {
            CallTutorialManagerService(deviceInfo);
        }
        //send device info
    }

    static void CallTutorialManagerService(DeviceInfo data)
    {
        WWWForm webForm = new WWWForm();
        webForm.AddField("appid", data.appid);
        webForm.AddField("app_ver", data.app_ver);
        webForm.AddField("model", data.model);
        webForm.AddField("deviceid", data.deviceid);
        webForm.AddField("ram", data.ram);
        webForm.AddField("cpu", data.cpu);
        webForm.AddField("cpu_count", data.cpu_count);
        webForm.AddField("gfx_name", data.gfx_name);
        webForm.AddField("gfx_vendor", data.gfx_vendor);
        webForm.AddField("screen", data.screen);
        webForm.AddField("dpi", data.dpi.ToString());
        webForm.AddField("in_editor", data.in_editor.ToString());
        webForm.AddField("platform", data.platform.ToString());
        webForm.AddField("os_ver", data.os_ver);
        webForm.AddField("gfx_shader", data.gfx_shader);

        //var webReq = new WWW()
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

        if (toShow)
        {
            Analytics.CustomEvent("tutorial_start", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
        }
        tutorialStep = 0;
        SetTutorialStep(tutorialStep);
        return toShow;
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
