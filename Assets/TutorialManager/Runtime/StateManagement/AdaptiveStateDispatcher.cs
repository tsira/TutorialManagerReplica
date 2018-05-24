using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics {
    public class AdaptiveStateDispatcher : IFSMDispatcher
	{
        public event OnEnterStateHandler OnEnterState;
        public event OnExitStateHandler OnExitState;

        static AdaptiveStateDispatcher m_Instance;

        string m_State;
        public string state {
            get {
                return m_State;
            }
            private set {
                m_State = value;
            }
        }

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
            state = id;
            if (OnEnterState != null) {
                OnEnterState(id);
            }
        }

        public void DispatchExitState(string id)
        {
            state = string.Empty;
            if (OnExitState != null) {
                OnExitState(id);
            }
        }
	}
}