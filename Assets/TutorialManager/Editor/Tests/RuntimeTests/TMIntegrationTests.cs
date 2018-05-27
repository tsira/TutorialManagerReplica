/// <summary>
/// These automated runtime integration tests demonstrate that the frontend Tutorial Manager porcelain works.
/// </summary>

using System;
using System.Collections;
using UnityEngine.Analytics.TutorialManagerRuntime;
using UnityEngine.Analytics;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

namespace UnityEngine.Analytics
{
    /// <summary>
    /// This is an automated runtime integration test suite, intended to demonstrate that the entire TutorialManager 
    /// system is functional.
    /// </summary>
    public class TMIntegrationTests : TMIntegrationBase
    {

        void Setup()
        {
            SetupTutorial();
            SetupSteps();
            SetupContent();
            SetupAdaptiveContent();
        }

        [UnityTest]
        public IEnumerator TM_AutoAdvanceStepThroughState()
        {
            Setup();

            TutorialManager.TutorialStart(tutorialName1);
            yield return null;

            Assert.IsTrue(TutorialManager.state.autoAdvance, "autoAdvance should default to true");

            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step1LookupID), string.Format("currentStep should be {0}", t1Step1LookupID));
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(0, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 0));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");


            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step1LookupID), string.Format("step one, fsm should be in state {0}", t1Step1LookupID));

            TutorialManager.TutorialStep();
            yield return null;

            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step2LookupID), string.Format("currentStep should be {0}", t1Step2LookupID));
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 1));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");

            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step2LookupID), string.Format("step two, fsm should be in state {0}", t1Step2LookupID));

            TutorialManager.TutorialStep();
            yield return null;

            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step3LookupID), string.Format("currentStep should be {0}", t1Step3LookupID));
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(2, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 2));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");

            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step3LookupID), string.Format("step three, fsm should be in state {0}", t1Step3LookupID));

            TutorialManager.TutorialStep();
            yield return null;

            // Tutorial resolved. Should be zeroed out
            Assert.IsNull(TutorialManager.state.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.state.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsTrue(TutorialManager.state.complete, "complete should be true");

            Assert.IsNull(TutorialManager.state.fsm.state, "fsm should be in a null state");
        }

        [UnityTest]
        public IEnumerator TM_ManualStepThroughState()
        {
            Setup();

            TutorialManager.TutorialStart(tutorialName1, false);
            yield return null;

            Assert.IsFalse(TutorialManager.state.autoAdvance, "autoAdvance should default to false");

            // Step 1
            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step1LookupID), string.Format("currentStep should be {0}", t1Step1LookupID));
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(0, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 0));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step1LookupID), string.Format("step one, fsm should be in state {0}", t1Step1LookupID));

            TutorialManager.TutorialStep();
            yield return null;

            // Between steps 1 and 2
            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.IsNull(TutorialManager.state.currentStep, "currentStep should be null");
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(-1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.IsNull(TutorialManager.state.fsm.state, "fsm should be in null state");

            TutorialManager.TutorialStep();
            yield return null;

            // Step 2
            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step2LookupID), string.Format("currentStep should be {0}", t1Step2LookupID));
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 1));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step2LookupID), string.Format("step two, fsm should be in state {0}", t1Step2LookupID));

            TutorialManager.TutorialStep();
            yield return null;

            // Between steps 2 and 3
            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.IsNull(TutorialManager.state.currentStep, "currentStep should be null");
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(-1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.IsNull(TutorialManager.state.fsm.state, "fsm should be in null state");

            TutorialManager.TutorialStep();
            yield return null;

            // Step 3
            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step3LookupID), string.Format("currentStep should be {0}", t1Step3LookupID));
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(2, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 2));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step3LookupID), string.Format("step three, fsm should be in state {0}", t1Step3LookupID));

            TutorialManager.TutorialStep();
            yield return null;

            // Tutorial resolved. Should be zeroed out
            Assert.IsNull(TutorialManager.state.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.state.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsTrue(TutorialManager.state.complete, "complete should be true");
            Assert.IsNull(TutorialManager.state.fsm.state, "fsm should be in a null state");
        }

        [UnityTest]
        public IEnumerator TM_StepThroughStateAdaptiveContent()
        {
            Setup();

            var listOfContent = GetListOfContent();
            yield return null;

            TutorialManager.TutorialStart(tutorialName1);
            yield return null;

            TestIsActive(listOfContent, TutorialManager.state.fsm.state);

            var step1GoText = GameObject.Find(ConstructTextName(t1Step1LookupID));
            Assert.That(step1GoText.GetComponent<Text>().text, Is.EqualTo(t1step1Text), string.Format("step one text should be {0}", t1step1Text));

            TutorialManager.TutorialStep();
            yield return null;

            TestIsActive(listOfContent, TutorialManager.state.fsm.state);

            var step2GoText = GameObject.Find(ConstructTextName(t1Step2LookupID));
            Assert.That(step2GoText.GetComponent<Text>().text, Is.EqualTo(t1step2Text), string.Format("step two text should be {0}", t1step2Text));

            TutorialManager.TutorialStep();
            yield return null;

            TestIsActive(listOfContent, TutorialManager.state.fsm.state);

            var step3GoText = GameObject.Find(ConstructTextName(t1Step3LookupID));
            Assert.That(step3GoText.GetComponent<Text>().text, Is.EqualTo(t1step3Text), string.Format("step three text should be {0}", t1step3Text));

            TutorialManager.TutorialStep();
            yield return null;

            TestIsActive(listOfContent, "someUnknownKey");
        }

        [UnityTest]
        public IEnumerator TM_UpdateAStepAndAdvance()
        {
            Setup();
            yield return null;

            var model = TutorialManagerModelMiddleware.GetInstance();
            var step1TextKey = ConstructTextName(t1Step1LookupID);
            model.UpdateContentEntity(step1TextKey, t1step1Textv2);

            TutorialManager.TutorialStart(tutorialName1);
            yield return null;

            var step1GoText = GameObject.Find(ConstructTextName(t1Step1LookupID));
            Assert.That(step1GoText.GetComponent<Text>().text, Is.EqualTo(t1step1Textv2), string.Format("step one text should be {0}", t1step1Textv2));

            TutorialManager.TutorialStep();
            yield return null;

            var step2GoText = GameObject.Find(ConstructTextName(t1Step2LookupID));
            Assert.That(step2GoText.GetComponent<Text>().text, Is.EqualTo(t1step2Text), string.Format("step two text should be {0}", t1step2Text));

            TutorialManager.TutorialStep();
            yield return null;

            var step3GoText = GameObject.Find(ConstructTextName(t1Step3LookupID));
            Assert.That(step3GoText.GetComponent<Text>().text, Is.EqualTo(t1step3Text), string.Format("step three text should be {0}", t1step3Text));
        }

        [UnityTest]
        public IEnumerator TM_TutorialSkip()
        {
            Setup();
            yield return null;

            TutorialManager.TutorialStart(tutorialName1);
            yield return null;

            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step1LookupID), string.Format("currentStep should be {0}", t1Step1LookupID));
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(0, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 0));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");

            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step1LookupID), string.Format("fsm state should be {0}", t1Step1LookupID));

            TutorialManager.TutorialSkip();
            yield return null;

            Assert.IsNull(TutorialManager.state.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.state.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");

            Assert.IsNull(TutorialManager.state.fsm.state, "fsm should be in a null state");
        }

        [UnityTest]
        public IEnumerator TM_ArbitraryStateAccess()
        {
            Setup();

            TutorialManager.TutorialStart(tutorialName1);
            yield return null;

            // Step 1
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step1LookupID), string.Format("currentStep should be {0}", t1Step1LookupID));
            Assert.AreEqual(0, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 0));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step1LookupID), string.Format("step one, fsm should be in state {0}", t1Step1LookupID));

            TutorialManager.GotoTutorialStep(t1Step3LookupID);
            yield return null;

            // Step 3
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step3LookupID), string.Format("currentStep should be {0}", t1Step3LookupID));
            Assert.AreEqual(2, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 2));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step3LookupID), string.Format("step three, fsm should be in state {0}", t1Step3LookupID));


            TutorialManager.GotoTutorialStep(t1Step2LookupID);
            yield return null;

            // Step 2
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step2LookupID), string.Format("currentStep should be {0}", t1Step2LookupID));
            Assert.AreEqual(1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 1));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step2LookupID), string.Format("step two, fsm should be in state {0}", t1Step2LookupID));

            TutorialManager.TutorialStep();
            yield return null;

            // Back to 3
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step3LookupID), string.Format("step three, fsm should be in state {0}", t1Step3LookupID));

            TutorialManager.TutorialStep();
            yield return null;

            // Tutorial resolved. Should be zeroed out
            Assert.IsNull(TutorialManager.state.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.state.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsTrue(TutorialManager.state.complete, "complete should be true");

            Assert.IsNull(TutorialManager.state.fsm.state, "fsm should be in a null state");
        }

        [UnityTest]
        public IEnumerator TM_ResetState()
        {
            Setup();
            TutorialManager.TutorialStart(tutorialName1, true, true);
            yield return null;

            TutorialManager.TutorialStep();
            yield return null;

            Assert.IsTrue(TutorialManager.state.showTutorial, "showTutorial was forced to true");
            Assert.That(TutorialManager.state.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.state.currentStep, Is.EqualTo(t1Step2LookupID), string.Format("currentStep should be {0}", t1Step2LookupID));
            Assert.AreEqual(3, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", 1));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.That(TutorialManager.state.fsm.state, Is.EqualTo(t1Step2LookupID), string.Format("step two, fsm should be in state {0}", t1Step2LookupID));

            TutorialManager.ResetState();
            yield return null;

            Assert.IsFalse(TutorialManager.state.showTutorial, "showTutorial should be reset to false");
            Assert.IsNull(TutorialManager.state.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.state.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.state.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.state.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsFalse(TutorialManager.state.complete, "complete should be false");
            Assert.IsNull(TutorialManager.state.fsm.state, "fsm state should be null");
        }
    }
}
