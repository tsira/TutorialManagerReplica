using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Analytics;
using System.Linq;
using UnityEngine.Analytics.TutorialManagerRuntime;

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
}
