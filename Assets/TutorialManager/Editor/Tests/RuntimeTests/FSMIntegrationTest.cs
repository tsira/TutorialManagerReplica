#if UNITY_5_6_OR_NEWER
using System.Collections;
using UnityEngine.Analytics.TutorialManagerRuntime;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.UI;

namespace UnityEngine.Analytics
{
    /// <summary>
    /// This is an automated runtime integration test, intended to demonstrate that – absent the porcelain
    /// of the TutorialManager class – the underlying state machine, dispatcher, and components work.
    /// </summary>
    public class FSMIntegrationTest : TMIntegrationBase
    {
        void SetupFromStart()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();
            SetupAdaptiveContent();

            BootstrapRuntime();
            fsm.stateList = modelMiddleware.TMData.tutorials[0].steps;
        }

        virtual protected void SetupFromMiddle()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();

            BootstrapRuntime();

            fsm.stateList = modelMiddleware.TMData.tutorials[0].steps;
            fsm.GoToState(t1Step2LookupID);

            //Content comes along *after* the state is at step 2
            SetupAdaptiveContent();
        }


        protected void BootstrapRuntime()
        {
            modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
            dispatcher = AdaptiveStateDispatcher.GetInstance();
            provider = StateSystemProvider.GetInstance();

            fsm = new TutorialManagerFSM();
            fsm.dispatcher = dispatcher;

            provider.SetDispatcher(dispatcher);
            provider.SetDataStore(modelMiddleware);
        }

        [UnityTest]
        public IEnumerator FSM_BasicStepThroughState()
        {
            SetupFromStart();
            yield return null;

            Assert.IsNull(fsm.state, "game start, fsm should be in a null state");

            fsm.NextState();
            yield return null;

            Assert.That(fsm.state, Is.EqualTo(t1Step1LookupID), string.Format("step one, fsm should be in state {0}", t1Step1LookupID));

            fsm.NextState();
            yield return null;

            Assert.That(fsm.state, Is.EqualTo(t1Step2LookupID), string.Format("step two, fsm should be in state {0}", t1Step2LookupID));

            fsm.NextState();
            yield return null;

            Assert.That(fsm.state, Is.EqualTo(t1Step3LookupID), string.Format("step three, fsm should be in state {0}", t1Step3LookupID));

            fsm.NextState();
            yield return null;

            Assert.IsNull(fsm.state, "tutorial complete, fsm should be in a null state");
        }

        [UnityTest]
        public IEnumerator FSM_StepThroughStateAdaptiveContent()
        {
            SetupFromStart();

            var listOfContent = GetListOfContent();
            yield return null;

            fsm.NextState();
            yield return null;

            TestIsActive(listOfContent, fsm.state);

            var step1GoText = GameObject.Find(ConstructTextName(t1Step1LookupID));
            Assert.That(step1GoText.GetComponent<Text>().text, Is.EqualTo(t1step1Text), string.Format("step one text should be {0}", t1step1Text));

            fsm.NextState();
            yield return null;

            TestIsActive(listOfContent, fsm.state);

            var step2GoText = GameObject.Find(ConstructTextName(t1Step2LookupID));
            Assert.That(step2GoText.GetComponent<Text>().text, Is.EqualTo(t1step2Text), string.Format("step two text should be {0}", t1step2Text));

            fsm.NextState();
            yield return null;

            TestIsActive(listOfContent, fsm.state);

            var step3GoText = GameObject.Find(ConstructTextName(t1Step3LookupID));
            Assert.That(step3GoText.GetComponent<Text>().text, Is.EqualTo(t1step3Text), string.Format("step three text should be {0}", t1step3Text));

            fsm.NextState();
            yield return null;

            TestIsActive(listOfContent, "someUnknownKey");
        }

        [UnityTest]
        public IEnumerator FSM_UpdateAStepAndAdvance()
        {
            SetupFromStart();
            yield return null;

            var model = TutorialManagerModelMiddleware.GetInstance();
            var step1TextKey = ConstructTextName(t1Step1LookupID);
            model.UpdateContentEntity(step1TextKey, t1step1Textv2);

            fsm.NextState();
            yield return null;

            var step1GoText = GameObject.Find(ConstructTextName(t1Step1LookupID));
            Assert.That(step1GoText.GetComponent<Text>().text, Is.EqualTo(t1step1Textv2), string.Format("step one text should be {0}", t1step1Textv2));

            fsm.NextState();
            yield return null;

            var step2GoText = GameObject.Find(ConstructTextName(t1Step2LookupID));
            Assert.That(step2GoText.GetComponent<Text>().text, Is.EqualTo(t1step2Text), string.Format("step two text should be {0}", t1step2Text));

            fsm.NextState();
            yield return null;

            var step3GoText = GameObject.Find(ConstructTextName(t1Step3LookupID));
            Assert.That(step3GoText.GetComponent<Text>().text, Is.EqualTo(t1step3Text), string.Format("step three text should be {0}", t1step3Text));
        }

        [UnityTest]
        public IEnumerator FSM_StartPartway()
        {
            SetupFromMiddle();
            var listOfContent = GetListOfContent();
            yield return null;

            TestIsActive(listOfContent, fsm.state);

            var step2GoText = GameObject.Find(ConstructTextName(t1Step2LookupID));
            Assert.That(step2GoText.GetComponent<Text>().text, Is.EqualTo(t1step2Text), string.Format("step two text should be {0}", t1step2Text));

            fsm.NextState();
            yield return null;

            TestIsActive(listOfContent, fsm.state);

            var step3GoText = GameObject.Find(ConstructTextName(t1Step3LookupID));
            Assert.That(step3GoText.GetComponent<Text>().text, Is.EqualTo(t1step3Text), string.Format("step three text should be {0}", t1step3Text));
        }
    }
}
#endif
