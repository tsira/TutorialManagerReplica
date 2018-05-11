#if UNITY_5_6_OR_NEWER
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Analytics.TutorialManagerRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace UnityEngine.Analytics
{

    public class TMModelTests
    {
        const string tutorialName1 = "tutorial";
        const string tutorialName2 = "tutorial-2";
        const string tutorialName3 = "tutorial-3";
        const string tutorialName4 = "tutorial-4";

        const string tutorialName1v2 = "anothertutorialname";
        const string tutorialName2v2 = "anothertutorialname-2";



        const string stepBaseID = "step";
        const string stepBaseID2 = "step2";


        string t1step1 = stepBaseID;
        string t1step2 = stepBaseID2;

        string t1Step1LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID);
        string t1Step2LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID2);


        const string t1step1v2 = "anotherstepname";
        const string t1step2v2 = "anotherstepname-2";

        const string t1step1v2LookupID = "tutorial-anotherstepname";
        const string t1step2v2LookupID = "tutorial-anotherstepname-2";

        const string t2step1 = "stepname";
        const string t2step2 = "stepname-2";

        const string t2step1LookupID = "tutorial-2-stepname";
        const string t2step2LookupID = "tutorial-2-stepname-2";

        const string t1step1Text = "Here is text for tutorial one step one";
        const string t1step2Text = "Here is text for tutorial one step two";

        const string t1step1Textv2 = "Text modified for tutorial one step one";


        [Test]
        public void CreateTutorial()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            model.Clear();

            model.CreateTutorialEntity(tutorialName1);

            Assert.AreEqual(1, model.TMData.tutorials.Count, "tutorial length should be 1");
            TutorialEntity entityFromList = model.TMData.tutorials[0];
            Assert.IsNotNull(entityFromList, "entity should exist");
            Assert.That(tutorialName1, Is.EqualTo(entityFromList.id), string.Format("entity id shoyuld be {0}", tutorialName1));
            TutorialEntity entityFromLookup = model.TMData.tutorialTable[tutorialName1];

            Assert.AreSame(entityFromList, entityFromLookup, "tutorial entities should be the same object");
            Assert.AreEqual(1, model.TMData.tutorialTable.Count, "tutorialTable has 1 item");

            model.CreateTutorialEntity(tutorialName2);
            Assert.AreEqual(2, model.TMData.tutorials.Count, "tutorial length should be 2");
            TutorialEntity entityFromList2 = model.TMData.tutorials[1];
            Assert.IsNotNull(entityFromList2, "second entity should exist");
            Assert.That(tutorialName2, Is.EqualTo(entityFromList2.id), string.Format("second entity id should be {0}", tutorialName2));
            TutorialEntity entityFromLookup2 = model.TMData.tutorialTable[tutorialName2];
            Assert.AreSame(entityFromList2, entityFromLookup2, "tutorial entities should be the same object");
            Assert.AreEqual(2, model.TMData.tutorialTable.Count, "tutorialTable has 2 items");

            PostTestCleanup();
        }

        [Test]
        public void UpdateTutorialWhenNoStepsOrContent()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            model.Clear();

            model.CreateTutorialEntity(tutorialName1);
            model.CreateTutorialEntity(tutorialName2);
            Assert.AreEqual(2, model.TMData.tutorials.Count, "tutorial length should be 2");

            model.UpdateTutorialEntity(tutorialName1, tutorialName1v2);

            Assert.AreEqual(2, model.TMData.tutorials.Count, "tutorial length should still be 2");

            Assert.IsFalse(model.TMData.tutorialTable.ContainsKey(tutorialName1), "the old key has been destroyed");

            TutorialEntity entityFromLookup = model.TMData.tutorialTable[tutorialName1v2];
            Assert.IsNotNull(entityFromLookup, "the new key exists");
            Assert.That(tutorialName1v2, Is.EqualTo(entityFromLookup.id), "tutorial name should be changed");
            TutorialEntity entityFromList = model.TMData.tutorials[0];
            Assert.IsNotNull(entityFromList, "entity should exist");
            Assert.AreSame(entityFromList, entityFromLookup, "tutorial entities should be the same object");

            TutorialEntity entityFromList2 = model.TMData.tutorials[1];
            Assert.IsNotNull(entityFromList2, "second entity should not be destroyed");
            Assert.That(tutorialName2, Is.EqualTo(entityFromList2.id), "second entity id should be unchanged");
            TutorialEntity entityFromLookup2 = model.TMData.tutorialTable[tutorialName2];
            Assert.AreSame(entityFromList2, entityFromLookup2, "tutorial entities should be the same object");

            Assert.AreEqual(2, model.TMData.tutorialTable.Count, "tutorialTable has 2 items");

            PostTestCleanup();
        }

        [Test]
        public void UpdateTutorialWhenSteps()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            model.Clear();

            SetupTutorial();
            SetupSteps();

            var tutorial1 = model.TMData.tutorials[0];

            Assert.AreEqual(2, tutorial1.steps.Count, "tutorial 1 should still have 2 steps");
            Assert.IsFalse(model.TMData.stepTable.ContainsKey(t1step1), "the old t1S1 key should be gone");
            Assert.IsFalse(model.TMData.stepTable.ContainsKey(t1step2), "the old t1S2 key should be gone");

            var newStep1Id = ConstructID(tutorialName1, stepBaseID);
            var newStep2Id = ConstructID(tutorialName1, stepBaseID2);

            Assert.IsTrue(model.TMData.stepTable.ContainsKey(newStep1Id), string.Format("new key should be {0}", newStep1Id));
            Assert.IsTrue(model.TMData.stepTable.ContainsKey(newStep2Id), string.Format("new key should be {0}", newStep2Id));
            Assert.That(newStep1Id, Is.EqualTo(model.TMData.stepTable[newStep1Id].id), "renamed tutorial should cascade and rename step 1");
            Assert.That(newStep2Id, Is.EqualTo(model.TMData.stepTable[newStep2Id].id), "renamed tutorial should cascade and rename step 2");

            PostTestCleanup();
        }

        [Test]
        public void UpdateTutorialWhenStepsAndContent()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            model.Clear();

            SetupTutorial();
            SetupSteps();
            SetupContent();

            var tutorial1 = model.TMData.tutorials[0];

            var content1Id = ConstructID(t1Step1LookupID, ContentType.text.ToString());
            var content2Id = ConstructID(t1Step2LookupID, ContentType.text.ToString());

            Assert.IsTrue(model.TMData.contentTable.ContainsKey(content1Id), "should have content key 1");
            Assert.IsTrue(model.TMData.contentTable.ContainsKey(content2Id), "should have content key 2");

            model.UpdateTutorialEntity(tutorial1.id, tutorialName1v2);

            Assert.AreEqual(2, model.TMData.contentTable.Count, "should still have 2 content elements");
            Assert.IsFalse(model.TMData.contentTable.ContainsKey(content1Id), "the old key 1 should be gone");
            Assert.IsFalse(model.TMData.contentTable.ContainsKey(content1Id), "the old key 2 should be gone");

            var newStep1Id = ConstructID(tutorialName1v2, stepBaseID);
            var newStep2Id = ConstructID(tutorialName1v2, stepBaseID2);
            var newContent1Id = ConstructID(newStep1Id, ContentType.text.ToString());
            var newContent2Id = ConstructID(newStep2Id, ContentType.text.ToString());


            Assert.IsTrue(model.TMData.contentTable.ContainsKey(newContent1Id), "should have updated content key 1");
            Assert.IsTrue(model.TMData.contentTable.ContainsKey(newContent2Id), "should have updated content key 2");

            Assert.That(newContent1Id, Is.EqualTo(model.TMData.contentTable[newContent1Id].id), "renamed tutorial should cascade and rename content 1");
            Assert.That(newContent2Id, Is.EqualTo(model.TMData.contentTable[newContent2Id].id), "renamed tutorial should cascade and rename content 2");

            PostTestCleanup();
        }

        [Test]
        public void DestroyTutorial()
        {

            var model = TutorialManagerModelMiddleware.GetInstance();
            model.Clear();

            model.CreateTutorialEntity(tutorialName1);
            model.CreateTutorialEntity(tutorialName2);
            Assert.AreEqual(2, model.TMData.tutorials.Count, "tutorial length should be 2");

            model.DestroyTutorialEntity(tutorialName1);
            Assert.AreEqual(1, model.TMData.tutorials.Count, "tutorial length should be 1");

            TutorialEntity entityFromLookup = model.TMData.tutorialTable[tutorialName2];
            Assert.IsNotNull(entityFromLookup, "the second tutorial still exists");
            Assert.That(tutorialName2, Is.EqualTo(entityFromLookup.id), "tutorial id 2 is unaffected");
            TutorialEntity entityFromList = model.TMData.tutorials[0];
            Assert.IsNotNull(entityFromList, "entity should exist");
            Assert.AreSame(entityFromList, entityFromLookup, "tutorial entities should be the same object");

            model.DestroyTutorialEntity(tutorialName2);
            Assert.AreEqual(0, model.TMData.tutorials.Count, "tutorials should be empty");
            Assert.AreEqual(0, model.TMData.tutorialTable.Count, "tutorialTable should be empty");

            PostTestCleanup();
        }

        [Test]
        public void CreateStep()
        {
            SetupTutorial();
            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial1 = model.TMData.tutorials[0];
            var tutorial2 = model.TMData.tutorials[1];
            var tutorial3 = model.TMData.tutorials[2];

            Assert.IsEmpty(tutorial1.steps, "tutorial 1 should exist but have no steps");
            Assert.IsEmpty(tutorial2.steps, "tutorial 2 should exist but have no steps");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should exist but have no steps");

            // Add first step to t1
            model.CreateStepEntity(t1step1, tutorial1.id);


            Assert.AreEqual(1, tutorial1.steps.Count, "tutorial 1 should have one step");
            Assert.IsEmpty(tutorial2.steps, "tutorial 2 should still have no steps");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should still have no steps");
            Assert.AreEqual(1, model.TMData.stepTable.Count, "model should have 1 step");

            StepEntity t1S1FromList = model.TMData.steps[0];
            StepEntity t1S1FromTable = model.TMData.stepTable[t1Step1LookupID];
            Assert.IsNotNull(t1S1FromList, "new step should be in list");
            Assert.IsNotNull(t1S1FromTable, "new step should be in table");
            Assert.AreSame(t1S1FromList, t1S1FromTable, "the two entities should represent the same step");



            Assert.That(t1Step1LookupID, Is.EqualTo(t1S1FromList.id), string.Format("step id should be {0}", t1Step1LookupID));
            Assert.That(t1Step1LookupID, Is.EqualTo(tutorial1.steps[0]), string.Format("first step in tutorial 1 should be {0}", t1Step1LookupID));

            // Add second step to t1
            model.CreateStepEntity(t1step2, tutorial1.id);

            Assert.AreEqual(2, tutorial1.steps.Count, "tutorial 1 should have 2 steps");
            Assert.IsEmpty(tutorial2.steps, "tutorial 2 should still have no steps");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should still have no steps");
            Assert.AreEqual(2, model.TMData.stepTable.Count, "model should have 2 steps");

            StepEntity t1S2FromList = model.TMData.steps[1];
            StepEntity t1S2FromTable = model.TMData.stepTable[t1Step2LookupID];
            Assert.IsNotNull(t1S2FromList, "new step t1S2 should be in list");
            Assert.IsNotNull(t1S2FromTable, "new step t1S2 should be in table");
            Assert.AreSame(t1S2FromList, t1S2FromTable, "the two entities should represent the same step");


            Assert.That(t1Step2LookupID, Is.EqualTo(t1S2FromList.id), string.Format("step id should be {0}", t1Step2LookupID));
            Assert.That(t1Step2LookupID, Is.EqualTo(tutorial1.steps[1]), string.Format("second step in tutorial 1 should be {0}", t1Step2LookupID));

            // Add first step to t2
            model.CreateStepEntity(t2step1, tutorial2.id);

            Assert.AreEqual(2, tutorial1.steps.Count, "tutorial 1 should have 2 steps");
            Assert.AreEqual(1, tutorial2.steps.Count, "tutorial 2 should have 1 step");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should still have no steps");
            Assert.AreEqual(3, model.TMData.stepTable.Count, "model should have 3 steps");

            StepEntity t2S1FromList = model.TMData.steps[2];
            StepEntity t2S1FromTable = model.TMData.stepTable[t2step1LookupID];
            Assert.IsNotNull(t2S1FromList, "new step t2S1 hould be in list");
            Assert.IsNotNull(t2S1FromTable, "new step t2S1 should be in table");
            Assert.AreSame(t2S1FromList, t2S1FromTable, "the two entities should represent the same step");

            Assert.That(t2step1LookupID, Is.EqualTo(t2S1FromList.id), string.Format("step id should be {0}", t2step1LookupID));
            Assert.That(t2step1LookupID, Is.EqualTo(tutorial2.steps[0]), string.Format("first step in tutorial 2 should be {0}", t2step1LookupID));

            // Add second step to t2
            model.CreateStepEntity(t2step2, tutorial2.id);

            Assert.AreEqual(2, tutorial1.steps.Count, "tutorial 1 should have 2 steps");
            Assert.AreEqual(2, tutorial2.steps.Count, "tutorial 2 should have 2 steps");
            Assert.IsEmpty(tutorial3.steps, "tutorial 3 should still have no steps");
            Assert.AreEqual(4, model.TMData.stepTable.Count, "model should have 4 steps");

            StepEntity t2S2FromList = model.TMData.steps[3];
            StepEntity t2S2FromTable = model.TMData.stepTable[t2step2LookupID];
            Assert.IsNotNull(t2S2FromList, "new step t2S2 should be in list");
            Assert.IsNotNull(t2S2FromTable, "new step t2S2 should be in table");
            Assert.AreSame(t2S2FromList, t2S2FromTable, "the two entities should represent the same step");

            Assert.That(t2step2LookupID, Is.EqualTo(t2S2FromList.id), string.Format("step id should be {0}", t2step2LookupID));
            Assert.That(t2step2LookupID, Is.EqualTo(tutorial2.steps[1]), string.Format("second step in tutorial 1 should be {0}", t2step2LookupID));

            PostTestCleanup();
        }

        [Test]
        public void UpdateStepWhenNoContent()
        {
            SetupTutorial();
            SetupSteps();
            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial1 = model.TMData.tutorials[0];

            model.UpdateStepEntity(t1Step1LookupID, t1step1v2LookupID);

            Assert.AreEqual(2, tutorial1.steps.Count, "tutorial 1 should still have 2 steps");
            Assert.IsFalse(tutorial1.steps.Contains(t1step1), "the old t1S1 key should be gone from the Tutorial steps");
            Assert.IsFalse(model.TMData.stepTable.ContainsKey(t1step1), "the old t1S1 key should be gone from the stepTable");
            Assert.IsTrue(tutorial1.steps.Contains(t1step1v2LookupID), "the new t1step1v2 key should be in from the Tutorial steps");
            Assert.IsTrue(model.TMData.stepTable.ContainsKey(t1step1v2LookupID), "there should be a new t1step1v2 key in the stepTable");

            StepEntity stepEntity = model.TMData.stepTable[t1step1v2LookupID];
            Assert.That(t1step1v2LookupID, Is.EqualTo(stepEntity.id), string.Format("step id should be {0}", t1step1v2LookupID));
            Assert.That(t1Step2LookupID, Is.EqualTo(model.TMData.stepTable[t1Step2LookupID].id), "other step t1step2 should be unchanged");

            EnsureUnaffectedSteps();

            PostTestCleanup();
        }

        [Test]
        public void UpdateStepWhenContent()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();

            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial1 = model.TMData.tutorials[0];


            string step1TextId = ConstructID(t1Step1LookupID, ContentType.text.ToString());
            Assert.IsTrue(model.TMData.contentTable.ContainsKey(step1TextId), "should have a step1TextId key");
            Assert.That(step1TextId, Is.EqualTo(model.TMData.contentTable[step1TextId].id), "content object should have correct id");

            model.UpdateStepEntity(t1Step1LookupID, t1step1v2LookupID);
            string step1TextIdv2 = ConstructID(t1step1v2LookupID, ContentType.text.ToString());

            Assert.AreEqual(2, model.TMData.content.Count, "there should still be 2 content items");
            Assert.IsFalse(model.TMData.contentTable.ContainsKey(step1TextId), "step1TextId key should be gone");
            Assert.IsTrue(model.TMData.contentTable.ContainsKey(step1TextIdv2), "should have a new step1TextIdv2 key");
            Assert.That(step1TextIdv2, Is.EqualTo(model.TMData.contentTable[step1TextIdv2].id), "content object should have correct id");

            EnsureUnaffectedSteps();

            PostTestCleanup();
        }

        [Test]
        public void DestroyStep()
        {
            SetupTutorial();
            SetupSteps();

            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial1 = model.TMData.tutorials[0];
            var tutorial2 = model.TMData.tutorials[1];
            var tutorial3 = model.TMData.tutorials[2];

            int stepTableCount = model.TMData.stepTable.Count;
            int stepListCount = model.TMData.steps.Count;

            model.DestroyStepEntity(t1Step1LookupID);

            Assert.AreEqual(1, tutorial1.steps.Count, "tutorial 1 should have just 1 step");
            Assert.That(t1Step2LookupID, Is.EqualTo(tutorial1.steps[0]), "the first step item in tutorial 1 should be t1step2");
            Assert.AreEqual(1, stepTableCount - model.TMData.stepTable.Count, "stepTable count should have decremented by one");
            Assert.AreEqual(1, stepListCount - model.TMData.steps.Count, "steps count should have decremented by one");
            Assert.IsFalse(model.TMData.stepTable.ContainsKey(t1Step1LookupID), "the step should have been removed from the table");

            Assert.IsFalse(FoundKeyInList(t1Step1LookupID, tutorial1.steps), "content should have been removed from step list");

            model.DestroyStepEntity(t1Step2LookupID);

            Assert.IsEmpty(tutorial1.steps, "tutorial 1 should be empty");
            Assert.AreEqual(2, stepTableCount - model.TMData.stepTable.Count, "stepTable count should have decremented by two");
            Assert.AreEqual(2, stepListCount - model.TMData.steps.Count, "steps count should have decremented by two");
            Assert.IsFalse(model.TMData.stepTable.ContainsKey(t1Step2LookupID), "the step should have been removed from the table");

            EnsureUnaffectedSteps();

            PostTestCleanup();
        }

        [Test]
        public void CreateText()
        {
            SetupTutorial();
            SetupSteps();

            var model = TutorialManagerModelMiddleware.GetInstance();
            string expectedStep1TextId = ConstructID(t1Step1LookupID, ContentType.text.ToString());
            string expectedStep2TextId = ConstructID(t1Step2LookupID, ContentType.text.ToString());

            var t1s1 = model.TMData.stepTable[t1Step1LookupID];
            var t1s2 = model.TMData.stepTable[t1Step2LookupID];


            // Add content to s1
            string contentId = model.CreateContentEntity(t1Step1LookupID, ContentType.text, t1step1Text);
            Assert.That(expectedStep1TextId, Is.EqualTo(contentId), string.Format("text content 1 should have id {0}", expectedStep1TextId));


            Assert.AreEqual(1, t1s1.messaging.content.Count, "step 1 should have one content item");
            Assert.IsEmpty(t1s2.messaging.content, "step 2 should still have no steps");
            Assert.AreEqual(1, model.TMData.contentTable.Count, "model should have 1 content item");

            ContentEntity t1S1textFromList = model.TMData.content[0];
            ContentEntity t1S1textFromTable = model.TMData.contentTable[contentId];
            Assert.IsNotNull(t1S1textFromList, "new content item should be in list");
            Assert.IsNotNull(t1S1textFromTable, "new content item should be in table");
            Assert.AreSame(t1S1textFromList, t1S1textFromTable, "the two entities should represent the same content item");

            // Check the content of the object
            Assert.That("text", Is.EqualTo(t1S1textFromTable.type), "text content type should be 'text'");
            Assert.That(t1step1Text, Is.EqualTo(t1S1textFromTable.text), string.Format("text content value should be {0}", t1step1Text));
            Assert.That(expectedStep1TextId, Is.EqualTo(t1S1textFromTable.id), string.Format("text id should be {0}", expectedStep1TextId));

            Assert.That(contentId, Is.EqualTo(t1S1textFromTable.id), string.Format("content id should be {0}", contentId));
            Assert.That(contentId, Is.EqualTo(t1s1.messaging.content[0]), string.Format("content id in step 1 should be {0}", contentId));

            // Add content to s2
            string contentId2 = model.CreateContentEntity(t1Step2LookupID, ContentType.text, t1step2Text);
            Assert.That(expectedStep2TextId, Is.EqualTo(contentId2), string.Format("text content 2 should have id {0}", expectedStep1TextId));

            Assert.AreEqual(1, t1s2.messaging.content.Count, "step 2 should have one content item");
            Assert.AreEqual(1, t1s1.messaging.content.Count, "step 1 should still have 1 step");
            Assert.AreEqual(2, model.TMData.contentTable.Count, "model should have 2 content items");

            ContentEntity t1S2textFromList = model.TMData.content[1];
            ContentEntity t1S2textFromTable = model.TMData.contentTable[contentId2];
            Assert.IsNotNull(t1S2textFromList, "new content item should be in list");
            Assert.IsNotNull(t1S2textFromTable, "new content item should be in table");
            Assert.AreSame(t1S2textFromList, t1S2textFromTable, "the two entities should represent the same content item");

            Assert.That(contentId2, Is.EqualTo(t1S2textFromTable.id), string.Format("content id should be {0}", contentId2));
            Assert.That(contentId2, Is.EqualTo(t1s2.messaging.content[0]), string.Format("content id in step 2 should be {0}", contentId2));

            EnsureUnaffectedSteps();

            PostTestCleanup();
        }

        [Test]
        public void UpdateText()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();

            string expectedStep1TextId = ConstructID(t1Step1LookupID, ContentType.text.ToString());

            var model = TutorialManagerModelMiddleware.GetInstance();
            var step1 = model.TMData.steps[0];
            var tutorial1 = model.TMData.tutorials[0];

            var content = model.TMData.contentTable[expectedStep1TextId];

            Assert.That(t1step1Text, Is.EqualTo(content.text), "content should have original text");
            model.UpdateContentEntity(expectedStep1TextId, t1step1Textv2);
            Assert.That(t1step1Textv2, Is.EqualTo(content.text), "content should have updated text");

            EnsureUnaffectedSteps();

            PostTestCleanup();
        }

        [Test]
        public void DestroyText()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();

            string textId1 = ConstructID(t1Step1LookupID, ContentType.text.ToString());
            string textId2 = ConstructID(t1Step2LookupID, ContentType.text.ToString());

            var model = TutorialManagerModelMiddleware.GetInstance();
            int contentListCount = model.TMData.content.Count;
            int contentTableCount = model.TMData.contentTable.Count;
            Assert.That(textId1, Is.EqualTo(model.TMData.content[0].id), "the original content 0 was in first position");

            model.DestroyContentEntity(textId1);

            Assert.AreEqual(1, contentListCount - model.TMData.content.Count, "content list should have lost an item");
            Assert.AreEqual(1, contentTableCount - model.TMData.contentTable.Count, "content table should have lost an item");
            Assert.That(textId2, Is.EqualTo(model.TMData.content[0].id), "content 2 is now in the first position");
            Assert.IsFalse(model.TMData.contentTable.ContainsKey(t1step1), "original content removed from the content table");

            model.DestroyContentEntity(textId2);

            Assert.AreEqual(2, contentListCount - model.TMData.content.Count, "content list should have lost an item");
            Assert.AreEqual(2, contentTableCount - model.TMData.contentTable.Count, "content table should have lost an item");
            Assert.IsEmpty(model.TMData.contentTable, "all content removed from the content table");
            Assert.IsEmpty(model.TMData.content, "all content removed from the content list");

            EnsureUnaffectedSteps();
            PostTestCleanup();
        }

        [Test]
        public void Clear()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            model.Clear();

            SetupTutorial();
            SetupSteps();
            SetupContent();

            Assert.IsNotEmpty(model.TMData.tutorials, "tutorials should be populated");
            Assert.IsNotEmpty(model.TMData.steps, "steps should be populated");
            Assert.IsNotEmpty(model.TMData.content, "content should be populated");
            Assert.IsNotEmpty(model.TMData.tutorialTable, "tutorial table should be populated");
            Assert.IsNotEmpty(model.TMData.stepTable, "step table should be populated");
            Assert.IsNotEmpty(model.TMData.contentTable, "content table should be populated");

            model.Clear();

            Assert.IsEmpty(model.TMData.tutorials, "tutorials should be empty");
            Assert.IsEmpty(model.TMData.steps, "steps should be empty");
            Assert.IsEmpty(model.TMData.content, "content should be empty");
            Assert.IsEmpty(model.TMData.tutorialTable, "tutorial table should be empty");
            Assert.IsEmpty(model.TMData.stepTable, "step table should be empty");
            Assert.IsEmpty(model.TMData.contentTable, "content table should be empty");

            PostTestCleanup();
        }

        [Test]
        public void ReadFromFile_WorksWhenFileExists()
        {
            PostTestCleanup();

            var tutorialName = "tutorial 1";
            var stepName = "step 1";
            var stepLookupName = ConstructID(tutorialName, stepName);
            var textLookupName = ConstructID(stepLookupName, "text");
            var actualModel = new TutorialManagerModel();
            var tutorial = new TutorialEntity(tutorialName);
            tutorial.steps.Add(stepName);
            var step = new StepEntity(stepLookupName);
            step.messaging.isActive = true;
            var text = new ContentEntity(textLookupName, "text", "yooo what's up! I work!");
            step.messaging.content.Add(text.id);
            actualModel.tutorials.Add(tutorial);
            actualModel.steps.Add(step);
            actualModel.content.Add(text);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Create(GetModelSavePath());

            binaryFormatter.Serialize(file, actualModel);
            file.Close();

            var model = TutorialManagerModelMiddleware.GetInstance();
            Assert.IsNotEmpty(model.TMData.tutorials);
            Assert.IsNotEmpty(model.TMData.steps);
            Assert.IsNotEmpty(model.TMData.content);
            Assert.AreEqual(tutorialName, model.TMData.tutorials[0].id);
            Assert.AreEqual(stepLookupName, model.TMData.steps[0].id);
            Assert.AreEqual(textLookupName, model.TMData.content[0].id);

            Assert.IsTrue(model.TMData.tutorialTable.ContainsKey(tutorialName));
            Assert.IsTrue(model.TMData.stepTable.ContainsKey(stepLookupName));
            Assert.IsTrue(model.TMData.contentTable.ContainsKey(textLookupName));

            PostTestCleanup();
        }

        [Test]
        public void ReadFromFile_DoesntBreakWhenNoFile()
        {
            PostTestCleanup();

            var model = TutorialManagerModelMiddleware.GetInstance();

            Assert.IsEmpty(model.TMData.tutorials);
            Assert.IsEmpty(model.TMData.steps);
            Assert.IsEmpty(model.TMData.content);

            PostTestCleanup();
        }

        void PostTestCleanup()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            var type = model.GetType();
            var prop = type.GetField("m_Instance", BindingFlags.NonPublic | BindingFlags.Static);
            prop.SetValue(model, null);
            File.Delete(GetModelSavePath());
        }

        string GetModelSavePath()
        {
            return Path.Combine(Application.persistentDataPath, "unity_tutorial_manager.dat");
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
            model.CreateStepEntity(t2step1, tutorial2.id);
            model.CreateStepEntity(t2step2, tutorial2.id);

            Assert.AreEqual(2, tutorial1.steps.Count, "tutorial 1 should have 2 steps");
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

            Assert.AreEqual(2, model.TMData.content.Count, "content should have 2 items");
            Assert.AreEqual(2, model.TMData.contentTable.Count, "contentTable should have 2 items");
            Assert.AreEqual(1, t1s1.messaging.content.Count, "t1s1 content should have 1 items");
            Assert.AreEqual(1, t1s2.messaging.content.Count, "t1s2 should have 1 items");
        }

        void EnsureUnaffectedSteps()
        {
            var model = TutorialManagerModelMiddleware.GetInstance();
            var tutorial2 = model.TMData.tutorials[1];
            var tutorial3 = model.TMData.tutorials[2];

            Assert.AreEqual(2, tutorial2.steps.Count, "tutorial 2 should still have 2 steps");
            Assert.AreEqual(0, tutorial3.steps.Count, "tutorial 3 should still have no steps");
        }

        string ConstructID(string tutorialId, string stepId)
        {
            return string.Format("{0}-{1}", tutorialId, stepId);
        }

        bool FoundKeyInList(string key, List<string> list)
        {
            for (int a = 0; a < list.Count; a++)
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