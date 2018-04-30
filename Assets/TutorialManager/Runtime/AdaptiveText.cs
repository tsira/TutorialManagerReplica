using UnityEngine;
using System.Collections;
using TMPro;

namespace TutorialManager
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class AdaptiveText : AdaptiveContent
    {
        public TextMeshProUGUI tmp;

        override protected void Start()
        {
            base.Start();
            tmp = GetComponent<TextMeshProUGUI>();
        }

	}
}