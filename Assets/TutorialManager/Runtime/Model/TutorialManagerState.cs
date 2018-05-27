namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    /// <summary>
    /// The state of all Tutorial Manager-related decisions and progress on this device.
    /// </summary>
    public class TutorialManagerState
    {
        TutorialManagerFSM m_FSM;

        /// <summary>
        /// [Read Only] A reference to the Finite State Machine that drives the Tutorial Manager
        /// </summary>
        /// <value>A reference to the Finite State Machine.</value>
        public TutorialManagerState(TutorialManagerFSM fsm)
        {
            m_FSM = fsm;
        }

        /// <summary>
        /// Utility method to restore saved state at runtime. 
        /// </summary>
        /// <param name="state">The saved state.</param>
        internal void RestoreFromSavableState(TutorialManagerSaveableState state)
        {
            showTutorial = state.showTutorial;
            fsm.autoAdvance = state.autoAdvance;
            decisionReceived = state.decisionReceived;
            adaptiveOnboardingEventSent = state.adaptiveOnboardingEventSent;
            tutorialId = state.tutorialId;
            fsm.GoToState(state.currentStep);
        }

        /// <summary>
        /// [ReadOnly] The Finite State Machine that drives the Tutorial Manager
        /// </summary>
        /// <value>A reference to the Finite State Machine.</value>
        public TutorialManagerFSM fsm {
            get {
                return m_FSM;
            }
        }

        bool m_ShowTutorial;
        /// <summary>
        /// Reflects the server recommendation as to whether the player should see the tutorial.
        /// </summary>
        /// <value><c>true</c> if the tutorial should be shown; otherwise, <c>false</c>.</value>
        public bool showTutorial {
            get {
                return m_ShowTutorial;
            }
            internal set {
                m_ShowTutorial = value;
            }
        }

        string m_TutorialId;
        /// <summary>
        /// The ID of the current tutorial, as set by <c>TutorialManager.StartTutorial(tutorialId)</c>
        /// </summary>
        /// <value>The tutorial identifier.</value>
        public string tutorialId {
            get {
                return m_TutorialId;
            }
            internal set {
                m_TutorialId = value;
            }
        }

        /// <summary>
        /// The binding ID of the current tutorial step.
        /// </summary>
        /// <value>A string representing the binding ID of the current step.</value>
        public string currentStep {
            get {
                return fsm.state;
            }
        }

        /// <summary>
        /// The number of steps in the current tutorial.
        /// </summary>
        /// <value>The length of the tutorial.</value>
        public int tutorialLength {
            get {
                return fsm.length;
            }
        }

        /// <summary>
        /// The index of the step in the currently running tutorial.
        /// </summary>
        /// <value>The index of the step.</value>
        public int stepIndex {
            get {
                return fsm.index;
            }
        }

        /// <summary>
        /// A flag indicating whether the current tutorial is complete.
        /// </summary>
        /// <value><c>true</c> if the current tutorial is marked as complete; otherwise, <c>false</c>.</value>
        public bool complete {
            get {
                return fsm.complete;
            }
        }

        /// <summary>
        /// A flag indicating whether the tutorial should automatically progress from one step to the next.
        /// </summary>
        /// <remarks>
        /// By default, the Tutorial Manager will progress from one step to the next automatically. This is often
        /// desirable, but not every tutorial works this way. If your game's tutorial has steps interspersed with
        /// gameplay, you might want more precise control. By calling <c>StartTutorial(tutorialId, false)</c>, this flag
        /// will be set to <c>false</c>. The result is that calling <c>TutorialStep()</c> will complete a step,
        /// but not automatically start the next one. This allows gameplay to proceed with the tutorial "paused" between
        /// states. You will need to call <c>TutorialStep()</c> again to start the next step.
        /// </remarks>
        /// <value><c>true</c> if auto advance; otherwise, <c>false</c>.</value>
        public bool autoAdvance {
            get {
                return fsm.autoAdvance;
            }
        }

        bool m_DecisionReceived;
        /// <summary>
        /// Indicates whether the current device has received a decision from the Tutorial Manager server.
        /// </summary>
        /// <value><c>true</c> if decision has been received; otherwise, <c>false</c>.</value>
        internal bool decisionReceived {
            get {
                return m_DecisionReceived;
            }
            set {
                m_DecisionReceived = value;
            }
        }

        bool m_AdaptiveOnboardingEventSent;
        /// <summary>
        /// Indicates whether the current device has reached any decision point.
        /// </summary>
        /// <remarks>
        /// When Tutorial Manager first initializes, it requests a decision from the Tutorial Manager server.
        /// This event is sent the first time <c>TutorialStart()</c> is called to indicate whether that decision
        /// was ever actually acted upon.
        /// </remarks>
        /// <value><c>true</c> if adaptive onboarding event sent; otherwise, <c>false</c>.</value>
        internal bool adaptiveOnboardingEventSent {
            get {
                return m_AdaptiveOnboardingEventSent;
            }
            set {
                m_AdaptiveOnboardingEventSent = value;
            }
        }
    }
}