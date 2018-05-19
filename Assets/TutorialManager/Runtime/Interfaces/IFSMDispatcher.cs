using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics
{
    public delegate void OnEnterStateHandler(string id);
    public delegate void OnExitStateHandler(string id);

    public interface IFSMDispatcher
    {
        string state { get; }

        event OnEnterStateHandler OnEnterState;
        event OnExitStateHandler OnExitState;

        void DispatchEnterState(string id);

        void DispatchExitState(string id);
    }
}