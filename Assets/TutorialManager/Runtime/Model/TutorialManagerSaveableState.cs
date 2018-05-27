namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    /// <summary>
    /// A stripped-down version of TutorialManagerState that can be serialized and saved to disk.
    /// </summary>
    [System.Serializable]
    public class TutorialManagerSaveableState
    {
        public bool showTutorial;
        public bool decisionReceived;
        public bool adaptiveOnboardingEventSent;
        public bool autoAdvance;
        public string tutorialId;
        public string currentStep;

        public TutorialManagerSaveableState() { }

        public TutorialManagerSaveableState(TutorialManagerState state)
        {
            showTutorial = state.showTutorial;
            decisionReceived = state.decisionReceived;
            adaptiveOnboardingEventSent = state.adaptiveOnboardingEventSent;
            autoAdvance = state.autoAdvance;
            tutorialId = state.tutorialId;
            currentStep = state.currentStep;
        }
    }
}