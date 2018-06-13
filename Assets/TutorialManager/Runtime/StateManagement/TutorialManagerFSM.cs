using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{

    public class TutorialManagerFSM
    {
        public IFSMDispatcher dispatcher;

        List<string> m_StateList = new List<string>();
        public List<string> stateList
        {
            get
            {
                return m_StateList;
            }
            set
            {
                m_Complete = false;
                m_StateList = value;
            }
        }

        bool m_Complete = false;
        public bool complete {
            get {
                return m_Complete;
            }
        }

        string m_NextState;
        string m_State;
        public string state
        {
            get
            {
                return m_State;
            }
        }

        public int index {
            get {
                return stateList.IndexOf(state);
            }
        }

        public int length {
            get {
                return stateList.Count;
            }
        }

        bool m_AutoAdvance = true;
        public bool autoAdvance
        {
            get
            {
                return m_AutoAdvance;
            }
            set
            {
                m_AutoAdvance = value;
            }
        }

        public TutorialManagerFSM(bool shouldAutoAdvance = true)
        {
            autoAdvance = shouldAutoAdvance;
        }

        public void GoToState(string newState)
        {
            // Is there a statelist?
            if (stateList.Count == 0)
            {
                Debug.LogWarning("GoToState called with no state list");
                return;
            }

            m_Complete = false;

            // Are we in a state?
            if (string.IsNullOrEmpty(state) == false)
            {
                if (stateList.Contains(state))
                {
                    ExitState(state);
                }
            }

            // Does the new state exist?
            if (stateList.Contains(newState))
            {
                EnterState(newState);
            }
        }

        public void NextState()
        {
            // Is there a statelist?
            if (stateList.Count == 0)
            {
                Debug.LogWarning("NextState called with no state list");
                return;
            }

            if (m_Complete) {
                return;
            }

            // Does next state exist?
            if (index < stateList.Count - 1)
            {
                if (string.IsNullOrEmpty(m_NextState) == false) {
                    GoToState(m_NextState);
                    m_NextState = null;
                }
                else if (autoAdvance || index == -1) {
                    GoToState(stateList[index + 1]);
                }
                else {
                    ExitState(state, stateList[index + 1]);
                }
            }
            else
            {
                // If not move out. We're done.
                m_Complete = true;
                m_NextState = null;
                ExitState(state);
                m_State = null;
            }
        }

        public void SkipAndResolve()
        {
            m_NextState = null;
            m_State = null;
        }

        public void Reset()
        {
            autoAdvance = true;
            m_Complete = false;
            m_State = null;
            m_NextState = null;
            m_StateList = new List<string>();
        }

        protected void EnterState(string id)
        {
            m_State = id;
            dispatcher.DispatchEnterState(state);
        }

        protected void ExitState(string id, string cueUpNext = null)
        {
            if (string.IsNullOrEmpty(cueUpNext) == false) {
                m_NextState = cueUpNext;
            }
            dispatcher.DispatchExitState(state);
            m_State = null;
        }
    }
}