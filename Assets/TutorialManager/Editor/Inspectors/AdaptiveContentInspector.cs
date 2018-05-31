using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [CustomEditor(typeof(AdaptiveContent))]
    public class AdaptiveContentInspector : Editor
    {

        GUIContent bindingLabel = new GUIContent("Binding ID", "The tutorial and step ID to which this component will be bound.");
        GUIContent blankBinding = new GUIContent("Select a binding...");
        const string k_TMSettingsRequiredMessage = "Adaptive content keys must be set up in the Tutorial Manager window.\nGo to Window > Unity Analytics > TutorialManager";

        GenericMenu bindingsMenu;
        int bindingIndex;
        List<string> stepIds = new List<string>();

        public override void OnInspectorGUI()
        {
            AdaptiveContent myTarget = (AdaptiveContent)target;
            TutorialManagerModel model = TutorialManagerModelMiddleware.GetInstance().TMData;
            if (model.steps.Count() == 0) {
                // Display warning
                EditorGUILayout.HelpBox(k_TMSettingsRequiredMessage, MessageType.Warning, true);

            } else {
                bindingIndex = GetCurrentIndex(model, myTarget);
                RenderBindingPopup(myTarget);
            }
        }

        static int GetCurrentIndex(TutorialManagerModel model, AdaptiveContent myTarget)
        {
            return model.steps.Select((value, index) => new { value, index })
                        .Where(pair => pair.value.id == myTarget.bindingId)
                        .Select(pair => pair.index + 1)
                        .FirstOrDefault() - 1;
        }

        private bool CachedStepsAreStillValid(TutorialManagerModel model)
        {
            bool isValid = model.stepTable.Count == stepIds.Count;
            if (isValid) {
                int aa = 0;
                foreach (KeyValuePair<string, StepEntity> t in model.stepTable) {
                    if (t.Value.id != stepIds[aa]) {
                        isValid = false;
                        break;
                    }
                    aa++;
                }
            }
            return isValid;
        }

        protected void BindTo(AdaptiveContent myTarget, string id, int index)
        {
            bindingIndex = index;
            myTarget.bindingId = id;
        }

        protected void RenderBindingPopup(AdaptiveContent myTarget)
        {
            TutorialManagerModel model = TutorialManagerModelMiddleware.GetInstance().TMData;
            using (new GUILayout.HorizontalScope()) {
                var labelRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(labelRect, bindingLabel);
                var buttonRect = EditorGUILayout.GetControlRect();
                string id = model.steps[bindingIndex].id;
                string bindingDisplayName = Regex.Replace(id, "-", ", ");
                if (GUI.Button(buttonRect, new GUIContent(bindingIndex < 0 ? blankBinding : new GUIContent(bindingDisplayName)), EditorStyles.popup)) {
                    BuildBindingsMenu(bindingIndex);
                    bindingsMenu.DropDown(buttonRect);
                }
            }
        }

        void BuildBindingsMenu(int selectedIndex = -1)         {
            TutorialManagerModel model = TutorialManagerModelMiddleware.GetInstance().TMData;             bindingsMenu = new GenericMenu();
            int index = 0;
            model.steps.ForEach(step => {
                string stepId = step.id;
                bindingsMenu.AddItem(
                    new GUIContent(stepId.Replace('-', '/')),
                    index == selectedIndex,
                    SetSelectedBinding,
                    new SelectedBindingInfo(stepId, index)
                );
                index++;
            });         }

        void SetSelectedBinding(object info)
        {
            var selectedBindingInfo = (SelectedBindingInfo)info;
            BindTo(target as AdaptiveContent, selectedBindingInfo.bindingId, selectedBindingInfo.bindingIndex);
        }
    }

    struct SelectedBindingInfo
    {
        public string bindingId;
        public int bindingIndex;

        public SelectedBindingInfo(string stepId, int index)
        {
            bindingId = stepId;
            bindingIndex = index;
        }
    }
}

