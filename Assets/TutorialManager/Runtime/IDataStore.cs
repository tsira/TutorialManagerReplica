using UnityEngine;
using System.Collections;
using System;

namespace UnityEngine.Analytics
{
    public delegate void OnDataUpdateHandler();


    public interface IDataStore
    {
        string GetString(string id, string defaultValue = null);

        bool GetBool(string id, bool defaultValue = false);

        event OnDataUpdateHandler OnDataUpdate; 
    }
}