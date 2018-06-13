using UnityEngine;
using UnityEngine.Analytics;

public class TMIntegratedManualTest : MonoBehaviour
{

    public string m_TutorialToStart;

    void Start()
    {
    }

    public void Next()
    {
        if (string.IsNullOrEmpty(TutorialManager.tutorialId)) {
            if (TutorialManager.GetDecision()) {
                TutorialManager.Start(m_TutorialToStart);
            }
        } else if (TutorialManager.complete == false){
            TutorialManager.StepComplete();
        }
    }
}
