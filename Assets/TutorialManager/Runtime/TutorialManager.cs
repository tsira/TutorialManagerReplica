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
        ABTestingWrapper.EnsureBucket(false);
        string bucket = PlayerPrefs.GetString("unity_analytics_ab_test_bucket");
        bool tutorialValue = (bucket == "_b") ? false : true;

        bool toShow = ABTestingWrapper.GetBool(tutorialKey, tutorialValue);
        if (toShow)
        {
            Analytics.CustomEvent("tutorial_start", new Dictionary<string, object> { { tutorialIdKey, tutorialKey } });
        }
        tutorialStep = 0;
        PlayerPrefs.SetInt(tutorialStepPlayerPrefsKey, tutorialStep);
        PlayerPrefs.Save();
        return toShow;
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
        if (PlayerPrefs.HasKey(tutorialStepPlayerPrefsKey))
        {
            tutorialStep = PlayerPrefs.GetInt(tutorialStepPlayerPrefsKey);
        }
        tutorialStep++;
        PlayerPrefs.SetInt(tutorialStepPlayerPrefsKey, tutorialStep);
        PlayerPrefs.Save();
        return Analytics.CustomEvent("tutorial_step", new Dictionary<string, object> {
            { tutorialIdKey, tutorialKey },
            {"step_index", tutorialStep}
        });
    }
}
