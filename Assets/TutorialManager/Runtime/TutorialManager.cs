//#define TEXTMESHPRO_PRESENT

using System.Collections.Generic;
using System.IO;
using UnityEngine.Analytics.TutorialManagerRuntime;
using System;

namespace UnityEngine.Analytics
{
    /// <summary>
    /// A set of methods for managing your game's tutorial
    /// </summary>
    /// <remarks>
    /// Unity Tutorial Manager (beta) leverages Unity Analytics and Remote Settings to help you build and tune your game's
    /// onboarding experience. The ultimate objective is to help you create best-in-class tutorials for your game by
    /// optimizing the entire flow. For certain aspects of your tutorial, this means picking the singular best experience.
    /// In some cases, the tool can actually deliver different experiences to different players, such that the tutorial
    /// is optimized differently based on the different needs of each.
    /// </remarks>
    public class TutorialManager
    {

        static TutorialManagerState m_State;
        /// <summary>
        /// The current state of the Tutorial Manager.
        /// </summary>
        /// <value>A reference to the <c>TutorialManagerState</c> object.</value>
        public static TutorialManagerState state {
            get {
                return m_State;
            }
        }

        /// <summary>
        /// Retrieves the show/no show recommendation from the Tutorial Manager Server.
        /// </summary>
        /// <remarks>
        /// This method is the lynchpin of the TutorialManager system. It should be called when the player arrives
        /// at the first tutorial. Calling it lets you know what recommendation the server has for this player,
        /// and informs the server that this recommendation has been acted upon.
        ///     <code>
        ///     if (TutorialManager.GetDecision()) {
        ///         TutorialManager.TutorialStart("Tutorial1");
        ///         // Any other code required by your tutorial
        ///     } else {
        ///         // skip the tutorial
        ///     }
        ///     </code>
        /// </remarks>
        /// <returns><c>true</c>, if the tutorial should be shown, <c>false</c> otherwise.</returns>
        public static bool GetDecision()
        {
            HandleAdaptiveOnboardingEvent(m_State.showTutorial);
            return m_State.showTutorial;
        }

        /// <summary>
        /// Starts a tutorial.
        /// </summary>
        /// <remarks>
        ///     <code>
        ///     if (TutorialManager.GetDecision()) {
        ///         TutorialManager.TutorialStart("Tutorial1");
        ///     } else {
        ///         // skip the tutorial
        ///     }
        ///     </code>
        /// </remarks>
        /// <param name="tutorialId">The binding id representing the current tutorial</param>
        /// <param name="autoAdvance">If 'true' (default) ending one tutorial step will automatically advance to the next step</param>
        /// <returns><c>true</c>, if tutorial should be shown, <c>false</c> otherwise.</returns>
        public static bool TutorialStart(string tutorialId, bool autoAdvance = true)
        {
            return TutorialStart(tutorialId, autoAdvance, m_State.showTutorial);
        }

        /// <summary>
        /// Starts a tutorial.
        /// </summary>
        /// <remarks>
        /// This version takes a boolean <c>toShow</c> parameter, which forces either true or false.
        /// This is useful for testing and QA, when you want to ensure a specific outcome. To use the server
        /// recommendation, call <c>ShowTutorial(id)</c>
        ///     <code>
        ///     if (TutorialManager.GetDecision()) {
        ///         TutorialManager.TutorialStart("Tutorial1", true, true);
        ///     } else {
        ///         // skip the tutorial (since the override in this example is 'true', this code is unreachable)
        ///     }
        ///     </code>
        /// </remarks>
        /// <param name="tutorialId">The binding id representing the current tutorial</param>
        /// <param name="autoAdvance">If <c>true</c> (default) ending one tutorial step will automatically advance to the next step</param>
        /// <param name="toShow"><c>true</c> to force the tutorial to start, <c>false</c> to force it to skip</param>
        /// <returns><c>true</c>, if tutorial should be shown, <c>false</c> otherwise.</returns>
        public static bool TutorialStart(string tutorialId, bool autoAdvance, bool toShow)
        {
            SetupState(tutorialId, autoAdvance, toShow);
            SaveState();
            Analytics.CustomEvent(tutorialStartEventName, new Dictionary<string, object> {
                { tutorialIdKey, m_State.tutorialId }
            });
            return GetDecision();
        }

        /// <summary>
        /// Call this if the player skips the tutorial.
        /// </summary>
        /// <remarks>
        /// This will clear the current tutorial state
        /// </remarks>
        public static void TutorialSkip()
        {
            Analytics.CustomEvent(tutorialSkipName, new Dictionary<string, object> {
                { tutorialIdKey, m_State.tutorialId },
                { stepIndexKey, m_State.currentStep }
            });
            ResolveTutorial(false);
        }

