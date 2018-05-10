using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    public interface IFSMDispatcher
    {
        void DispatchEnterState(string id);

        void DispatchExitState(string id);
    }
}