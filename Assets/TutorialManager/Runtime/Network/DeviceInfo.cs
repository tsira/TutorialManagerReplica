using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    internal class DeviceInfo
    {

        public bool ads_tracking;
        public bool in_editor;

        public float battery_level;
        public float realtime_since_startup;

        public int cpu_count;
        public int dpi;
        public int gfx_shader;
        public int max_texture_size;
        public int ram;
        public int platformid;

        public long sessionid;

        public string adsid;
        public string appid;
        public bool debug_device;
        public string app_install_mode;
        public string app_install_store;
        public string app_ver;
        public string battery_status;
        public string build_guid;
        public string cpu;
        public string device_name;
        public string deviceid;
        public string gfx_name;
        public string gfx_vendor;
        public string gfx_ver;
        public string lang;
        public string model;
        public string network;
        public string os_ver;
        public string platform;
        public string screen;
        public string screen_orientation;
        public string userid;

        internal DeviceInfo()
        {
            this.appid = Application.cloudProjectId;
            this.debug_device = Debug.isDebugBuild;
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
#if UNITY_5_6_OR_NEWER
            this.battery_level = SystemInfo.batteryLevel;
            this.battery_status = SystemInfo.batteryStatus.ToString();
#endif
            this.lang = Application.systemLanguage.ToString();
#if UNITY_5_6_OR_NEWER
            this.build_guid = Application.buildGUID;
#endif
            this.app_install_mode = Application.installMode.ToString();
            this.app_install_store = Application.installerName;

#if UNITY_2017_2_OR_NEWER
            // Adjusting for 2017.02+ Analytics Namespace
            this.userid = AnalyticsSessionInfo.userId;
            this.sessionid = AnalyticsSessionInfo.sessionId;
#else
            this.userid = PlayerPrefs.GetString("unity.cloud_userid");
            if (PlayerPrefs.HasKey("unity.player_sessionid"))
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
        // Get manufacturer, model, and device
        AndroidJavaClass jc = new AndroidJavaClass("android.os.Build");
        string manufacturer = jc.GetStatic<string>("MANUFACTURER");
        string model = jc.GetStatic<string>("MODEL");
        string device = jc.GetStatic<string>("DEVICE");
        return string.Format("{0}/{1}/{2}", manufacturer, model, device);
#else
            return SystemInfo.deviceModel;
#endif
        }
    }
}
