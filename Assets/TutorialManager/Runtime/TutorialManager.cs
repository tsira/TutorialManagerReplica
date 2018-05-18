using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Analytics.TutorialManagerRuntime;

/// <summary>
/// Tutorial manager configures the ABTester for the Tutorial test,
/// and encapsulates some key functionality.
/// </summary>

namespace UnityEngine.Analytics
{
    public class TutorialManager
    {

#pragma warning disable 414
        public readonly static string k_PluginVersion = "0.1.0";

        static int adaptiveOnboardingEventSent = 0;
        static int tutorialStep = 0;
        static string tutorialKey = "show_tutorial";

        const string adaptiveOnboardingEventName = "adaptive_onboarding";
        const string controlGroupValue = "control";
        const string testGroupValue = "test";
        const string tutorialOnKey = "tutorial_on";
        const string tutorialTestGroupKey = "test_group";
        const string tutorialIdKey = "tutorial_id";

        const string adaptiveOnboardingSentPrefsKey = "adaptive_onboarding_event_sent";
        const string adaptiveOnboardingShowTutorialPrefsKey = "adaptive_onboarding_show_tutorial";
        const string tutorialStepPlayerPrefsKey = "unity_analytics_tutorial_test_current_step";

#pragma warning restore 414

        public class ValuesJSONParser
        {
            public bool app_installed;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeTutorialManager()
        {
            BootstrapAdaptiveContent();

            string analyticsLocation = GetAnalyticsValuesLocation();
            if (File.Exists(analyticsLocation))
            {
                var fileText = File.ReadAllText(analyticsLocation);
                if(string.IsNullOrEmpty(fileText) == false)
                {
                    if (JsonUtility.FromJson<ValuesJSONParser>(fileText).app_installed == true)
                    {
                        return;
                    }
                }
            }
            if (PlayerPrefs.HasKey(adaptiveOnboardingShowTutorialPrefsKey))
            {
                return;
            }
            DecisionRequestService.OnDecisionReceived += DecisionRequestService_OnDecisionReceived;
            DecisionRequestService.RequestDecision();
        }

        static void DecisionRequestService_OnDecisionReceived(TutorialWebResponse response)
        {
            DecisionRequestService.OnDecisionReceived -= DecisionRequestService_OnDecisionReceived;
            PlayerPrefs.SetInt(adaptiveOnboardingShowTutorialPrefsKey, response.showTutorial ? 1 : 0);
            PlayerPrefs.Save();
            TutorialManagerModelMiddleware.GetInstance().UpdateContentEntityValues(response.contentTable);
        }

        private static string GetAnalyticsValuesLocation()
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
        /// Send standard events tracking the user's tutorial progress.
        /// </summary>
        private static void HandleAdaptiveOnboardingEvent(bool toShow)
        {
            adaptiveOnboardingEventSent = PlayerPrefs.GetInt(adaptiveOnboardingSentPrefsKey, adaptiveOnboardingEventSent);
            if (adaptiveOnboardingEventSent == 0)
            {
                if (toShow)
                {
                    Analytics.CustomEvent("tutorial_start", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
                }
                Analytics.CustomEvent(adaptiveOnboardingEventName,
                    new Dictionary<string, object>{
                    { tutorialOnKey, toShow }
                    }
                );
                adaptiveOnboardingEventSent = 1;
                PlayerPrefs.SetInt(adaptiveOnboardingSentPrefsKey, adaptiveOnboardingEventSent);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Determine whether to show the tutorial.
        /// </summary>
        /// <remarks>
        ///     <code>
        ///     if (TutorialManager.ShowTutorial()) {
        ///         // show the tutorial
        ///     } else {
        ///         // skip the tutorial
        ///     }
        ///     </code>
        /// </remarks>
        /// <returns><c>true</c>, if tutorial should be shown, <c>false</c> otherwise.</returns>
        public static bool ShowTutorial()
        {
            bool toShow = true;
            toShow = PlayerPrefs.GetInt(adaptiveOnboardingShowTutorialPrefsKey, 1) == 1;
            tutorialStep = 0;
            SetTutorialStep(tutorialStep);
            HandleAdaptiveOnboardingEvent(toShow);
            return toShow;
        }

        /// <summary>
        /// Overload of ShowTutorial with a bool passed in that will be returned as the decision. Used to mock
        /// the result of ShowTutorial for QA purposes. Please do not push this to 'live' builds.
        /// </summary>
        /// <remarks>
        ///     <code>
        ///     if (TutorialManager.ShowTutorial(true)) {
        ///         // show the tutorial
        ///     } else {
        ///         // skip the tutorial
        ///     }
        ///     </code>
        /// </remarks>
        /// <param name="overrideDecision">Value that will be returned by this method for QA.</param>
        /// <returns><c>true</c>, if tutorial should be shown, <c>false</c> otherwise.</returns>
        public static bool ShowTutorial(bool overrideDecision)
        {
            HandleAdaptiveOnboardingEvent(overrideDecision);
            tutorialStep = 0;
            SetTutorialStep(tutorialStep);
            return overrideDecision;
        }

        /// <summary>
        /// Call this when the player completes the tutorial.
        /// </summary>
        public static AnalyticsResult CompleteTutorial()
        {
            return Analytics.CustomEvent("tutorial_complete", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
        }

        /// <summary>
        /// Call this if the player skips the tutorial.
        /// </summary>
        public static AnalyticsResult SkipTutorial()
        {
            return Analytics.CustomEvent("tutorial_skip", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
        }

        /// <summary>
        /// Call this each time the player advances a step in the tutorial.
        /// </summary>
        public static AnalyticsResult AdvanceTutorial()
        {
            tutorialStep = GetTutorialStep();
            tutorialStep++;
            SetTutorialStep(tutorialStep);
            return Analytics.CustomEvent("tutorial_step", new Dictionary<string, object> {
            { tutorialIdKey, tutorialKey },
            { "step_index", tutorialStep }
        });
        }

        /// <summary>
        /// Helper Method for AdvanceTutorial().
        /// </summary>
        static void SetTutorialStep(int newTutorialStep)
        {
            PlayerPrefs.SetInt(tutorialStepPlayerPrefsKey, newTutorialStep);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Helper Method for AdvanceTutorial().
        /// </summary>
        static int GetTutorialStep()
        {
            if (PlayerPrefs.HasKey(tutorialStepPlayerPrefsKey))
            {
                return tutorialStep = PlayerPrefs.GetInt(tutorialStepPlayerPrefsKey);
            }
            return 0;
        }

        static void BootstrapAdaptiveContent()
        {
            var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
            var dispatcher = AdaptiveStateDispatcher.GetInstance();

            var fsm = new TutorialManagerFSM();
            // TODO: this is a default case...but we need to address again if a user is not at the start.
            fsm.stateList = modelMiddleware.TMData.tutorials[0].steps;
            fsm.dispatcher = dispatcher;

            var provider = StateSystemProvider.GetInstance();
            provider.SetDispatcher(dispatcher);
            provider.SetDataStore(modelMiddleware);
        }

    }
}