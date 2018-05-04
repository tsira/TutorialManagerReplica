#if UNITY_5_6_OR_NEWER
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;
namespace UnityEngine.Analytics
{

    public class TMModelTests
    {
        [Test]
        public void CreateTutorial()
        {
            const string tutorialName1 = "testvalue";
            const string tutorialName2 = "testvalue-2";

            var model = TutorialManagerModel.GetInstance();

            model.CreateEntity<TutorialEntity>(tutorialName1);

            Assert.AreEqual(1, model.tutorials.Count, "tutorial length should be 1");
            TutorialEntity entityFromList = model.tutorials[0];
            Assert.IsNotNull(entityFromList, "entity should exist");
            Assert.AreEqual(tutorialName1, entityFromList.id, string.Format("entity id shoyuld be {0}", tutorialName1));
            TutorialEntity entityFromLookup = model.tutorialTable[tutorialName1];
            Assert.AreSame(entityFromList, entityFromLookup, "tutorial entities should be the same object");
            Assert.AreEqual(1, model.tutorialTable.Count, "tutorialTable has 1 item");

            model.CreateEntity<TutorialEntity>(tutorialName2);
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
            const string tutorialName1 = "testvalue";
            const string tutorialName2 = "testvalue-2";

            const string tutorialName1v2 = "anothertestvalue";
            const string tutorialName2v2 = "anothertestvalue-2";

            var model = TutorialManagerModel.GetInstance();

            model.CreateEntity<TutorialEntity>(tutorialName1);
            model.CreateEntity<TutorialEntity>(tutorialName2);
            Assert.AreEqual(2, model.tutorials.Count, "tutorial length should be 2");

            model.UpdateEntity<TutorialEntity>(tutorialName1, tutorialName1v2);

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
            const string tutorialName1 = "testvalue";
            const string tutorialName2 = "testvalue-2";

            var model = TutorialManagerModel.GetInstance();

            model.CreateEntity<TutorialEntity>(tutorialName1);
            model.CreateEntity<TutorialEntity>(tutorialName2);
            Assert.AreEqual(2, model.tutorials.Count, "tutorial length should be 2");

            model.DestroyEntity<TutorialEntity>(tutorialName1);
            Assert.AreEqual(1, model.tutorials.Count, "tutorial length should be 1");

            TutorialEntity entityFromLookup = model.tutorialTable[tutorialName2];
            Assert.IsNotNull(entityFromLookup, "the second item still exists");
            Assert.AreEqual(tutorialName2, entityFromLookup.id);
            TutorialEntity entityFromList = model.tutorials[0];
            Assert.IsNotNull(entityFromList, "entity should exist");
            Assert.AreSame(entityFromList, entityFromLookup, "tutorial entities should be the same object");

            model.DestroyEntity<TutorialEntity>(tutorialName2);
            Assert.AreEqual(0, model.tutorials.Count, "tutorials should be empty");
            Assert.AreEqual(0, model.tutorialTable.Count, "tutorialTable should be empty");
        }

        [Test]
        public void CreateStep()
        {
            
        }

        [Test]
        public void UpdateStep()
        {
            
        }

        [Test]
        public void DestroyStep()
        {
            
        }

        [Test]
        public void CreateText()
        {
            
        }

        [Test]
        public void UpdateText()
        {
            
        }

        [Test]
        public void DestroyText()
        {
            
        }
    }


#endif
}