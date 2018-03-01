using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;

/// <summary>
/// Tutorial manager configures the ABTester for the Tutorial test,
/// and encapsulates some key functionality.
/// </summary>
public class TutorialManager
{
    static string testName = "tutorial_test";
    static string tutorialKey = "show_tutorial";
    static float percentage_a = 0f;
    static float percentage_b = .2f;
    static int tutorialStep = 0;

    const string tutorialIdKey = "tutorial_id";
    const string tutorialStepPlayerPrefsKey = "unity_analytics_tutorial_test_current_step";
    const string adaptiveOnboardingEventName = "adaptive_onboarding";
    const string tutorialOnKey = "tutorial_on";
    const string tutorialTestGroupKey = "test_group";
    const string testGroupValue = "test";
    const string controlGroupValue = "control";
    const string adaptiveOnboardingSentPrefsKey = "adaptive_onboarding_event_sent";
    static int adaptiveOnboardingEventSent = 0;

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
        ABTestingWrapper.Configure(testName, percentage_a, percentage_b);
        ABTestingWrapper.EnsureBucket();
        string bucket = PlayerPrefs.GetString("unity_analytics_ab_test_bucket");
        bool tutorialValue = (bucket == "_b") ? false : true;

        bool toShow = ABTestingWrapper.GetBool(tutorialKey, tutorialValue);
        HandleAdaptiveOnboardingEvent(toShow, bucket);
        
        if (toShow)
        {
            Analytics.CustomEvent("tutorial_start", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
        }
        tutorialStep = 0;
        SetTutorialStep(tutorialStep);
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
        HandleAdaptiveOnboardingEvent(overrideDecision, controlGroupValue);
        if (overrideDecision)
        {
            Analytics.CustomEvent("tutorial_start", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
        }
        return overrideDecision;
    }

    static void HandleAdaptiveOnboardingEvent (bool toShow, string bucket)
    {
        adaptiveOnboardingEventSent = PlayerPrefs.GetInt(adaptiveOnboardingSentPrefsKey, adaptiveOnboardingEventSent);
        if(adaptiveOnboardingEventSent == 0) 
        {
            Analytics.CustomEvent(adaptiveOnboardingEventName,
                new Dictionary<string, object>{
                    { tutorialOnKey, toShow },
                    { tutorialTestGroupKey, bucket == "_b" ? testGroupValue : controlGroupValue }
                }
            );
            adaptiveOnboardingEventSent = 1;
            PlayerPrefs.SetInt(adaptiveOnboardingSentPrefsKey, adaptiveOnboardingEventSent);
            PlayerPrefs.Save();
        }
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
            {"step_index", tutorialStep}
        });
    }

    static void SetTutorialStep(int newTutorialStep)
    {
        PlayerPrefs.SetInt(tutorialStepPlayerPrefsKey, newTutorialStep);
        PlayerPrefs.Save();
    }

    static int GetTutorialStep()
    {
        if (PlayerPrefs.HasKey(tutorialStepPlayerPrefsKey))
        {
            return tutorialStep = PlayerPrefs.GetInt(tutorialStepPlayerPrefsKey);
        }
        return 0;
    }

}
