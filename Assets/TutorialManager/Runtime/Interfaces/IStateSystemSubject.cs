using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;

namespace UnityEngine.Analytics
{
    public interface IStateSystemSubject
    {
        IFSMDispatcher dispatcher { get; set;  }
        IDataStore dataStore { get; set; }
    }
}
