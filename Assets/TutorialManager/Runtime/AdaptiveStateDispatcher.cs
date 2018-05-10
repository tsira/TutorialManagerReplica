using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics.TutorialManagerRuntime {
    public class AdaptiveStateDispatcher : IFSMDispatcher
	{
        public delegate void OnEnterStateHandler(string id);
        public delegate void OnExitStateHandler(string id);

        public event OnEnterStateHandler OnEnterState;
        public event OnExitStateHandler OnExitState;

        static AdaptiveStateDispatcher m_Instance;
        public static AdaptiveStateDispatcher GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = new AdaptiveStateDispatcher();
            }
            return m_Instance;
        }

        public void DispatchEnterState(string id)
        {
            if (OnEnterState != null) {
                OnEnterState(id);
            }
        }

        public void DispatchExitState(string id)
        {
            if (OnExitState != null) {
                OnExitState(id);
            }
        }
	}
}