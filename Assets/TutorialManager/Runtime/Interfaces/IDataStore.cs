using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics
{
    public interface IDataStore
    {
        string GetString(string id, string defaultValue = null);
    }
}