using UnityEditor;
using UnityEngine.Analytics.TutorialManagerEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [CustomEditor(typeof(AdaptiveContent))]
    [CanEditMultipleObjects]
    public class AdaptiveContentInspector : Editor
    {

        const string k_TMSettingsRequiredMessage = "Adaptive content keys must be set up in the Tutorial Manager window.\nGo to Window > Unity Analytics > TutorialManager";
        const string k_AddBindingTooltip = "Add a binding to this GameObject.";
        const string k_DeleteBindingTooltip = "Remove a binding from this GameObject.";

        protected SerializedProperty bindingIdsProperty;
        protected SerializedProperty respectRemoteProperty;

        GUIContent bindingLabel = new GUIContent("Binding Keys", "One or more tutorial/step IDs to which this component will be bound.");
        GUIContent emptyLabel = new GUIContent("");
        GUIContent respectRemoteLabel = new GUIContent("Respect 'off' decision", "This GameObject will appear when the bound step starts, " +
                                                       "AND TutorialManager decides to show the tutorial. Uncheck this box " +
                                                       "to force the object to appear, regardless of that decision.");
        GUIContent blankBinding = new GUIContent("Select a binding...");
        GUIContent addBindingButtonGUIContent = new GUIContent("+", k_AddBindingTooltip);
        GUIContent deleteBindingButtonGUIContent = new GUIContent("-", k_DeleteBindingTooltip);

        GUIStyle addButtonStyle;

        GenericMenu bindingsMenu;
        List<string> stepIds = new List<string>();
        protected bool bindingsHaveChanged = false;

        protected virtual void OnEnable()
        {
            bindingIdsProperty = serializedObject.FindProperty("bindingIds");
            respectRemoteProperty = serializedObject.FindProperty("respectRemoteIsActive");

            CheckToSendAddedEvent();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            RenderContentElements();
            serializedObject.ApplyModifiedProperties();
            bindingsHaveChanged = false;
        }

        protected void RenderContentElements()
        {
            if (addButtonStyle == null) {
                DefineStyles();
            }

            TutorialManagerModel model = TutorialManagerModelMiddleware.GetInstance().TMData;
            if (model.steps.Count() == 0) {
                // Display warning
                EditorGUILayout.HelpBox(k_TMSettingsRequiredMessage, MessageType.Warning, true);

            } else {
                RenderBindings();
                RenderOptOutCheckbox();
            }
        }

        private int GetCurrentIndex(TutorialManagerModel model, string bindingId)
        {
            return model.steps.Select((value, index) => new { value, index })
                        .Where(pair => pair.value.id == bindingId)
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

        protected void RenderBindings()
        {
            SerializedProperty arraySizeProperty = bindingIdsProperty.FindPropertyRelative("Array.size");
            TutorialManagerModel model = TutorialManagerModelMiddleware.GetInstance().TMData;

            int count = Mathf.Max(1, arraySizeProperty.intValue);
            for (int a = 0; a < count; a++) {
                bool isFirst = (a == 0);
                bool isLast = (a == count - 1);

                int bindingIndex = 0;

                if (isLast && a >= arraySizeProperty.intValue) {
                    bindingIndex = 0;
                } else {
                    SerializedProperty elementProperty = bindingIdsProperty.GetArrayElementAtIndex(a);
                    bindingIndex = GetCurrentIndex(model, elementProperty.stringValue);
                }

                using (new GUILayout.HorizontalScope()) {
                    var labelRect = EditorGUILayout.GetControlRect();

                    labelRect.width = EditorGUIUtility.labelWidth;
                    var buttonRect = new Rect(
                        labelRect.x + labelRect.width,
                        labelRect.y,
                        EditorGUIUtility.currentViewWidth - (labelRect.width + (labelRect.x * 2f)),
                        EditorGUIUtility.singleLineHeight
                    );

                    if (isLast) {
                        buttonRect.width -= 40f;
                    }

                    var labelContent = isFirst ? bindingLabel : emptyLabel;
                    EditorGUI.LabelField(labelRect, labelContent);

                    string id = "Binding lost";
                    if (bindingIndex >= 0 && bindingIndex < model.steps.Count) {
                        id = model.steps[bindingIndex].id;
                    }

                    string bindingDisplayName = Regex.Replace(id, "-", ", ");
                    if (GUI.Button(buttonRect, new GUIContent(bindingIndex < 0 ? blankBinding : new GUIContent(bindingDisplayName)), EditorStyles.popup)) {
                        BuildBindingsMenu(a, bindingIndex);
                        bindingsMenu.DropDown(buttonRect);
                    }

                    if (isLast) {
                        if (GUILayout.Button(addBindingButtonGUIContent, addButtonStyle, GUILayout.MaxWidth(20f))) {
                            arraySizeProperty.intValue++;
                            bindingsHaveChanged = true;

                            SendAddBindingEvent(arraySizeProperty.intValue);
                        }
                        EditorGUI.BeginDisabledGroup(isFirst);
                        if (GUILayout.Button(deleteBindingButtonGUIContent, addButtonStyle, GUILayout.MaxWidth(20f))) {
                            arraySizeProperty.intValue = Mathf.Max(1, arraySizeProperty.intValue - 1);
                            bindingsHaveChanged = true;

                            SendRemoveBindingEvent(arraySizeProperty.intValue);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
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

        void BuildBindingsMenu(int listIndex, int selectedIndex = -1)
        {
            TutorialManagerModel model = TutorialManagerModelMiddleware.GetInstance().TMData;
            bindingsMenu = new GenericMenu();
            int index = 0;
            model.steps.ForEach(step => {
                string stepId = step.id;
                bindingsMenu.AddItem(
                    new GUIContent(stepId.Replace('-', '/')),
                    index == selectedIndex,
                    SetSelectedBinding,
                    new SelectedBindingInfo(stepId, index, listIndex)
                );
                index++;
            });
        }

        void SetSelectedBinding(object info)
        {
            var selectedBindingInfo = (SelectedBindingInfo)info;
            BindTo(selectedBindingInfo.bindingId, selectedBindingInfo.listIndex);
        }

        protected void BindTo(string id, int listIndex)
        {
            SerializedProperty arraySizeProperty = bindingIdsProperty.FindPropertyRelative("Array.size");
            if (listIndex >= arraySizeProperty.intValue) {
                bindingIdsProperty.InsertArrayElementAtIndex(listIndex);
            }
            SerializedProperty elementProperty = bindingIdsProperty.GetArrayElementAtIndex(listIndex);

            elementProperty.stringValue = id;
            bindingsHaveChanged = true;
            serializedObject.ApplyModifiedProperties();
        }

        private void DefineStyles()
        {
            addButtonStyle = new GUIStyle(GUI.skin.button);
            addButtonStyle.normal.background = null;
            addButtonStyle.fontStyle = FontStyle.Bold;
            addButtonStyle.alignment = TextAnchor.MiddleLeft;
            addButtonStyle.fixedWidth = 30f;
        }

        protected virtual void SendAddBindingEvent(int count)
        {
            TMEditorEvent.Send(TMEditorEventType.addBinding, new Dictionary<string, object>{
                { "binding_count", count },
                { "component_type", "Content" }
            });
        }

        protected virtual void SendRemoveBindingEvent(int count)
        {
            TMEditorEvent.Send(TMEditorEventType.removeBinding, new Dictionary<string, object>{
                { "binding_count", count },
                { "component_type", "Content" }
            });
        }

        void CheckToSendAddedEvent()
        {
            AdaptiveContent myTarget = (AdaptiveContent)target;
            if (myTarget.hasBeenReset) {
                myTarget.hasBeenReset = false;
                SendAddedEvent();
            }
        }

        protected virtual void SendAddedEvent()
        {
            TMEditorEvent.Send(TMEditorEventType.addAdaptiveContent);
        }
    }

    struct SelectedBindingInfo
    {
        public string bindingId;
        public int bindingIndex;
        public int listIndex;

        public SelectedBindingInfo(string stepId, int index, int listIndx)
        {
            bindingId = stepId;
            bindingIndex = index;
            listIndex = listIndx;
        }
    }
}