        /// <summary>
        /// Call this each time the player starts or completes a step in the tutorial.
        /// </summary>
        /// <remarks>
        /// If <c>autoAdvance</c> is true, call this ONLY at the completion of a step. The next step, if one exists, will
        /// start automatically.
        /// </remarks>
        public static void TutorialStep()
        {
            m_State.fsm.NextState();
            SaveState();
        }

        /// <summary>
        /// Call this method if you need to go to a state out of sequential order.
        /// </summary>
        /// <remarks>
        /// This method is supplied as a convenience, but may have negative effects on your tracking funnel.
        /// </remarks>
        public static void GotoTutorialStep(string state)
        {
            m_State.fsm.GoToState(state);
            SaveState();
        }

        /// <summary>
        /// Clears the runtime TutorialManager state.
        /// </summary>
        /// <remarks>
        /// This method is provided for QA and testing purposes only. Calling it will null out all prior decisions and
        /// runtime progress on the current device.
        /// </remarks>
        public static void ResetState()
        {
            var fsm = m_State.fsm;
            fsm.Reset();
            m_State = new TutorialManagerState(fsm);
            SaveState();
        }

        // End public methods /// /////////////////////////////////

        public readonly static string k_PluginVersion = "0.1.0";

        // Event names
        const string adaptiveOnboardingEventName = "adaptive_onboarding";
        const string tutorialStartEventName = "tutorial_start";
        const string tutorialStepStartEventName = "tutorial_step_start";
        const string tutorialStepCompleteEventName = "tutorial_step_complete";
        const string tutorialCompleteEventName = "tutorial_complete";
        const string tutorialSkipName = "tutorial_skip";

        // Parameter keys
        const string tutorialOnKey = "tutorial_on";
        const string tutorialTestGroupKey = "test_group";
        const string tutorialIdKey = "tutorial_id";
        const string stepIdKey = "step_id";
        const string stepIndexKey = "step_index";


        /// <summary>
        /// Retrieve recommendation/settings from server. Called automatically on startup.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        static void InitializeTutorialManager()
        {
            BootstrapAdaptiveContent();
            ReadState();
            if (RequiresDecision()) {
                DecisionRequestService.OnDecisionReceived += DecisionRequestService_OnDecisionReceived;
                DecisionRequestService.RequestDecision();
            }
        }

        /// <summary>
        /// Handles the Decision server response
        /// </summary>
        /// <param name="response">A TutorialWebResponse object.</param>
        static void DecisionRequestService_OnDecisionReceived(TutorialWebResponse response)
        {
            DecisionRequestService.OnDecisionReceived -= DecisionRequestService_OnDecisionReceived;
            m_State.showTutorial = response.showTutorial;
            m_State.decisionReceived = true;
            SaveState();
            TutorialManagerModelMiddleware.GetInstance().UpdateContentEntityValues(response.contentTable);
        }

