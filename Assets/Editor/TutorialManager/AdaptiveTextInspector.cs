using UnityEngine;
using UnityEditor;

namespace UnityEngine.Analytics.TutorialManagerRuntime {
    [CustomEditor(typeof(AdaptiveText))]
    public class AdaptiveTextInspector : AdaptiveContentInspector
    {

        string lastKnownTextValue;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AdaptiveText myTarget = (AdaptiveText)target;
            TutorialManagerModelMiddleware model = TutorialManagerModelMiddleware.GetInstance();

            if (lastKnownTextValue != myTarget.GetCurrentText()) {
                CreateOrUpdateTextEntity(myTarget, model);
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
