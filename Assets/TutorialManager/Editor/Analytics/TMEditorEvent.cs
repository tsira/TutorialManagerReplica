
using System.Collections.Generic;
using UnityEditor.CDP;

namespace UnityEngine.Analytics.TutorialManagerEditor
{
    public enum TMEditorEventType
    {
        editorOpened,
        pullData,
        pushData,
        addTutorial,
        addStep,
        addAdaptiveContent,
        addAdaptiveText,
        addBinding,
        removeTutorial,
        removeStep,
        removeBinding,
        clearProgress,
        pickGenre,
        gotoDashboard
    }

    public class TMEditorEvent
    {
        const string eventPrefix = "tutorialManager.";
        const string eventVersion = ".v1";


        public static void Send(TMEditorEventType tMEvent, Dictionary<string, object> dictionary = null)
        {
            string eventName = string.Format("{0}{1}{2}", eventPrefix, tMEvent.ToString(), eventVersion);
            var dict = dictionary;
            if (dict == null) {
                dict = new Dictionary<string, object>();
            }
            dict.Add("tm_version", TutorialManager.k_VersionNumber);
            CDPEvent.Send(eventName, dict);
        }
    }
}
