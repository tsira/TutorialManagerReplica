using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using NUnit.Framework;
using UnityEngine.Analytics.TutorialManagerRuntime;

public class TMModelToJsonInterpreterTests
{

    const string tutorialName1 = "tutorial";
    const string tutorialName2 = "tutorial2";

    const string stepBaseID = "step";
    const string stepBaseID2 = "step2";

    string t1step1 = stepBaseID;
    string t1step2 = stepBaseID2;

    string t1Step1LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID);
    string t1Step2LookupID = string.Format("{0}-{1}", tutorialName1, stepBaseID2);
    string t2Step1LookupID = string.Format("{0}-{1}", tutorialName2, stepBaseID);
    string t2Step2LookupID = string.Format("{0}-{1}", tutorialName2, stepBaseID2);


    const string t2step1 = "step";
    const string t2step2 = "step2";

    const string t1step1Text = "Here is text for tutorial one step one";
    const string t1step2Text = "Here is text for tutorial one step two";
    const string t2step1Text = "Here is text for tutorial two step one";
    const string t2step2Text = "Here is text for tutorial two step two";

    const string expectedT1S1C0 = @"""remoteSettings"":{""tutorials"":""[tutorial]"",""tutorial"":""[tutorial-step]""}";
    const string expectedT1S1C1 = @"""remoteSettings"":{""tutorials"":""[tutorial]"",""tutorial"":""[tutorial-step]"",""tutorial-step-text"":""Here is text for tutorial one step one""}";
    const string expectedT1S2C0 = @"""remoteSettings"":{""tutorials"":""[tutorial]"",""tutorial"":""[tutorial-step,tutorial-step2]""}";

    const string expectedT2S1C0 = @"""remoteSettings"":{""tutorials"":""[tutorial,tutorial2]"",""tutorial"":""[tutorial-step]"",""tutorial2"":""[tutorial2-step]""}";
    const string expectedT2S1C1 = @"""remoteSettings"":{""tutorials"":""[tutorial,tutorial2]"",""tutorial"":""[tutorial-step]"",""tutorial2"":""[tutorial2-step]"",""tutorial-step-text"":""Here is text for tutorial one step one"",""tutorial2-step-text"":""Here is text for tutorial two step one""}";
    const string expectedT2S2C1 = @"""remoteSettings"":{""tutorials"":""[tutorial,tutorial2]"",""tutorial"":""[tutorial-step,tutorial-step2]"",""tutorial2"":""[tutorial2-step,tutorial2-step2]"",""tutorial-step-text"":""Here is text for tutorial one step one"",""tutorial-step2-text"":""Here is text for tutorial one step two"",""tutorial2-step-text"":""Here is text for tutorial two step one"",""tutorial2-step2-text"":""Here is text for tutorial two step two""}";



    [Test]
    public void TestOneTutorialOneStepNoContent()
    {
        var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        modelMiddleware.Clear();

        modelMiddleware.CreateTutorialEntity(tutorialName1);
        modelMiddleware.CreateStepEntity(t1step1, tutorialName1);

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(expectedT1S1C0, Is.EqualTo(output));
    }

    [Test]
    public void TestOneTutorialOneStepOneContent()
    {
        var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        modelMiddleware.Clear();

        modelMiddleware.CreateTutorialEntity(tutorialName1);
        modelMiddleware.CreateStepEntity(t1step1, tutorialName1);
        modelMiddleware.CreateContentEntity(t1Step1LookupID, ContentType.text, t1step1Text);

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(expectedT1S1C1, Is.EqualTo(output));
    }

    [Test]
    public void TestOneTutorialTwoStepsNoContent()
    {
        var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        modelMiddleware.Clear();

        modelMiddleware.CreateTutorialEntity(tutorialName1);
        modelMiddleware.CreateStepEntity(t1step1, tutorialName1);
        modelMiddleware.CreateStepEntity(t1step2, tutorialName1);

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(expectedT1S2C0, Is.EqualTo(output));
    }

    [Test]
    public void TestTwoTutorialsOneStepEachNoContent()
    {
        var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        modelMiddleware.Clear();

        modelMiddleware.CreateTutorialEntity(tutorialName1);
        modelMiddleware.CreateStepEntity(t1step1, tutorialName1);
        modelMiddleware.CreateTutorialEntity(tutorialName2);
        modelMiddleware.CreateStepEntity(t2step1, tutorialName2);

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(expectedT2S1C0, Is.EqualTo(output));
    }

    [Test]
    public void TestTwoTutorialsOneStepEachPlusContent()
    {
        var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        modelMiddleware.Clear();

        modelMiddleware.CreateTutorialEntity(tutorialName1);
        modelMiddleware.CreateStepEntity(t1step1, tutorialName1);
        modelMiddleware.CreateContentEntity(t1Step1LookupID, ContentType.text, t1step1Text);

        modelMiddleware.CreateTutorialEntity(tutorialName2);
        modelMiddleware.CreateStepEntity(t2step1, tutorialName2);
        modelMiddleware.CreateContentEntity(t2Step1LookupID, ContentType.text, t2step1Text);

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(expectedT2S1C1, Is.EqualTo(output));
    }

    [Test]
    public void TestTwoTutorialsTwoStepEachPlusContent()
    {
        var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        modelMiddleware.Clear();

        modelMiddleware.CreateTutorialEntity(tutorialName1);
        modelMiddleware.CreateStepEntity(t1step1, tutorialName1);
        modelMiddleware.CreateContentEntity(t1Step1LookupID, ContentType.text, t1step1Text);
        modelMiddleware.CreateStepEntity(t1step2, tutorialName1);
        modelMiddleware.CreateContentEntity(t1Step2LookupID, ContentType.text, t1step2Text);

        modelMiddleware.CreateTutorialEntity(tutorialName2);
        modelMiddleware.CreateStepEntity(t2step1, tutorialName2);
        modelMiddleware.CreateContentEntity(t2Step1LookupID, ContentType.text, t2step1Text);
        modelMiddleware.CreateStepEntity(t2step2, tutorialName2);
        modelMiddleware.CreateContentEntity(t2Step2LookupID, ContentType.text, t2step2Text);

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);

        Assert.That(expectedT2S2C1, Is.EqualTo(output));
    }
}
