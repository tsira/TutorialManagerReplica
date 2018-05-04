using UnityEngine;
using UnityEditor;

namespace UnityEngine.Analytics
{
    public class TutorialManagerWindow : EditorWindow
    {
        private const string m_TabTitle = "Tutorial Manager";


        [MenuItem("Window/Unity Analytics/TutorialManager")]
        static void TutorialManagerMenuOption()
        {
            EditorWindow.GetWindow(typeof(TutorialManagerWindow), false, m_TabTitle);
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        private void OnFocus()
        {

        }

        private void OnGUI()
        {

        }

        private void SetGenre(string type)
        {

        }

        private void CreateTutorial(string id)
        {

        }

        private void UpdateTutorial(string id)
        {

        }

        private void DestroyTutorial(string id)
        {

        }

        private void CreateStep(string id)
        {

        }

        private void UpdateStep(string id)
        {

        }

        private void DestroyStep(string id)
        {

        }

        private void PushData()
        {

        }

        private void PullData()
        {

        }
    }
}