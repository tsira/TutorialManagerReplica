using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [Serializable]
    public struct RemoteSettingsKeyValueType
    {
        public string key;
        public string value;
        public string type;

        public RemoteSettingsKeyValueType(string k, string v, string t)
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
        public List<RemoteSettingsKeyValueType> remoteSettings;
    }
}
