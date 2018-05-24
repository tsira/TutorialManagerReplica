using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [CustomEditor(typeof(AdaptiveContent))]
    public class AdaptiveContentInspector : Editor
    {

        GUIContent bindingLabel = new GUIContent("Binding");

        const string k_TMSettingsRequiredMessage = "Adaptive content keys must be set up in the Tutorial Manager window.\nGo to Window > Unity Analytics > TutorialManager";

        int bindingIndex = 0;
        int lastKnownBindingIndex = -1;
        GUIContent[] bindingContent;


        List<string> stepIds = new List<string>();

        public override void OnInspectorGUI()
        {
            AdaptiveContent myTarget = (AdaptiveContent)target;

            TutorialManagerModel model = TutorialManagerModelMiddleware.GetInstance().TMData;

            if (CachedStepsAreStillValid(model) == false) {
                ConstructBindings(model);
            }

            if (bindingContent.Count() == 0) {
                // Display warning
                EditorGUILayout.HelpBox(k_TMSettingsRequiredMessage, MessageType.Warning, true);

            } else {
                // Display popup
                bindingIndex = stepIds.IndexOf(myTarget.bindingId);
                bindingIndex = EditorGUILayout.Popup(bindingLabel, bindingIndex, bindingContent);
            }

            // If changed, make the new connection
            if (bindingIndex != lastKnownBindingIndex && bindingIndex > -1) {
                BindTo(myTarget, stepIds[bindingIndex]);
            }

            lastKnownBindingIndex = bindingIndex;
        }


        protected void ConstructBindings(TutorialManagerModel manifest)
        {
            stepIds = new List<string>();
            bindingContent = new GUIContent[manifest.steps.Count];

            int a = 0;
            manifest.stepTable.ToList().ForEach(x => {
                var stepId = x.Value.id;
                stepIds.Add(stepId);
                // FIXME : Do we need this? Might lose, depending on Sean's design. -- MAT, May 17, 2018
                //var tutorialName = FindTutorialByStepId(manifest.tutorialTable, stepId);
                var contentString = string.Format("{0}", stepId);
                bindingContent[a] = new GUIContent(contentString);
                a++;
            });
        }

        static string FindTutorialByStepId(Dictionary<string, TutorialEntity> table, string stepId)
        {
            return table.Where(x => x.Value.steps.Contains(stepId)).First().Value.id;
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

        protected void BindTo(AdaptiveContent myTarget, string id)
        {
            myTarget.bindingId = id;
        }
    }
}

