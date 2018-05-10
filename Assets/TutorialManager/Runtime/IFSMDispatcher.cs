using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    public interface IFSMDispatcher
    {
        event AdaptiveStateDispatcher.OnEnterStateHandler OnEnterState;
        event AdaptiveStateDispatcher.OnExitStateHandler OnExitState;

        void DispatchEnterState(string id);

        void DispatchExitState(string id);
    }
}