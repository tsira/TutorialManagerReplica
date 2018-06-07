using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Analytics;
using System.Linq;
using UnityEngine.Analytics.TutorialManagerRuntime;
using UnityEngine.Analytics.TutorialManagerEditor;
using System.Collections.Generic;

public class TMUpdaterUtility
{
    [InitializeOnLoadMethod]
    static void WatchSceneOpens()
    {
        EditorSceneManager.sceneOpened += SceneOpenedCallback;
    }

    static void SceneOpenedCallback(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        ForceUpdate();
    }

    public static void ForceUpdate()
    {
        AdaptiveText[] contentList = Object.FindObjectsOfType<AdaptiveText>();
        foreach (AdaptiveText content in contentList) {
            content.dataStore = TutorialManagerModelMiddleware.GetInstance();
            content.OnDataUpdate();
        }
    }

    public static void ConvertRemoteSettingsToModel(TutorialManagerModelMiddleware TMModel, List<RemoteSettingsKeyValueType> remoteSettings)
    {
        TMModel.Clear();

        // Convert to dictionary
        var dictionary = remoteSettings.ToDictionary(x => x.key, x => x);

        // Find the Tutorials key
        var tutorialsSet = dictionary["tutorials"];
        var tutorials = tutorialsSet.value.Substring(1, tutorialsSet.value.Length - 2).Split(',');

        // Loop through tutorials
        for (int a = 0; a < tutorials.Length; a++) {
            var tutorialId = tutorials[a].Substring(1, tutorials[a].Length - 2);
            TMModel.CreateTutorialEntity(tutorialId);

            // Find the specific tutorial key
            var tutorialSteps = dictionary[tutorialId];
            var steps = tutorialSteps.value.Substring(1, tutorialSteps.value.Length - 2).Split(',');

            // Loop through steps
            for (int b = 0; b < steps.Length; b++) {
                var fqStepId = steps[b].Substring(1, steps[b].Length - 2);
                var stepId = fqStepId.Split('-')[1];
                TMModel.CreateStepEntity(stepId, tutorialId);

                var contentKey = string.Format("{0}-{1}-text", tutorialId, stepId);
                if (dictionary.ContainsKey(contentKey)) {
                    var contentValue = dictionary[contentKey].value;
                    TMModel.CreateContentEntity(fqStepId, ContentType.text, contentValue);
                }
            }
        }
    }
}
