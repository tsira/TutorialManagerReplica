using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#if TEXTMESHPRO_PRESENT
using TMPro;
#endif

namespace UnityEngine.Analytics
{
    public class AdaptiveText : AdaptiveContent
    {

        public bool ignoreIfModelValueEmpty = true;

        override protected void Start()
        {
            base.Start();
            SyncTextToTextField();
        }

        protected override void OnEnterState(string id)
        {
            base.OnEnterState(id);
            SyncTextToTextField();
        }

        public override void OnDataUpdate()
        {
            SyncTextToTextField();
        }

        public string GetCurrentText()
        {
#if TEXTMESHPRO_PRESENT
            if (GetComponent<TMP_Text>() != null) {
                return GetComponent<TMP_Text>().text;
            } else
#endif
            if (GetComponent<Text>() != null) {
                return GetComponent<Text>().text;
            }
            else if (GetComponent<TextMesh>() != null) {
                return GetComponent<TextMesh>().text;
            }

            return string.Empty;
        }

        protected void SyncTextToTextField()
        {
            // Attempt to resolve a binding id
            if (string.IsNullOrEmpty(bindingId)) {
                if (bindingIds.Count > 0) {
                    bindingId = bindingIds[0];
                    if (string.IsNullOrEmpty(bindingId)) {
                        return;
                    }
                }
            }

#if TEXTMESHPRO_PRESENT
            if (GetComponent<TMP_Text>() != null) {
                SyncToTextMeshProUGUI();
            } else
#endif
            if (GetComponent<Text>() != null) {
                SyncToUIText();
            }
            else if (GetComponent<TextMesh>() != null) {
                SyncToTextMesh();
            } 
        }

#if TEXTMESHPRO_PRESENT
        protected void SyncToTextMeshProUGUI()
        {
            string existingText = GetComponent<TMP_Text>().text;
            string newText = dataStore.GetString(bindingId, existingText);
            if (ignoreIfModelValueEmpty && string.IsNullOrEmpty(newText)) {
                return;
            }
            GetComponent<TMP_Text>().text = newText;
        }
#endif

        protected void SyncToTextMesh()
        {
            string existingText = GetComponent<TextMesh>().text;
            string newText = dataStore.GetString(bindingId, existingText);
            if (ignoreIfModelValueEmpty && string.IsNullOrEmpty(newText)) {
                return;
            }
            GetComponent<TextMesh>().text = newText;
        }

        protected void SyncToUIText()
        {
            string existingText = GetComponent<Text>().text;
            string newText = dataStore.GetString(bindingId, existingText);
            if (ignoreIfModelValueEmpty && string.IsNullOrEmpty(newText)) {
                return;
            }
            GetComponent<Text>().text = newText;
        }
    }
}

