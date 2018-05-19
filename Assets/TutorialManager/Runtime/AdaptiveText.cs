using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnityEngine.Analytics {
    public class AdaptiveText : AdaptiveContent
    {

        override protected void Start()
        {
            base.Start();
            SyncTextToTextField();
        }

        protected override void OnDataUpdate()
        {
            base.OnDataUpdate();
            SyncTextToTextField();
        }

        public string GetCurrentText()
        {
            if (GetComponent<TextMesh>() != null) {
                return GetComponent<TextMesh>().text;
            } else if (GetComponent<Text>() != null) {
                return GetComponent<Text>().text;
            }
            //else if (GetComponent<TextMeshProUGUI>() != null) {
            //    return GetComponent<TextMeshProUGUI>().text;
            //}

            return null;
        }

        protected void SyncTextToTextField()
        {
            if (string.IsNullOrEmpty(bindingId)) {
                return;
            }

            if (GetComponent<TextMesh>() != null) {
                SyncToTextMesh();
            } 
            else if (GetComponent<Text>() != null) {
                SyncToUIText();
            } 
            //else if (GetComponent<TextMeshProUGUI>() != null) {
            //    SyncToTextMeshProUGUI();
            //}
        }

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

        //protected void SyncToTextMeshProUGUI()
        //{
        //    string existingText = GetComponent<SyncToTextMeshProUGUI>().text;
        //    string newText = dataStore.GetString(bindingId, existingText);
        //    GetComponent<SyncToTextMeshProUGUI>().text = newText;
        //}
    }
}

