using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics
{
    public interface IFSMDispatcher
    {
        string state { get; }

        event AdaptiveStateDispatcher.OnEnterStateHandler OnEnterState;
        event AdaptiveStateDispatcher.OnExitStateHandler OnExitState;

        void DispatchEnterState(string id);

        void DispatchExitState(string id);
    }
}