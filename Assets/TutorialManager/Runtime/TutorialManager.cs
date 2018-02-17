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
    static string testName = "tutorial_test";
    static string tutorialKey = "show_tutorial";
    static float percentage_a = 0f;
    static float percentage_b = .2f;
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

    private class DeviceInfo
    {
        public string model;
        public int ram;
        public string cpu;
        public string gfx_name;
        public string gfx_vendor;
        public string device_id;
        public int cpu_count;
        public float dpi;
        public string screen;
        public string project_id;
        public int platform_id;
        public string os_ver;
        public int gfx_shader;
        public string gfx_ver;
        public int max_texture_size;
        public string app_build_version;
        public bool in_editor;
        public string network;
        public string screenOrientation;
        public float realtimeSinceStartup;
        public float batterLevel;

        public DeviceInfo(string projectId /*, string app_build_version*/)
        {
            this.project_id = projectId;
            //this.app_build_version = app_build_version;
            this.model = GetDeviceModel();
            this.device_id = SystemInfo.deviceUniqueIdentifier;
            this.ram = SystemInfo.systemMemorySize;
            this.cpu = SystemInfo.processorType;
            this.cpu_count = SystemInfo.processorCount;
            this.gfx_name = SystemInfo.graphicsDeviceName;
            this.gfx_vendor = SystemInfo.graphicsDeviceVendor;
            this.screen = Screen.currentResolution.ToString();
            this.dpi = Screen.dpi;
            this.in_editor = Application.isEditor;
            this.platform_id = (int)Application.platform;
            this.os_ver = SystemInfo.operatingSystem;
            this.gfx_shader = SystemInfo.graphicsShaderLevel;
            this.gfx_ver = SystemInfo.graphicsDeviceVersion;
            this.max_texture_size = SystemInfo.maxTextureSize;
            this.network = Application.internetReachability.ToString();
            this.screenOrientation = Screen.orientation.ToString();
            this.realtimeSinceStartup = Time.realtimeSinceStartup;
            this.batterLevel = SystemInfo.batteryLevel;
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
    static void InitializeRemoteSettingsHandler ()
    {
        RemoteSettings.Updated += RemoteSettings_Updated;
    }

    static void RemoteSettings_Updated()
    {
        //player has already been allocated. Do nothing.
        if(PlayerPrefs.HasKey("adaptive_onboarding_show_tutorial"))
        {
            return;
        }
        bool playerInTestGroup = RemoteSettings.GetBool("adapative_onboarding_test_group", false);
        if(playerInTestGroup)
        {
            //send device info
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
        string bucket = controlGroupValue;
        bool playerInTestGroup = RemoteSettings.GetBool("adapative_onboarding_test_group", false);
        if(playerInTestGroup)
        {
            toShow = PlayerPrefs.GetInt("adaptive_onboarding_show_tutorial") == 1;
            bucket = testGroupValue;
        }

        HandleAdaptiveOnboardingEvent(toShow, bucket);
        
        if (toShow)
        {
            Analytics.CustomEvent("tutorial_start", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
        }
        tutorialStep = 0;
        SetTutorialStep(tutorialStep);
        return toShow;
    }

    static void HandleAdaptiveOnboardingEvent (bool toShow, string bucket)
    {
        adaptiveOnboardingEventSent = PlayerPrefs.GetInt(adaptiveOnboardingSentPrefsKey, adaptiveOnboardingEventSent);
        if(adaptiveOnboardingEventSent == 0) 
        {
            Analytics.CustomEvent(adaptiveOnboardingEventName,
                new Dictionary<string, object>{
                    { tutorialOnKey, toShow },
                    { tutorialTestGroupKey, bucket}
                }
            );
            adaptiveOnboardingEventSent = 1;
            PlayerPrefs.SetInt(adaptiveOnboardingSentPrefsKey, adaptiveOnboardingEventSent);
            PlayerPrefs.Save();
        }
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
