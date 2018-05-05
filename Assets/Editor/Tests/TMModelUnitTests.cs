#if UNITY_5_6_OR_NEWER
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace UnityEngine.Analytics
{

    public class TMModelTests
    {
        const string tutorialName1 = "testvalue";
        const string tutorialName2 = "testvalue-2";
        const string tutorialName3 = "testvalue-3";
        const string tutorialName4 = "testvalue-4";

        const string tutorialName1v2 = "anothertestvalue";
        const string tutorialName2v2 = "anothertestvalue-2";

        const string t1step1 = "stepvalue";
        const string t1step2 = "stepvalue-2";

        const string t1step1v2 = "anotherstepvalue";
        const string t1step2v2 = "anotherstepvalue-2";

        const string t2step1 = "t2-stepvalue";
        const string t2step2 = "t2-stepvalue-2";

        const string t1step1Text = "Here is text for tutorial one step one";
        const string t1step2Text = "Here is text for tutorial one step two";

        const string t1step1Textv2 = "Text modified for tutorial one step one";

        [Test]
        public void CreateTutorial()
        {
            var model = TutorialManagerModel.GetInstance();

            model.CreateTutorialEntity(tutorialName1);

            Assert.AreEqual(1, model.tutorials.Count, "tutorial length should be 1");
            TutorialEntity entityFromList = model.tutorials[0];
            Assert.IsNotNull(entityFromList, "entity should exist");
            Assert.AreEqual(tutorialName1, entityFromList.id, string.Format("entity id shoyuld be {0}", tutorialName1));
            TutorialEntity entityFromLookup = model.tutorialTable[tutorialName1];
            Assert.AreSame(entityFromList, entityFromLookup, "tutorial entities should be the same object");
            Assert.AreEqual(1, model.tutorialTable.Count, "tutorialTable has 1 item");

            model.CreateTutorialEntity(tutorialName2);
            Assert.AreEqual(2, model.tutorials.Count, "tutorial length should be 2");
            TutorialEntity entityFromList2 = model.tutorials[1];
            Assert.IsNotNull(entityFromList2, "second entity should exist");
            Assert.AreEqual(tutorialName2, entityFromList2.id, string.Format("second entity id should be {0}", tutorialName2));
            TutorialEntity entityFromLookup2 = model.tutorialTable[tutorialName1];
            Assert.AreSame(entityFromList2, entityFromLookup2, "tutorial entities should be the same object");
            Assert.AreEqual(2, model.tutorialTable.Count, "tutorialTable has 2 items");
        }

        [Test]
        public void UpdateTutorial()
        {
            var model = TutorialManagerModel.GetInstance();

            model.CreateTutorialEntity(tutorialName1);
            model.CreateTutorialEntity(tutorialName2);
            Assert.AreEqual(2, model.tutorials.Count, "tutorial length should be 2");

            model.UpdateTutorialEntity(tutorialName1, tutorialName1v2);

            Assert.AreEqual(2, model.tutorials.Count, "tutorial length should still be 2");

            TutorialEntity oldEntityFromLookup = model.tutorialTable[tutorialName1];
            Assert.IsNull(oldEntityFromLookup, "the old key has been destroyed");

            TutorialEntity entityFromLookup = model.tutorialTable[tutorialName1v2];
            Assert.IsNotNull(entityFromLookup, "the new key exists");
            Assert.AreEqual(tutorialName2v2, entityFromLookup.id);
            TutorialEntity entityFromList = model.tutorials[0];
            Assert.IsNotNull(entityFromList, "entity should exist");
            Assert.AreSame(entityFromList, entityFromLookup, "tutorial entities should be the same object");

            TutorialEntity entityFromList2 = model.tutorials[1];
            Assert.IsNotNull(entityFromList2, "second entity should not be destroyed");
            Assert.AreEqual(tutorialName2, entityFromList2.id, string.Format("second entity id should be unchanged", tutorialName2));
            TutorialEntity entityFromLookup2 = model.tutorialTable[tutorialName1];
            Assert.AreSame(entityFromList2, entityFromLookup2, "tutorial entities should be the same object");

            Assert.AreEqual(2, model.tutorialTable.Count, "tutorialTable has 2 items");
        }

        [Test]
        public void DestroyTutorial()
        {

            var model = TutorialManagerModel.GetInstance();

            model.CreateTutorialEntity(tutorialName1);
            model.CreateTutorialEntity(tutorialName2);
            Assert.AreEqual(2, model.tutorials.Count, "tutorial length should be 2");

            model.DestroyTutorialEntity(tutorialName1);
            Assert.AreEqual(1, model.tutorials.Count, "tutorial length should be 1");

            TutorialEntity entityFromLookup = model.tutorialTable[tutorialName2];
            Assert.IsNotNull(entityFromLookup, "the second item still exists");
            Assert.AreEqual(tutorialName2, entityFromLookup.id);
            TutorialEntity entityFromList = model.tutorials[0];
            Assert.IsNotNull(entityFromList, "entity should exist");
            Assert.AreSame(entityFromList, entityFromLookup, "tutorial entities should be the same object");

            model.DestroyTutorialEntity(tutorialName2);
            Assert.AreEqual(0, model.tutorials.Count, "tutorials should be empty");
            Assert.AreEqual(0, model.tutorialTable.Count, "tutorialTable should be empty");
        }

        [Test]
        public void CreateStep()
        {

            SetupTutorial();
            var model = TutorialManagerModel.GetInstance();
            var tutorial1 = model.tutorials[0];
            var tutorial2 = model.tutorials[1];
            var tutorial3 = model.tutorials[2];

            Assert.IsEmpty(tutorial1.steps, "tutorial 1 should exist but have no steps");
            Assert.IsEmpty(tutorial2.steps, "tutorial 2 should exist but have no steps");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should exist but have no steps");

            // Add first step to t1
            model.CreateStepEntity(t1step1, tutorial1.id);

            Assert.AreEqual(tutorial1.steps.Length, 1, "tutorial 1 should have one step");
            Assert.IsEmpty(tutorial2.steps, "tutorial 2 should still have no steps");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should still have no steps");
            Assert.AreEqual(model.stepTable.Count, 1, "model should have 1 step");

            StepEntity t1S1FromList = model.steps[0];
            StepEntity t1S1FromTable = model.stepTable[t1step1];
            Assert.IsNotNull(t1S1FromList, "new step should be in list");
            Assert.IsNotNull(t1S1FromTable, "new step should be in table");
            Assert.AreSame(t1S1FromList, t1S1FromTable, "the two entities should represent the same step");

            Assert.AreEqual(t1step1, t1S1FromList.id, string.Format("step id should be {0}", t1step1));
            Assert.AreEqual(t1step1, tutorial1.steps[0], string.Format("first step in tutorial 1 should be {0}", t1step1));

            // Add second step to t1
            model.CreateStepEntity(t1step2, tutorial1.id);

            Assert.AreEqual(tutorial1.steps.Length, 2, "tutorial 1 should have 2 steps");
            Assert.IsEmpty(tutorial2.steps, "tutorial 2 should still have no steps");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should still have no steps");
            Assert.AreEqual(model.stepTable.Count, 2, "model should have 2 steps");

            StepEntity t1S2FromList = model.steps[1];
            StepEntity t1S2FromTable = model.stepTable[t1step2];
            Assert.IsNotNull(t1S2FromList, "new step t1S2 should be in list");
            Assert.IsNotNull(t1S2FromTable, "new step t1S2 should be in table");
            Assert.AreSame(t1S2FromList, t1S2FromTable, "the two entities should represent the same step");

            Assert.AreEqual(t1step2, t1S2FromList.id, string.Format("step id should be {0}", t1step2));
            Assert.AreEqual(t1step2, tutorial1.steps[1], string.Format("second step in tutorial 1 should be {0}", t1step2));

            // Add first step to t2
            model.CreateStepEntity(t2step1, tutorial2.id);

            Assert.AreEqual(tutorial1.steps.Length, 2, "tutorial 1 should have 2 steps");
            Assert.AreEqual(tutorial2.steps.Length, 1, "tutorial 2 should have 1 step");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should still have no steps");
            Assert.AreEqual(model.stepTable.Count, 1, "model should have 3 step");

            StepEntity t2S1FromList = model.steps[2];
            StepEntity t2S1FromTable = model.stepTable[t2step1];
            Assert.IsNotNull(t2S1FromList, "new step t2S1 hould be in list");
            Assert.IsNotNull(t2S1FromTable, "new step t2S1 should be in table");
            Assert.AreSame(t2S1FromList, t2S1FromTable, "the two entities should represent the same step");

            Assert.AreEqual(t2step1, t2S1FromList.id, string.Format("step id should be {0}", t2step1));
            Assert.AreEqual(t2step1, tutorial2.steps[0], string.Format("first step in tutorial 2 should be {0}", t2step1));

            // Add second step to t2
            model.CreateStepEntity(t2step2, tutorial2.id);

            Assert.AreEqual(tutorial1.steps.Length, 2, "tutorial 1 should have 2 steps");
            Assert.AreEqual(tutorial2.steps.Length, 2, "tutorial 2 should have 2 steps");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should still have no steps");
            Assert.AreEqual(model.stepTable.Count, 4, "model should have 2 steps");

            StepEntity t2S2FromList = model.steps[3];
            StepEntity t2S2FromTable = model.stepTable[t2step2];
            Assert.IsNotNull(t2S2FromList, "new step t2S2 should be in list");
            Assert.IsNotNull(t2S2FromTable, "new step t2S2 should be in table");
            Assert.AreSame(t2S2FromList, t2S2FromTable, "the two entities should represent the same step");

            Assert.AreEqual(t2step2, t2S2FromList.id, string.Format("step id should be {0}", t2step2));
            Assert.AreEqual(t2step2, tutorial2.steps[1], string.Format("second step in tutorial 1 should be {0}", t2step2));

        }

        [Test]
        public void UpdateStep()
        {
            SetupTutorial();
            SetupSteps();
            var model = TutorialManagerModel.GetInstance();
            var tutorial1 = model.tutorials[0];
            var tutorial2 = model.tutorials[1];
            var tutorial3 = model.tutorials[2];

            model.UpdateStepEntity(t1step1, t1step1v2);

            Assert.AreEqual(2, tutorial1.steps.Length, "tutorial 1 should still have 2 steps");
            Assert.IsFalse(model.stepTable.ContainsKey(t1step1), "the old t1S1 key should be gone");
            Assert.IsTrue(model.stepTable.ContainsKey(t1step1v2), "there should be a new t1step1v2 key");

            StepEntity stepEntity = model.stepTable[t1step1v2];
            Assert.AreEqual(t1step1v2, stepEntity.id, string.Format("step id should be {0}", t1step1v2));
            Assert.AreEqual(t1step2, model.stepTable[t1step2].id, "other step t1step2 should be unchanged");

            EnsureUnaffectedSteps();
        }

        [Test]
        public void DestroyStep()
        {
            SetupTutorial();
            SetupSteps();

            var model = TutorialManagerModel.GetInstance();
            var tutorial1 = model.tutorials[0];
            var tutorial2 = model.tutorials[1];
            var tutorial3 = model.tutorials[2];

            int stepTableCount = model.stepTable.Count;
            int stepListCount = model.steps.Count;

            model.DestroyStepEntity(t1step1);

            Assert.AreEqual(1, tutorial1.steps.Length, "tutorial 1 should have just 1 step");
            Assert.AreEqual(t1step2, tutorial1.steps[0], "the first step item in tutorial 1 should be t1step2");
            Assert.AreEqual(1, stepTableCount - model.stepTable.Count, "stepTable count should have decremented by one");
            Assert.AreEqual(1, stepListCount - model.steps.Count, "steps count should have decremented by one");
            Assert.IsFalse(model.stepTable.ContainsKey(t1step1), "the step should have been removed from the table");

            Assert.IsFalse(FoundKeyInList(t1step1, tutorial1.steps), "content should have been removed from step list");

            model.DestroyStepEntity(t1step2);

            Assert.IsEmpty(tutorial1.steps, "tutorial 1 should be empty");
            Assert.AreEqual(2, stepTableCount - model.stepTable.Count, "stepTable count should have decremented by two");
            Assert.AreEqual(2, stepListCount - model.steps.Count, "steps count should have decremented by two");
            Assert.IsFalse(model.stepTable.ContainsKey(t1step2), "the step should have been removed from the table");

            EnsureUnaffectedSteps();
        }

        [Test]
        public void CreateText()
        {
            SetupTutorial();
            SetupSteps();

            var model = TutorialManagerModel.GetInstance();
            var t1s1 = model.stepTable[t1step1];
            var t1s2 = model.stepTable[t1step2];

            // Add content to s1
            model.CreateContentEntity(t1step1Text, "text", t1step1);
            Assert.AreEqual(t1s1.messaging.content.Length, 1, "step 1 should have one content item");
            Assert.IsEmpty(t1s2.messaging.content, "step 2 should still have no steps");
            Assert.AreEqual(model.contentTable.Count, 1, "model should have 1 content item");

            ContentEntity t1S1textFromList = model.content[0];
            ContentEntity t1S1textFromTable = model.contentTable[t1step1];
            Assert.IsNotNull(t1S1textFromList, "new content item should be in list");
            Assert.IsNotNull(t1S1textFromTable, "new content item should be in table");
            Assert.AreSame(t1S1textFromList, t1S1textFromTable, "the two entities should represent the same content item");

            // Check the content of the object
            Assert.AreEqual("text", t1S1textFromTable.type);
            Assert.AreEqual(t1step1Text, t1S1textFromTable.text);
            Assert.AreEqual(t1step1, t1S1textFromTable.id);

            Assert.AreEqual(t1step1, t1S1textFromTable.id, string.Format("content id should be {0}", t1step1));
            Assert.AreEqual(t1step1, t1s1.messaging.content[0], string.Format("first step in tutorial 1 should be {0}", t1step1));

            // Add content to s2
            model.CreateContentEntity(t1step2Text, "text", t1step2);
            Assert.AreEqual(t1s2.messaging.content.Length, 1, "step 2 should have one content item");
            Assert.IsEmpty(t1s1.messaging.content, "step 1 should still have 1 step");
            Assert.AreEqual(model.contentTable.Count, 2, "model should have 2 content items");

            ContentEntity t1S2textFromList = model.content[1];
            ContentEntity t1S2textFromTable = model.contentTable[t1step2];
            Assert.IsNotNull(t1S2textFromList, "new content item should be in list");
            Assert.IsNotNull(t1S2textFromTable, "new content item should be in table");
            Assert.AreSame(t1S2textFromList, t1S2textFromTable, "the two entities should represent the same content item");

            Assert.AreEqual(t1step2, t1S1textFromTable.id, string.Format("content id should be {0}", t1step2));
            Assert.AreEqual(t1step2, t1s1.messaging.content[0], string.Format("first step in tutorial 1 should be {0}", t1step2));

            EnsureUnaffectedSteps();
        }

        [Test]
        public void UpdateText()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();

            var model = TutorialManagerModel.GetInstance();
            var step1 = model.steps[0];
            var tutorial1 = model.tutorials[0];

            var content = model.contentTable[t1step1];

            Assert.AreEqual(t1step1Text, content.text, "content should have original text");
            model.UpdateContentEntity(t1step1, t1step1Textv2);
            Assert.AreEqual(t1step1Textv2, content.text, "content should have updated text");

            EnsureUnaffectedSteps();
        }

        [Test]
        public void DestroyText()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();

            var model = TutorialManagerModel.GetInstance();
            int contentListCount = model.content.Count;
            int contentTableCount = model.contentTable.Count;
            Assert.AreEqual(t1step1, model.content[0], "the original content 0 was in first position");

            model.DestroyContentEntity(t1step1);

            Assert.AreEqual(1, contentListCount - model.content.Count, "content list should have lost an item");
            Assert.AreEqual(1, contentTableCount - model.contentTable.Count, "content table should have lost an item");
            Assert.AreEqual(t1step2, model.content[0], "content 2 is now in the first position");
            Assert.IsFalse(model.contentTable.ContainsKey(t1step1), "original content removed from the content table");

        }

        void SetupTutorial() {
            var model = TutorialManagerModel.GetInstance();
            model.CreateTutorialEntity(tutorialName1);
            model.CreateTutorialEntity(tutorialName2);
            model.CreateTutorialEntity(tutorialName3);
            model.CreateTutorialEntity(tutorialName4);
        }

        void SetupSteps() {
            var model = TutorialManagerModel.GetInstance();
            var tutorial1 = model.tutorials[0];
            var tutorial2 = model.tutorials[1];
            var tutorial3 = model.tutorials[2];

            model.CreateStepEntity(t1step1, tutorial1.id);
            model.CreateStepEntity(t1step2, tutorial1.id);
            model.CreateStepEntity(t2step2, tutorial2.id);
            model.CreateStepEntity(t2step2, tutorial2.id);

            Assert.AreEqual(2, tutorial1.steps.Length, "tutorial 1 should have 2 steps");
            Assert.AreEqual(2, tutorial2.steps.Length, "tutorial 2 should have 2 steps");
            Assert.AreEqual(0, tutorial3.steps.Length, "tutorial 3 should have no steps");
        }

        void SetupContent() {
            var model = TutorialManagerModel.GetInstance();

            var t1s1 = model.stepTable[t1step1];
            var t1s2 = model.stepTable[t1step2];

            model.CreateContentEntity(t1step1Text, "text", t1step1);
            model.CreateContentEntity(t1step2Text, "text", t1step2);

            Assert.AreEqual(2, model.content.Count, "content should have 2 items");
            Assert.AreEqual(2, model.contentTable.Count, "contentTable should have 2 items");
            Assert.AreEqual(2, t1s1.messaging.content.Length, "t1s1 content should have 2 items");
            Assert.AreEqual(0, t1s2.messaging.content.Length, "t1s2 should have 0 items");
        }

        void EnsureUnaffectedSteps() {
            var model = TutorialManagerModel.GetInstance();
            var tutorial2 = model.tutorials[1];
            var tutorial3 = model.tutorials[2];

            Assert.AreEqual(2, tutorial2.steps.Length, "tutorial 2 should still have 2 steps");
            Assert.AreEqual(0, tutorial3.steps.Length, "tutorial 3 should still have no steps");
            Assert.AreEqual(t2step1, model.stepTable[t2step1].id, "t2step1 should be unchanged");
            Assert.AreEqual(t2step2, model.stepTable[t2step2].id, "t2step2 should be unchanged");
        }

        bool FoundKeyInList(string key, string[] list)
        {
            for (int a = 0; a < list.Length; a++)
            {
                if (list[a] == key)
                {
                    return true;
                }
            }
            return false;
        }
    }


#endif
}