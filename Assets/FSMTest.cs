using UnityEngine;
using System.Collections;
using UnityEngine.Analytics.TutorialManagerRuntime;
using UnityEngine.Analytics;

public class FSMTest : MonoBehaviour
{
    TutorialManagerModelMiddleware modelMiddleware;
    AdaptiveStateDispatcher dispatcher;
    TutorialManagerFSM fsm;

    StateSystemProvider provider;


	// Use this for initialization
	void Awake()
	{
        modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        dispatcher = AdaptiveStateDispatcher.GetInstance();
        provider = StateSystemProvider.GetInstance();


        fsm = new TutorialManagerFSM();
        // TODO: this is a default case...but we need to address again if a user is not at the start.
        fsm.stateList = modelMiddleware.TMData.tutorials[0].steps;
        fsm.dispatcher = dispatcher;

        provider.SetDispatcher(dispatcher);
        provider.SetDataStore(modelMiddleware);
	}

    public void Next()
    {
        fsm.NextState();
    }
}
