using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [CustomEditor(typeof(AdaptiveContent))]
    [CanEditMultipleObjects]
    public class AdaptiveContentInspector : Editor
    {

        SerializedProperty bindingIdProperty;
        SerializedProperty respectRemoteProperty;

        GUIContent bindingLabel = new GUIContent("Binding ID", "The tutorial and step ID to which this component will be bound.");
        GUIContent respectRemoteLabel = new GUIContent("Respect 'off' decision", "This GameObject will appear when the bound step starts, " +
                                                       "AND TutorialManager decides to show the tutorial. Uncheck this box " +
                                                       "to force the object to appear, regardless of that decision.");
        GUIContent blankBinding = new GUIContent("Select a binding...");
        const string k_TMSettingsRequiredMessage = "Adaptive content keys must be set up in the Tutorial Manager window.\nGo to Window > Unity Analytics > TutorialManager";

        GenericMenu bindingsMenu;
        List<string> stepIds = new List<string>();


        private void OnEnable()
        {
            bindingIdProperty = serializedObject.FindProperty("bindingId");
            respectRemoteProperty = serializedObject.FindProperty("respectRemoteIsActive");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            TutorialManagerModel model = TutorialManagerModelMiddleware.GetInstance().TMData;
            if (model.steps.Count() == 0) {
                // Display warning
                EditorGUILayout.HelpBox(k_TMSettingsRequiredMessage, MessageType.Warning, true);

            } else {
                RenderBindingPopup();
                RenderOptOutCheckbox();
            }
            serializedObject.ApplyModifiedProperties();
        }

        int GetCurrentIndex(TutorialManagerModel model)
        {
            return model.steps.Select((value, index) => new { value, index })
                        .Where(pair => pair.value.id == bindingIdProperty.stringValue)
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

        protected void BindTo(string id, int index)
        {
            bindingIdProperty.stringValue = id;
            serializedObject.ApplyModifiedProperties();
        }

        protected void RenderBindingPopup()
        {
            TutorialManagerModel model = TutorialManagerModelMiddleware.GetInstance().TMData;
            int bindingIndex = GetCurrentIndex(model);
            using (new GUILayout.HorizontalScope()) {
                var labelRect = EditorGUILayout.GetControlRect();

                labelRect.width = EditorGUIUtility.labelWidth;
                var buttonRect = new Rect(
                    labelRect.x + labelRect.width,
                    labelRect.y,
                    EditorGUIUtility.currentViewWidth - (labelRect.width + (labelRect.x * 2f)),
                    EditorGUIUtility.singleLineHeight
                );

                EditorGUI.LabelField(labelRect, bindingLabel);

                string id = "Binding lost";
                if (bindingIndex >= 0 && bindingIndex < model.steps.Count) {
                     id = model.steps[bindingIndex].id;
                }

                string bindingDisplayName = Regex.Replace(id, "-", ", ");
                if (GUI.Button(buttonRect, new GUIContent(bindingIndex < 0 ? blankBinding : new GUIContent(bindingDisplayName)), EditorStyles.popup)) {
                    BuildBindingsMenu(bindingIndex);
                    bindingsMenu.DropDown(buttonRect);
                }
            }
        }

        protected void RenderOptOutCheckbox()
        {
            var labelRect = EditorGUILayout.GetControlRect();

            labelRect.width = EditorGUIUtility.labelWidth;
            var checkBoxRect = new Rect(
                labelRect.x + labelRect.width,
                labelRect.y,
                EditorGUIUtility.currentViewWidth - (labelRect.width + (labelRect.x * 2f)),
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.LabelField(labelRect, respectRemoteLabel);

            respectRemoteProperty.boolValue = EditorGUI.Toggle(checkBoxRect, respectRemoteProperty.boolValue);
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
            BindTo(selectedBindingInfo.bindingId, selectedBindingInfo.bindingIndex);
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