        /// <summary>
        /// Determines whether we need to ask the server for a decision for this instance
        /// </summary>
        /// <returns><c>true</c>, if decision is required, <c>false</c> otherwise.</returns>
        static bool RequiresDecision()
        {
            // Do we have a prior recorded decision?
            if (m_State.decisionReceived) {
                return false;
            }
            // Is this a pre-existing player?
            string analyticsLocation = GetAnalyticsValuesLocation();
            if (File.Exists(analyticsLocation)) {
                var fileText = File.ReadAllText(analyticsLocation);
                if (string.IsNullOrEmpty(fileText) == false) {
                    if (JsonUtility.FromJson<ValuesJSONParser>(fileText).app_installed == true) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Constructs and returns the path where analytics values are stored.
        /// </summary>
        /// <returns>Returns the path where analytics values are stored.</returns>
        static string GetAnalyticsValuesLocation()
        {
            string retv;
#if UNITY_TVOS
            retv = Application.temporaryCachePath;
#else
            retv = Application.persistentDataPath;
#endif
            retv = Path.Combine(retv, "Unity");
            retv = Path.Combine(retv, Application.cloudProjectId);
            retv = Path.Combine(retv, "Analytics");
            retv = Path.Combine(retv, "values");
            return retv;
        }

        /// <summary>
        /// Establish the initial state of the TutorialManager and the Finite State Machine.
        /// </summary>
        /// <param name="tutorialId">The binding identifier for this tutorial.</param>
        /// <param name="autoAdvance">If set to <c>true</c> the finite state machine will auto advance.</param>
        /// <param name="toShow">If set to <c>true</c> the tutorial is to be shown, <c>false</c> otherwise.</param>
        static void SetupState(string tutorialId, bool autoAdvance, bool toShow)
        {
            if (TutorialManagerModelMiddleware.GetInstance().TMData.tutorialTable.ContainsKey(tutorialId)) {
                m_State.fsm.stateList = TutorialManagerModelMiddleware.GetInstance().TMData.tutorialTable[tutorialId].steps;
                m_State.fsm.autoAdvance = autoAdvance;
                m_State.showTutorial = toShow;
                m_State.tutorialId = tutorialId;
                m_State.fsm.GoToState(m_State.fsm.stateList[0]);
            }
        }

        /// <summary>
        /// Saves the runtime state to disk.
        /// </summary>
        static void SaveState()
        {
            TutorialManagerSaveableState saveState = new TutorialManagerSaveableState(m_State);
            TMSerializer.WriteToDisk<TutorialManagerSaveableState>(ref saveState);
        }

        /// <summary>
        /// Reads the runtime state from disk.
        /// </summary>
        static void ReadState()
        {
            TutorialManagerSaveableState saveState = new TutorialManagerSaveableState();
            TMSerializer.ReadFromDisk<TutorialManagerSaveableState>(ref saveState);
            m_State.RestoreFromSavableState(saveState);
        }

        /// <summary>
        /// Sets up the various components of the Tutorial Manager system
        /// </summary>
        static void BootstrapAdaptiveContent()
        {
            var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
            var dispatcher = AdaptiveStateDispatcher.GetInstance();

            dispatcher.OnEnterState += FSM_EnterState;
            dispatcher.OnExitState += FSM_ExitState;

            var fsm = new TutorialManagerFSM();
            fsm.dispatcher = dispatcher;
            m_State = new TutorialManagerState(fsm);

            string cueTutorial = m_State.tutorialId;
            if (string.IsNullOrEmpty(cueTutorial)) {
                if (modelMiddleware.TMData.tutorials.Count == 0) {
                    modelMiddleware.CreateTutorialEntity("EmptyTutorial");
                    modelMiddleware.CreateStepEntity("EmptyStep", "EmptyTutorial");
                }
                cueTutorial = modelMiddleware.TMData.tutorials[0].id;
            }
            fsm.stateList = modelMiddleware.TMData.tutorialTable[cueTutorial].steps;

            var provider = StateSystemProvider.GetInstance();
            provider.SetDispatcher(dispatcher);
            provider.SetDataStore(modelMiddleware);
        }

        /// <summary>
        /// Handles analytics when enterring a tutorial step
        /// </summary>
        /// <param name="id">The binding ID for the step enterred.</param>
        static void FSM_EnterState(string id)
        {
            Analytics.CustomEvent(tutorialStepStartEventName, new Dictionary<string, object> {
                { tutorialIdKey, m_State.tutorialId },
                { stepIdKey, m_State.currentStep },
                { stepIndexKey, m_State.stepIndex }
            });
        }

        /// <summary>
        /// Handles analytics when exiting a tutorial step
        /// </summary>
        /// <param name="id">The binding ID for the step exited.</param>
        static void FSM_ExitState(string id)
        {
            Analytics.CustomEvent(tutorialStepCompleteEventName, new Dictionary<string, object> {
                { tutorialIdKey, m_State.tutorialId },
                { stepIdKey, m_State.currentStep },
                { stepIndexKey, m_State.stepIndex }
            });

            if (m_State.fsm.complete) {
                ResolveTutorial(true);
            }
        }

        /// <summary>
        /// Marks the tutorial as resolved
        /// </summary>
        /// <param name="isComplete">If <c>true</c> the tutorial completed successfully, otherwise it was skipped.</param>
        static void ResolveTutorial(bool isComplete)
        {
            if (isComplete) {
                Analytics.CustomEvent(tutorialCompleteEventName, new Dictionary<string, object> {
                        { tutorialIdKey, m_State.tutorialId }
                    });
            }
            m_State.tutorialId = null;
            m_State.fsm.Reset();
        }

        /// <summary>
        /// Send an event to confirm that server recommendation was acted upon.
        /// </summary>
        /// <param name="toShow"><c>true</c> if the tutorial is to be shown, <c>false</c> otherwise</param>
        static void HandleAdaptiveOnboardingEvent(bool toShow)
        {
            if (m_State.adaptiveOnboardingEventSent == false) {
                m_State.adaptiveOnboardingEventSent = true;
                Analytics.CustomEvent(adaptiveOnboardingEventName, new Dictionary<string, object> { { tutorialOnKey, toShow } });
            }
        }
    }

    internal class ValuesJSONParser
    {
#pragma warning disable 649
        public bool app_installed;
#pragma warning restore 649
    }
}