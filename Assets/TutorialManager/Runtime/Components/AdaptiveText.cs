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

        override protected void Start()
        {
            base.Start();
            SyncTextToTextField();
        }

        override public void OnDataUpdate()
        {
            base.OnDataUpdate();
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

            return null;
        }

        protected void SyncTextToTextField()
        {
            if (string.IsNullOrEmpty(bindingId)) {
                return;
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
            GetComponent<TMP_Text>().text = newText;
        }
#endif

        protected void SyncToTextMesh()
        {
            string existingText = GetComponent<TextMesh>().text;
            string newText = dataStore.GetString(bindingId, existingText);
            GetComponent<TextMesh>().text = newText;
        }

        protected void SyncToUIText()
        {
            string existingText = GetComponent<Text>().text;
            string newText = dataStore.GetString(bindingId, existingText);
            GetComponent<Text>().text = newText;
        }
    }
}

