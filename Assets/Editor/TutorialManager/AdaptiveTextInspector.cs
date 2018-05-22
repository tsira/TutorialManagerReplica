using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
#if TEXTMESHPRO_PRESENT
using TMPro;
#endif

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [CustomEditor(typeof(AdaptiveText))]
    public class AdaptiveTextInspector : AdaptiveContentInspector
    {

        const string k_TextRequiredMessage = "Without a Text, TextMesh or TextMeshPro object, this Adaptive Text component has nothing to adapt!";

        string lastKnownTextValue;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AdaptiveText myTarget = (AdaptiveText)target;
            TutorialManagerModelMiddleware model = TutorialManagerModelMiddleware.GetInstance();

            if (lastKnownTextValue != myTarget.GetCurrentText()) {
                CreateOrUpdateTextEntity(myTarget, model);
            }

            bool hasText = false;
            if (myTarget.GetComponent<Text>() != null || myTarget.GetComponent<TextMesh>() != null) {
                hasText = true;
            }
#if TEXTMESHPRO_PRESENT

            Debug.Log(myTarget.GetComponent<TMP_Text>());

            if (!hasText && myTarget.GetComponent<TMP_Text>() != null) {
                hasText = true;
            }
#endif
            if (hasText == false) {
                // Display warning
                EditorGUILayout.HelpBox(k_TextRequiredMessage, MessageType.Warning, true);
            }


            lastKnownTextValue = myTarget.GetCurrentText();
        }

        private void CreateOrUpdateTextEntity(AdaptiveText myTarget, TutorialManagerModelMiddleware model)
        {
            string qualifiedBindingId = string.Format("{0}-text", myTarget.bindingId);
            if (model.TMData.contentTable.ContainsKey(qualifiedBindingId)) {
                model.UpdateContentEntity(qualifiedBindingId, myTarget.GetCurrentText());
            } else {
                model.CreateContentEntity(myTarget.bindingId, ContentType.text, myTarget.GetCurrentText());
            }
        }

        private void DestroyTextEntity(string id)
        {
            // TODO: How do we reliably prune orphans?
        }
    }
}
