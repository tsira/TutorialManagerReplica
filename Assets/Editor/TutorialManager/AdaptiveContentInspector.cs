using UnityEditor;
using System.Collections.Generic;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [CustomEditor(typeof(AdaptiveContent))]
    public class AdaptiveContentInspector : Editor
    {

        GUIContent bindingLabel = new GUIContent("Binding");

        int bindingIndex = 0;
        int lastKnownBindingIndex = -1;
        GUIContent[] bindingContent;


        List<string> stepIds = new List<string>();

        public override void OnInspectorGUI()
        {
            AdaptiveContent myTarget = (AdaptiveContent)target;


            TutorialManagerModel manifest = TutorialManagerModelMiddleware.GetInstance().TMData;

            if (CachedStepsAreStillValid(manifest) == false) {
                ConstructBindings(manifest);
            }

            // Display popup
            bindingIndex = EditorGUILayout.Popup(bindingLabel, bindingIndex, bindingContent);

            // If changed, make the new connection
            if (bindingIndex != lastKnownBindingIndex && bindingIndex > -1) {
                BindTo(stepIds[bindingIndex]);
            }

            lastKnownBindingIndex = bindingIndex;

            EditorGUILayout.LabelField("My GUID");
            EditorGUILayout.TextField(myTarget.bindingId);
        }

        static string FindTutorialByStepId(Dictionary<string, TutorialEntity> table, string stepId)
        {
            foreach (KeyValuePair<string, TutorialEntity> kv in table) {
                foreach (string s in kv.Value.steps) {
                    if (s == stepId) {
                        return kv.Value.id;
                    }
                }
            }
            return null;
        }


        protected void ConstructBindings(TutorialManagerModel manifest)
        {
            stepIds = new List<string>();
            bindingContent = new GUIContent[manifest.steps.Count];
            // Build list
            int aa = 0;
            foreach (KeyValuePair<string, StepEntity> t in manifest.stepTable) {
                var stepId = t.Value.id;
                stepIds.Add(stepId);
                var tutorialName = FindTutorialByStepId(manifest.tutorialTable, stepId);
                var contentString = string.Format("{0}: {1}", tutorialName, stepId);
                bindingContent[aa] = new GUIContent(contentString);

                aa++;
            }
        }

        private bool CachedStepsAreStillValid(TutorialManagerModel manifest)
        {
            bool isValid = manifest.stepTable.Count == stepIds.Count;
            if (isValid) {
                int aa = 0;
                foreach (KeyValuePair<string, StepEntity> t in manifest.stepTable) {
                    if (t.Value.id != stepIds[aa]) {
                        isValid = false;
                        break;
                    }
                    aa++;
                }
            }
            return isValid;
        }

        protected void BindTo(string id)
        {

        }
    }


}

