using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [Serializable]
    public struct TMRemoteSettingsKeyValueType
    {
        public string key;
        public string value;
        public string type;

        public TMRemoteSettingsKeyValueType(string k, string v, string t)
        {
            key = k;
            value = v;
            type = t;
        }
    }

    [Serializable]
    public struct RemoteSettingsData
    {
        public string genre;
        public List<TMRemoteSettingsKeyValueType> remoteSettings;
    }
}
