using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics.TutorialManagerRuntime {
    public class AdaptiveStateDispatcher : IFSMDispatcher
	{
        public delegate void OnEnterState(string id);
        public delegate void OnExitState(string id);



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
        }

        public void DispatchExitState(string id)
        {
        }
	}
}