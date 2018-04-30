using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TutorialManager
{
    [CustomEditor(typeof(AdaptiveContent))]
    public class AdaptiveContentEditor : Editor
    {

        GUIContent bindingLabel = new GUIContent("Binding");

        int bindingIndex = 0;
        int lastKnownBindingIndex = -1;
        GUIContent[] bindingContent;

        public override void OnInspectorGUI()
        {
            AdaptiveContent myTarget = (AdaptiveContent)target;


            TMManifest manifest = TMParser.manifest;
            bindingContent = new GUIContent[manifest.steps.Length];
            List<string> stepIds = new List<string>();

            // Build list
            int aa = 0;
            foreach (KeyValuePair<string, Step> t in manifest.stepTable)
            {
                var stepName = t.Value.name;
                var stepId = t.Value.id;
                stepIds.Add(stepId);
                var tutorialName = FindTutorialByStepId(manifest.tutorialTable, stepId);
                var contentString = string.Format("{0}: {1}", tutorialName, stepName);
                bindingContent[aa] = new GUIContent(contentString);

                aa++;
            }

            // Display popup
            bindingIndex = EditorGUILayout.Popup(bindingLabel, bindingIndex, bindingContent);

            // If changed, make the new connection
            if (bindingIndex != lastKnownBindingIndex)
            {
                myTarget.BindTo(stepIds[bindingIndex]);
            }

            lastKnownBindingIndex = bindingIndex;

            EditorGUILayout.LabelField("My GUID");
            EditorGUILayout.TextField(myTarget.uniqueId);
        }

        static string FindTutorialByStepId(Dictionary<string, Tutorial> table, string stepId)
        {
            foreach (KeyValuePair<string, Tutorial> kv in table)
            {
                foreach (string s in kv.Value.steps)
                {
                    if (s == stepId)
                    {
                        return kv.Value.name;
                    }
                }
            }
            return null;
        }
    }
}