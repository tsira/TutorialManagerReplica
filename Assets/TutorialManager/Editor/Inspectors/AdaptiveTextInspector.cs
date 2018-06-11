using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
#if TEXTMESHPRO_PRESENT
using TMPro;
#endif

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [CustomEditor(typeof(AdaptiveText))]
    [CanEditMultipleObjects]
    public class AdaptiveTextInspector : AdaptiveContentInspector
    {

        const string k_TextRequiredMessage = "Without a Text, TextMesh or TextMeshPro object, this Adaptive Text component has nothing to adapt!";
        GUIContent ignoreIfEmptyLabel = new GUIContent("Ignore if remote is empty", "When checked, remote text will override local " +
                                                       "text ONLY when the provided string is non-empty. This can be useful when " +
                                                       "binding multiple steps to a single Text component (e.g., if your tutorial " +
                                                       "reuses a single dialog box).");


        string lastKnownTextValue;

        SerializedProperty ignoreIfModelValueEmptyProperty;

        override protected void OnEnable()
        {
            base.OnEnable();
            ignoreIfModelValueEmptyProperty = serializedObject.FindProperty("ignoreIfModelValueEmpty");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            RenderContentElements();

            AdaptiveText myTarget = (AdaptiveText)target;
            TutorialManagerModelMiddleware model = TutorialManagerModelMiddleware.GetInstance();

            RenderOverrideOnlyWhenNotEmptyCheckbox();

            if (lastKnownTextValue != myTarget.GetCurrentText() || bindingsHaveChanged) {
                CreateOrUpdateTextEntities(myTarget, model);
            }

            bool hasText = false;
            if (myTarget.GetComponent<Text>() != null || myTarget.GetComponent<TextMesh>() != null) {
                hasText = true;
            }
#if TEXTMESHPRO_PRESENT
            if (!hasText && myTarget.GetComponent<TMP_Text>() != null) {
                hasText = true;
            }
#endif
            if (hasText == false) {
                // Display warning
                EditorGUILayout.HelpBox(k_TextRequiredMessage, MessageType.Warning, true);
            }

            serializedObject.ApplyModifiedProperties();
            lastKnownTextValue = myTarget.GetCurrentText();
            bindingsHaveChanged = false;
        }

        protected void RenderOverrideOnlyWhenNotEmptyCheckbox()
        {
            var labelRect = EditorGUILayout.GetControlRect();

            labelRect.width = EditorGUIUtility.labelWidth;
            var checkBoxRect = new Rect(
                labelRect.x + labelRect.width,
                labelRect.y,
                EditorGUIUtility.currentViewWidth - (labelRect.width + (labelRect.x * 2f)),
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.LabelField(labelRect, ignoreIfEmptyLabel);

            ignoreIfModelValueEmptyProperty.boolValue = EditorGUI.Toggle(checkBoxRect, ignoreIfModelValueEmptyProperty.boolValue);
        }

        private void CreateOrUpdateTextEntities(AdaptiveText myTarget, TutorialManagerModelMiddleware model)
        {
            SerializedProperty arraySizeProperty = bindingIdsProperty.FindPropertyRelative("Array.size");
            int listCount = arraySizeProperty.intValue;
            for (int a = 0; a < listCount; a++) {
                string qualifiedBindingId = string.Format("{0}-text", bindingIdsProperty.GetArrayElementAtIndex(a).stringValue);
                if (model.TMData.contentTable.ContainsKey(qualifiedBindingId)) {
                    model.UpdateContentEntity(qualifiedBindingId, myTarget.GetCurrentText());
                } else if (string.IsNullOrEmpty(myTarget.bindingIds[a]) == false) {
                    model.CreateContentEntity(myTarget.bindingIds[a], ContentType.text, myTarget.GetCurrentText());
                }
            }
        }

        private void DestroyTextEntity(string id)
        {
            // TODO: How do we reliably prune orphans?
        }
    }
}
