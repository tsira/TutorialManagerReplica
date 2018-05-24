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

    const string expectedT1S1C0 = @"{""genre"":""Adventure"",""remoteSettings"":[{""key"":""tutorials"",""value"":""[\""tutorial\""]"",""type"":""string""},{""key"":""tutorial"",""value"":""[\""tutorial-step\""]"",""type"":""string""}]}";
    const string expectedT1S1C1 = @"{""genre"":""Adventure"",""remoteSettings"":[{""key"":""tutorials"",""value"":""[\""tutorial\""]"",""type"":""string""},{""key"":""tutorial"",""value"":""[\""tutorial-step\""]"",""type"":""string""},{""key"":""tutorial-step-text"",""value"":""Here is text for tutorial one step one"",""type"":""string""}]}";
    const string expectedT1S2C0 = @"{""genre"":""Adventure"",""remoteSettings"":[{""key"":""tutorials"",""value"":""[\""tutorial\""]"",""type"":""string""},{""key"":""tutorial"",""value"":""[\""tutorial-step\"",\""tutorial-step2\""]"",""type"":""string""}]}";

    const string expectedT2S1C0 = @"{""genre"":""Adventure"",""remoteSettings"":[{""key"":""tutorials"",""value"":""[\""tutorial\"",\""tutorial2\""]"",""type"":""string""},{""key"":""tutorial"",""value"":""[\""tutorial-step\""]"",""type"":""string""},{""key"":""tutorial2"",""value"":""[\""tutorial2-step\""]"",""type"":""string""}]}";
    const string expectedT2S1C1 = @"{""genre"":""Adventure"",""remoteSettings"":[{""key"":""tutorials"",""value"":""[\""tutorial\"",\""tutorial2\""]"",""type"":""string""},{""key"":""tutorial"",""value"":""[\""tutorial-step\""]"",""type"":""string""},{""key"":""tutorial2"",""value"":""[\""tutorial2-step\""]"",""type"":""string""},{""key"":""tutorial-step-text"",""value"":""Here is text for tutorial one step one"",""type"":""string""},{""key"":""tutorial2-step-text"",""value"":""Here is text for tutorial two step one"",""type"":""string""}]}";
    const string expectedT2S2C1 = @"{""genre"":""Adventure"",""remoteSettings"":[{""key"":""tutorials"",""value"":""[\""tutorial\"",\""tutorial2\""]"",""type"":""string""},{""key"":""tutorial"",""value"":""[\""tutorial-step\"",\""tutorial-step2\""]"",""type"":""string""},{""key"":""tutorial2"",""value"":""[\""tutorial2-step\"",\""tutorial2-step2\""]"",""type"":""string""},{""key"":""tutorial-step-text"",""value"":""Here is text for tutorial one step one"",""type"":""string""},{""key"":""tutorial-step2-text"",""value"":""Here is text for tutorial one step two"",""type"":""string""},{""key"":""tutorial2-step-text"",""value"":""Here is text for tutorial two step one"",""type"":""string""},{""key"":""tutorial2-step2-text"",""value"":""Here is text for tutorial two step two"",""type"":""string""}]}";



    [Test]
    public void TestOneTutorialOneStepNoContent()
    {
        var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        modelMiddleware.Clear();

        modelMiddleware.CreateTutorialEntity(tutorialName1);
        modelMiddleware.CreateStepEntity(t1step1, tutorialName1);
        modelMiddleware.SaveGenre("Adventure");

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(output, Is.EqualTo(expectedT1S1C0));
    }

    [Test]
    public void TestOneTutorialOneStepOneContent()
    {
        var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        modelMiddleware.Clear();

        modelMiddleware.CreateTutorialEntity(tutorialName1);
        modelMiddleware.CreateStepEntity(t1step1, tutorialName1);
        modelMiddleware.CreateContentEntity(t1Step1LookupID, ContentType.text, t1step1Text);
        modelMiddleware.SaveGenre("Adventure");

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(output, Is.EqualTo(expectedT1S1C1));
    }

    [Test]
    public void TestOneTutorialTwoStepsNoContent()
    {
        var modelMiddleware = TutorialManagerModelMiddleware.GetInstance();
        modelMiddleware.Clear();

        modelMiddleware.CreateTutorialEntity(tutorialName1);
        modelMiddleware.CreateStepEntity(t1step1, tutorialName1);
        modelMiddleware.CreateStepEntity(t1step2, tutorialName1);
        modelMiddleware.SaveGenre("Adventure");

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(output, Is.EqualTo(expectedT1S2C0));
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
        modelMiddleware.SaveGenre("Adventure");

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(output, Is.EqualTo(expectedT2S1C0));
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
        modelMiddleware.SaveGenre("Adventure");

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(output, Is.EqualTo(expectedT2S1C1));
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
        modelMiddleware.SaveGenre("Adventure");

        string output = TMModelToJsonInterpreter.ProcessModelToJson(modelMiddleware.TMData);
        Assert.That(output, Is.EqualTo(expectedT2S2C1));
    }
}
