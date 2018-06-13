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

            TutorialManager.Start(tutorialName1);
            yield return null;

            Assert.IsTrue(TutorialManager.autoAdvance, "autoAdvance should default to true");

            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step1), string.Format("currentStep should be {0}", t1Step1));
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(0, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 0));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepComplete();
            yield return null;

            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step2), string.Format("currentStep should be {0}", t1Step2));
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 1));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepComplete();
            yield return null;

            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step3), string.Format("currentStep should be {0}", t1Step3));
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(2, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 2));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepComplete();
            yield return null;

            // Tutorial resolved. Should be zeroed out
            Assert.IsNull(TutorialManager.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsTrue(TutorialManager.complete, "complete should be true");
        }

        [UnityTest]
        public IEnumerator TM_ManualStepThroughState()
        {
            Setup();

            TutorialManager.Start(tutorialName1, false);
            yield return null;

            Assert.IsFalse(TutorialManager.autoAdvance, "autoAdvance should default to false");

            // Step 1
            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step1), string.Format("currentStep should be {0}", t1Step1));
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(0, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 0));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepComplete();
            yield return null;

            // Between steps 1 and 2
            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.IsNull(TutorialManager.currentStep, "currentStep should be null");
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(-1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepStart();
            yield return null;

            // Step 2
            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step2), string.Format("currentStep should be {0}", t1Step2));
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 1));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepComplete();
            yield return null;

            // Between steps 2 and 3
            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.IsNull(TutorialManager.currentStep, "currentStep should be null");
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(-1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepStart();
            yield return null;

            // Step 3
            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step3), string.Format("currentStep should be {0}", t1Step3));
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(2, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 2));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepComplete();
            yield return null;

            // Tutorial resolved. Should be zeroed out
            Assert.IsNull(TutorialManager.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsTrue(TutorialManager.complete, "complete should be true");
        }

        [UnityTest]
        public IEnumerator TM_StepThroughStateAdaptiveContent()
        {
            Setup();

            var listOfContent = GetListOfContent();
            yield return null;

            TutorialManager.Start(tutorialName1);
            yield return null;

            TestIsActive(listOfContent, TutorialManager.currentStep);

            var step1GoText = GameObject.Find(ConstructTextName(t1Step1LookupID));
            Assert.That(step1GoText.GetComponent<Text>().text, Is.EqualTo(t1step1Text), string.Format("step one text should be {0}", t1step1Text));

            TutorialManager.StepComplete();
            yield return null;

            TestIsActive(listOfContent, TutorialManager.currentStep);

            var step2GoText = GameObject.Find(ConstructTextName(t1Step2LookupID));
            Assert.That(step2GoText.GetComponent<Text>().text, Is.EqualTo(t1step2Text), string.Format("step two text should be {0}", t1step2Text));

            TutorialManager.StepComplete();
            yield return null;

            TestIsActive(listOfContent, TutorialManager.currentStep);

            var step3GoText = GameObject.Find(ConstructTextName(t1Step3LookupID));
            Assert.That(step3GoText.GetComponent<Text>().text, Is.EqualTo(t1step3Text), string.Format("step three text should be {0}", t1step3Text));

            TutorialManager.StepComplete();
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

            TutorialManager.Start(tutorialName1);
            yield return null;

            var step1GoText = GameObject.Find(ConstructTextName(t1Step1LookupID));
            Assert.That(step1GoText.GetComponent<Text>().text, Is.EqualTo(t1step1Textv2), string.Format("step one text should be {0}", t1step1Textv2));

            TutorialManager.StepComplete();
            yield return null;

            var step2GoText = GameObject.Find(ConstructTextName(t1Step2LookupID));
            Assert.That(step2GoText.GetComponent<Text>().text, Is.EqualTo(t1step2Text), string.Format("step two text should be {0}", t1step2Text));

            TutorialManager.StepComplete();
            yield return null;

            var step3GoText = GameObject.Find(ConstructTextName(t1Step3LookupID));
            Assert.That(step3GoText.GetComponent<Text>().text, Is.EqualTo(t1step3Text), string.Format("step three text should be {0}", t1step3Text));
        }

        [UnityTest]
        public IEnumerator TM_TutorialSkip()
        {
            Setup();
            yield return null;

            TutorialManager.Start(tutorialName1);
            yield return null;

            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step1), string.Format("currentStep should be {0}", t1Step1));
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(0, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 0));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step1), string.Format("fsm state should be {0}", t1Step1));

            TutorialManager.Skip();
            yield return null;

            Assert.IsNull(TutorialManager.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");
        }

        [UnityTest]
        public IEnumerator TM_ArbitraryStateAccess()
        {
            Setup();

            TutorialManager.Start(tutorialName1);
            yield return null;

            // Step 1
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step1), string.Format("currentStep should be {0}", t1Step1));
            Assert.AreEqual(0, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 0));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step1), string.Format("step one, fsm should be in state {0}", t1Step1));

            TutorialManager.StepStart(t1Step3);
            yield return null;

            // Step 3
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step3), string.Format("currentStep should be {0}", t1Step3));
            Assert.AreEqual(2, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 2));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepStart(t1Step2);
            yield return null;

            // Step 2
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step2), string.Format("currentStep should be {0}", t1Step2));
            Assert.AreEqual(1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 1));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepComplete();
            yield return null;

            // Back to 3
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepComplete();
            yield return null;

            // Tutorial resolved. Should be zeroed out
            Assert.IsNull(TutorialManager.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsTrue(TutorialManager.complete, "complete should be true");
        }

        [UnityTest]
        public IEnumerator TM_ArbitraryCrossTutorialStateAccess()
        {
            Setup();

            TutorialManager.Start(tutorialName1);
            yield return null;

            // Tutorial 1 Step 1
            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step1), string.Format("currentStep should be {0}", t1Step1));
            Assert.AreEqual(0, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 0));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step1), string.Format("step one, fsm should be in state {0}", t1Step1));

            TutorialManager.StepStart(tutorialName2, t2Step2);
            yield return null;

            // Tutorial 2 Step 2
            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName2), string.Format("tutorialId should be {0}", tutorialName2));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t2Step2), string.Format("currentStep should be {0}", t2Step2));
            Assert.AreEqual(1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 1));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepStart(tutorialName1, t1Step3);
            yield return null;

            // Tutorial 1 Step 3
            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step3), string.Format("currentStep should be {0}", t1Step3));
            Assert.AreEqual(2, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 2));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.StepComplete();
            yield return null;

            // Tutorial resolved. Should be zeroed out
            Assert.IsNull(TutorialManager.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsTrue(TutorialManager.complete, "complete should be true");
        }

        [UnityTest]
        public IEnumerator TM_ResetState()
        {
            Setup();
            TutorialManager.GetDecision(false);
            TutorialManager.Start(tutorialName1, true);

            yield return null;

            TutorialManager.StepComplete();
            yield return null;

            Assert.IsFalse(TutorialManager.showTutorial, "showTutorial was forced to false");
            Assert.That(TutorialManager.tutorialId, Is.EqualTo(tutorialName1), string.Format("tutorialId should be {0}", tutorialName1));
            Assert.That(TutorialManager.currentStep, Is.EqualTo(t1Step2), string.Format("currentStep should be {0}", t1Step2));
            Assert.AreEqual(3, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 3));
            Assert.AreEqual(1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", 1));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");

            TutorialManager.Reset();
            yield return null;

            Assert.IsTrue(TutorialManager.showTutorial, "showTutorial should be reset to true");
            Assert.IsNull(TutorialManager.tutorialId, "tutorialId should be null");
            Assert.IsNull(TutorialManager.currentStep, "currentStep should be null");
            Assert.AreEqual(0, TutorialManager.tutorialLength, string.Format("tutorial length should be {0}", 0));
            Assert.AreEqual(-1, TutorialManager.stepIndex, string.Format("stepIndex should be {0}", -1));
            Assert.IsFalse(TutorialManager.complete, "complete should be false");
        }
    }
}
