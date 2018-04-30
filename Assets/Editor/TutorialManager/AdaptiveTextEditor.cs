using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace UnityEngine.Analytics.TutorialManager
{
    
    [CustomEditor(typeof(AdaptiveText))]
    public class AdaptiveTextEditor : AdaptiveContentEditor
    {
        string lastKnownTextValue;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AdaptiveText myTarget = (AdaptiveText)target;

            if (lastKnownTextValue != myTarget.tmp.text)
            {
                myTarget.bindingResolver.WriteString(myTarget.bindings,
                                                     "content/text",
                                                     "text",
                                                     myTarget.tmp.text);
            }
            lastKnownTextValue = myTarget.tmp.text;

            if (GUILayout.Button("Reload"))
            {
                TMParser.ForceUpdate();
            }
            if (GUILayout.Button("Save"))
            {
                TMParser.SaveToJson();
            }
        }
    }
}