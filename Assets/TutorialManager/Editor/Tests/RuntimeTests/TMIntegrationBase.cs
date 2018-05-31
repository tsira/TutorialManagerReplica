using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UI;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics
{
    /// <summary>
    /// This parent class provices properties and methods for integration tests on the Tutorial Manager.
    /// </summary>
    public class TMIntegrationBase
    {

        protected const string tutorialName1 = "tutorial1";
        protected const string tutorialName2 = "tutorial2";
        protected const string tutorialName3 = "tutorial3";
        protected const string tutorialName4 = "tutorial4";

        protected const string tutorialName1v2 = "anothertutorialname";
        protected const string tutorialName2v2 = "anothertutorialname-2";

        protected const string stepBaseID = "step1";
        protected const string stepBaseID2 = "step2";
        protected const string stepBaseID3 = "step3";

        protected string t1Step1 = stepBaseID;
        protected string t1Step2 = stepBaseID2;
        protected string t1Step3 = stepBaseID3;

        protected string t1Step1LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID);
        protected string t1Step2LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID2);
        protected string t1Step3LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID3);

        protected const string t1step1v2 = "anotherstepname";
        protected const string t1step2v2 = "anotherstepname2";
        protected const string t1step2v3 = "anotherstepname3";

        protected const string t2step1 = "t2sname1";
        protected const string t2Step2 = "t2sname2";

        protected const string t2step1LookupID = "tutorial2-stepname";
        protected const string t2step2LookupID = "tutorial2-stepname2";

        protected const string t1step1Text = "Here is text for tutorial one step one";
        protected const string t1step2Text = "Here is text for tutorial one step two";
        protected const string t1step3Text = "Here is text for tutorial one step three";

        protected const string t1step1Textv2 = "Text modified for tutorial one step one";
        protected const string t1step2Textv2 = "Text modified for tutorial one step two";

        protected TutorialManagerModelMiddleware modelMiddleware;
        protected AdaptiveStateDispatcher dispatcher;
        protected TutorialManagerFSM fsm;

        protected StateSystemProvider provider;

        // Utilities ///////////////////////////////////////

        protected void TestIsActive(List<GameObject> list, string activeId)
        {
            foreach (GameObject go in list) {
                if (go.name.IndexOf(activeId, 0, StringComparison.CurrentCulture) > -1) {
                    Assert.IsTrue(go.activeSelf, string.Format("{0} active should be true when state is {1}", go.name, activeId));
                } else {
                    Assert.IsFalse(go.activeSelf, string.Format("{0} active should be false when state is {1}", go.name, activeId));
                }
            }
        }

        protected List<GameObject> GetListOfContent()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial1 = model.TMData.tutorials[0];
            var tutorial2 = model.TMData.tutorials[1];
            var tutorial3 = model.TMData.tutorials[2];
            var tutorials = new List<TutorialEntity> { tutorial1, tutorial2, tutorial3 };

            var list = new List<GameObject>();
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


        protected void SetupTutorial()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            model.Clear();
            model.CreateTutorialEntity(tutorialName1);
            model.CreateTutorialEntity(tutorialName2);
            model.CreateTutorialEntity(tutorialName3);
            model.CreateTutorialEntity(tutorialName4);
        }

        protected void SetupSteps()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial1 = model.TMData.tutorials[0];
            var tutorial2 = model.TMData.tutorials[1];
            var tutorial3 = model.TMData.tutorials[2];

            model.CreateStepEntity(t1Step1, tutorial1.id);
            model.CreateStepEntity(t1Step2, tutorial1.id);
            model.CreateStepEntity(t1Step3, tutorial1.id);
            model.CreateStepEntity(t2step1, tutorial2.id);
            model.CreateStepEntity(t2Step2, tutorial2.id);

            Assert.AreEqual(3, tutorial1.steps.Count, "tutorial 1 should have 3 steps");
            Assert.AreEqual(2, tutorial2.steps.Count, "tutorial 2 should have 2 steps");
            Assert.AreEqual(0, tutorial3.steps.Count, "tutorial 3 should have no steps");
        }

        protected void SetupContent()
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

        protected void SetupAdaptiveContent()
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

        protected GameObject SetupAdaptiveContentComponent(string bindingId, int a)
        {
            var go = new GameObject();
            go.name = ConstructContentName(bindingId, a);
            var adaptiveContent = go.AddComponent<AdaptiveContent>();
            adaptiveContent.bindingId = bindingId;
            return go;
        }

        protected GameObject SetupAdaptiveTextComponent(string bindingId)
        {
            var go = new GameObject();
            go.name = ConstructTextName(bindingId);
            go.AddComponent<Text>();
            var adaptiveContent = go.AddComponent<AdaptiveText>();
            adaptiveContent.bindingId = bindingId;
            return go;
        }

        protected string ConstructContentName(string id, int a)
        {
            return string.Format("{0}-{1}", id, a);
        }

        protected string ConstructTextName(string id)
        {
            return string.Format("{0}-{1}", id, "text");
        }
    }
}