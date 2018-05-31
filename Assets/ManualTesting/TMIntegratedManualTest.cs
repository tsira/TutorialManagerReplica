using UnityEngine;
using UnityEngine.Analytics;

public class TMIntegratedManualTest : MonoBehaviour
{
    void Start()
    {
    }

    public void Next()
    {
        if (string.IsNullOrEmpty(TutorialManager.tutorialId)) {
            TutorialManager.Start("Tutorial1");
        } else if (TutorialManager.complete == false){
            TutorialManager.StepComplete();
        }
    }
}
