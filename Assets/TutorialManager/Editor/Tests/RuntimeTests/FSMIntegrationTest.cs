/// <summary>
/// This is an automated runtime integration test, intended to demonstrate that – absent the porcelain
/// of the TutorialManager class – the underlying state machine, dispatcher, and components work.
/// </summary>

using System;
using System.Collections;
using UnityEngine.Analytics.TutorialManagerRuntime;
using UnityEngine.Analytics;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.Analytics
{
    public class FSMIntegrationTest : MonoBehaviour
    {
        const string tutorialName1 = "tutorial1";
        const string tutorialName2 = "tutorial2";
        const string tutorialName3 = "tutorial3";
        const string tutorialName4 = "tutorial4";

        const string tutorialName1v2 = "anothertutorialname";
        const string tutorialName2v2 = "anothertutorialname-2";

        const string stepBaseID = "step1";
        const string stepBaseID2 = "step2";
        const string stepBaseID3 = "step3";

        string t1step1 = stepBaseID;
        string t1step2 = stepBaseID2;
        string t1step3 = stepBaseID3;

        string t1Step1LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID);
        string t1Step2LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID2);
        string t1Step3LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID3);

        const string t1step1v2 = "anotherstepname";
        const string t1step2v2 = "anotherstepname2";
        const string t1step2v3 = "anotherstepname3";

        const string t2step1 = "t2sname1";
        const string t2step2 = "t2sname2";

        const string t2step1LookupID = "tutorial2-stepname";
        const string t2step2LookupID = "tutorial2-stepname2";

        const string t1step1Text = "Here is text for tutorial one step one";
        const string t1step2Text = "Here is text for tutorial one step two";
        const string t1step3Text = "Here is text for tutorial one step three";

        const string t1step1Textv2 = "Text modified for tutorial one step one";
        const string t1step2Textv2 = "Text modified for tutorial one step two";

        TutorialManagerModelMiddleware modelMiddleware;
        AdaptiveStateDispatcher dispatcher;
        TutorialManagerFSM fsm;

        StateSystemProvider provider;


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
        public IEnumerator FSM_UpdateStartingPartway()
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

        // Utilities ///////////////////////////////////////

        void TestIsActive(List<GameObject> list, string activeId)
        {
            foreach (GameObject go in list) {
                if (go.name.IndexOf(activeId, 0, StringComparison.CurrentCulture) > -1) {
                    Assert.IsTrue(go.activeSelf, string.Format("{0} active should be true when state is {1}", go.name, activeId));
                } else {
                    Assert.IsFalse(go.activeSelf, string.Format("{0} active should be false when state is {1}", go.name, activeId));
                }
            }
        }

        List<GameObject> GetListOfContent()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial1 = model.TMData.tutorials[0];
            var tutorial2 = model.TMData.tutorials[1];
            var tutorial3 = model.TMData.tutorials[2];
            var tutorials = new List<TutorialEntity> { tutorial1, tutorial2, tutorial3 };

            var list = new List<GameObject> ();
            foreach (TutorialEntity t in tutorials) {
                for (int a = 0; a < t.steps.Count; a++) {
                    var stepId = t.steps[a];
                    for (int b = 0; b < t.steps.Count; b++) {
                        var go = GameObject.Find(ConstructContentName(stepId, b));
                        list.Add(go);
                    }
                    var goText = GameObject.Find(ConstructTextName(stepId));
                    list.Add(goText);
                }
            }
            return list;
        }

        string ConstructContentName(string id, int a)
        {
            return string.Format("{0}-{1}", id, a);
        }

        string ConstructTextName(string id)
        {
            return string.Format("{0}-{1}", id, "text");
        }


        // Setup ///////////////////////////////////////

        void SetupFromStart()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();

            SetupAdaptiveContent();

            modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
            dispatcher = AdaptiveStateDispatcher.GetInstance();
            provider = StateSystemProvider.GetInstance();

            fsm = new TutorialManagerFSM();
            fsm.stateList = modelMiddleware.TMData.tutorials[0].steps;
            fsm.dispatcher = dispatcher;

            provider.SetDispatcher(dispatcher);
            provider.SetDataStore(modelMiddleware);
        }

        void SetupFromMiddle()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();

            modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
            dispatcher = AdaptiveStateDispatcher.GetInstance();
            provider = StateSystemProvider.GetInstance();

            fsm = new TutorialManagerFSM();
            fsm.stateList = modelMiddleware.TMData.tutorials[0].steps;
            fsm.dispatcher = dispatcher;
            fsm.GoToState(t1Step2LookupID);

            //Content comes along *after* the state is at step 2
            SetupAdaptiveContent();

            provider.SetDispatcher(dispatcher);
            provider.SetDataStore(modelMiddleware);
        }


        void SetupTutorial()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            model.Clear();
            model.CreateTutorialEntity(tutorialName1);
            model.CreateTutorialEntity(tutorialName2);
            model.CreateTutorialEntity(tutorialName3);
            model.CreateTutorialEntity(tutorialName4);
        }

        void SetupSteps()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial1 = model.TMData.tutorials[0];
            var tutorial2 = model.TMData.tutorials[1];
            var tutorial3 = model.TMData.tutorials[2];

            model.CreateStepEntity(t1step1, tutorial1.id);
            model.CreateStepEntity(t1step2, tutorial1.id);
            model.CreateStepEntity(t1step3, tutorial1.id);
            model.CreateStepEntity(t2step1, tutorial2.id);
            model.CreateStepEntity(t2step2, tutorial2.id);

            Assert.AreEqual(3, tutorial1.steps.Count, "tutorial 1 should have 3 steps");
            Assert.AreEqual(2, tutorial2.steps.Count, "tutorial 2 should have 2 steps");
            Assert.AreEqual(0, tutorial3.steps.Count, "tutorial 3 should have no steps");
        }

        void SetupContent()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();

            var t1s1 = model.TMData.stepTable[t1Step1LookupID];
            var t1s2 = model.TMData.stepTable[t1Step2LookupID];

            model.CreateContentEntity(t1Step1LookupID, ContentType.text, t1step1Text);
            model.CreateContentEntity(t1Step2LookupID, ContentType.text, t1step2Text);
            model.CreateContentEntity(t1Step3LookupID, ContentType.text, t1step3Text);

            Assert.AreEqual(3, model.TMData.content.Count, "content should have 3 items");
            Assert.AreEqual(3, model.TMData.contentTable.Count, "contentTable should have 3 items");
            Assert.AreEqual(1, t1s1.messaging.content.Count, "t1s1 content should have 1 items");
            Assert.AreEqual(1, t1s2.messaging.content.Count, "t1s2 should have 1 items");
        }

        void SetupAdaptiveContent()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial1 = model.TMData.tutorials[0];
            var tutorial2 = model.TMData.tutorials[1];
            var tutorial3 = model.TMData.tutorials[2];
            var tutorials = new List<TutorialEntity> { tutorial1, tutorial2, tutorial3 };

            for (int t = 0; t < tutorials.Count; t++) {
                for (int a = 0; a < tutorials[t].steps.Count; a++) {
                    var stepId = tutorials[t].steps[a];
                    for (int b = 0; b < tutorials[t].steps.Count; b++) {
                        SetupAdaptiveContentComponent(stepId, b);
                    }
                    SetupAdaptiveTextComponent(stepId);
                }
            }
        }

        GameObject SetupAdaptiveContentComponent(string bindingId, int a)
        {
            var go = new GameObject();
            go.name = ConstructContentName(bindingId, a);
            var adaptiveContent = go.AddComponent<AdaptiveContent>();
            adaptiveContent.bindingId = bindingId;
            return go;
        }

        GameObject SetupAdaptiveTextComponent(string bindingId)
        {
            var go = new GameObject();
            go.name = ConstructTextName(bindingId);
            go.AddComponent<Text>();
            var adaptiveContent = go.AddComponent<AdaptiveText>();
            adaptiveContent.bindingId = bindingId;
            return go;
        }
    }

}