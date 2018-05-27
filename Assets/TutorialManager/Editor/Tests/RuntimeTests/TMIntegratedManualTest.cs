using UnityEngine;
using UnityEngine.Analytics;

public class TMIntegratedManualTest : MonoBehaviour
{
    void Start()
    {
    }

    public void Next()
    {
        if (string.IsNullOrEmpty(TutorialManager.state.tutorialId)) {
            TutorialManager.TutorialStart("Tutorial1");
        } else if (TutorialManager.state.complete == false){
            TutorialManager.TutorialStep();
        }
    }
}
