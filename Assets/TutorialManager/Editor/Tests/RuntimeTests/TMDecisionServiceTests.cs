#if UNITY_5_6_OR_NEWER
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    public class TMDecisionServiceIntegrationTests
    {
#pragma warning disable 414
        static TutorialWebResponse webResponse;
        static bool testComplete = false;
#pragma warning restore 414
        /*
        [UnityTest]
        [PrebuildSetup(typeof(SetupNewUser))]
        public IEnumerator NewPlayer_ServerHit()
        {
            yield return new WaitUntil(() => testComplete || Time.timeSinceLevelLoad > 10.0f);

            Assert.IsTrue(TutorialManager.state.decisionReceived, "should have received a response");
            Assert.AreEqual(TutorialManager.state.showTutorial, webResponse.showTutorial, 
                            "test should complete and TM showTutorial should be the same as the given response");

            Debug.LogWarning("TMDecisionServiceTests.NewPlayer_ServerHit can only run while " +
                             "TMDecisionServiceTests.OldPlayer_ServerNoHit is disabled. Testing is not complete until " +
                             "both tests are run. Please disable this test and enable the other to complete testing.");
        }
        */

        /*
        [UnityTest]
        [PrebuildSetup(typeof(SetupOldUser))]
        public IEnumerator OldPlayer_ServerNoHit()
        {

            yield return new WaitUntil(() => Time.timeSinceLevelLoad > 10.0f);
            Assert.IsFalse(testComplete, "test should never complete, since no call should have been made");

            Debug.LogWarning("TMDecisionServiceTests.OldPlayer_ServerNoHit can only run while " +
                             "TMDecisionServiceTests.NewPlayer_ServerHit is disabled. Testing is not complete until " +
                             "both tests are run. Please disable this test and enable the other to complete testing.");
        }
        */

        public static string analyticsPath
        {
            get {
                string path = Path.Combine(Application.persistentDataPath, "Unity");
                path = Path.Combine(path, Application.cloudProjectId);
                path = Path.Combine(path, "Analytics");
                path = Path.Combine(path, "values");
                return path;
            }
        }


        public static string statePath {
            get {
                return Path.Combine(Application.persistentDataPath, "unity_tutorial_manager_state.dat");
            }
        }

        [RuntimeInitializeOnLoadMethod]
        public static void ListenForDecision()
        {
            DecisionRequestService.OnDecisionReceived -= TestResponseCaching;
            DecisionRequestService.OnDecisionReceived += TestResponseCaching;
            webResponse = null;
            testComplete = false;
        }

        static void TestResponseCaching(TutorialWebResponse response)
        {
            Debug.Log("TestResponseCaching");
            Debug.Log(response);
            webResponse = response;
            testComplete = true;
        }
    }

    public class SetupNewUser : IPrebuildSetup
    {
        public void Setup()
        {
            if (File.Exists(TMDecisionServiceIntegrationTests.analyticsPath)) {
                File.Delete(TMDecisionServiceIntegrationTests.analyticsPath);
            }

            if (File.Exists(TMDecisionServiceIntegrationTests.statePath)) {
                File.Delete(TMDecisionServiceIntegrationTests.statePath);
            }
        }
    }

    public class SetupOldUser : IPrebuildSetup
    {
        public void Setup()
        {
            if (!File.Exists(TMDecisionServiceIntegrationTests.analyticsPath)) {
                File.Create(TMDecisionServiceIntegrationTests.analyticsPath);
            }
            if (!File.Exists(TMDecisionServiceIntegrationTests.statePath)) {
                var fakeState = new TutorialManagerSaveableState();
                fakeState.decisionReceived = true;
                fakeState.showTutorial = true;
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream file = File.Create(TMDecisionServiceIntegrationTests.statePath);
                binaryFormatter.Serialize(file, fakeState);
                file.Close();
            }
        }
    }
}
#endif