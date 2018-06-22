/// <summary>
/// This is an automated runtime integration test, intended to demonstrate that – absent the porcelain
/// of the TutorialManager class – the underlying state machine, dispatcher, and components work.
/// </summary>

#if UNITY_5_6_OR_NEWER
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
    public class AdaptiveContentTests
    {

        IDataStore store;
        IFSMDispatcher dispatcher;
        StateSystemProvider provider;
        List<GameObject> gameObjectList;

        const string bindingId1 = "binding1";
        const string bindingId2 = "binding2";
        const string bindingId3 = "binding3";
        const string bindingId4 = "binding4";

        const string text1 = "Here is text for tutorial one step one";
        const string text2 = "Here is text for tutorial one step two";
        const string text3 = "Here is text for tutorial one step three";

        const string text1v2 = "Text modified for tutorial one step one";
        const string text2v2 = "Text modified for tutorial one step two";

        [UnityTest]
        public IEnumerator AdaptiveContent_ListensToStateDispatches()
        {
            SetupWorld();
            SetupContent();
            yield return null;

            dispatcher.DispatchEnterState(bindingId1);
            yield return null;

            TestGameObjectList(bindingId1);


            dispatcher.DispatchEnterState(bindingId3);
            yield return null;

            TestGameObjectList(bindingId3);

            dispatcher.DispatchEnterState("some unknown id");
            yield return null;

            TestGameObjectList("some unknown id");
        }

        [UnityTest]
        public IEnumerator AdaptiveContent_ContentRespectsAndIgnoresOff()
        {
            SetupWorld();
            (store as TestDataStore).remoteDecision = false;
            SetupContentWithRespect();
            yield return null;

            dispatcher.DispatchEnterState(bindingId1);
            yield return null;
            TestGameObjectListWithRespect(bindingId1);

            dispatcher.DispatchEnterState(bindingId2);
            yield return null;
            TestGameObjectListWithRespect(bindingId2);

            dispatcher.DispatchEnterState(bindingId3);
            yield return null;
            TestGameObjectListWithRespect(bindingId3);

            dispatcher.DispatchEnterState(bindingId4);
            yield return null;
            TestGameObjectListWithRespect(bindingId4);
        }

        void TestGameObjectListWithRespect(string id)
        {
            for (int a = 0; a < gameObjectList.Count; a++) {
                if (gameObjectList[a].name == id) {
                    if (gameObjectList[a].GetComponent<AdaptiveContent>().respectRemoteIsActive) {
                        // Tutorial is off, this component should be off
                        Assert.IsFalse(gameObjectList[a].activeSelf, string.Format("{0} should be inactive", id));
                    } else {
                        // This component doesn't respect the decision, and will always show when in state
                        Assert.IsTrue(gameObjectList[a].activeSelf, string.Format("{0} should be active", id));
                    }
                } else {
                    Assert.IsFalse(gameObjectList[a].activeSelf, string.Format("{0} should be inactive", id));
                }
            }
        }


        [UnityTest]
        public IEnumerator AdaptiveText_ReceivesExistingText()
        {
            SetupWorld();
            SetupText();
            yield return null;

            Assert.That(gameObjectList[0].GetComponent<Text>().text, Is.EqualTo(text1), string.Format("text should be equal to {0}", text1));
            Assert.That(gameObjectList[1].GetComponent<Text>().text, Is.EqualTo(text2), string.Format("text should be equal to {0}", text2));
            Assert.That(gameObjectList[2].GetComponent<Text>().text, Is.EqualTo(text3), string.Format("text should be equal to {0}", text3));
        }

        [UnityTest]
        public IEnumerator AdaptiveText_ReceivesRuntimeTextChanges()
        {
            SetupWorld();
            SetupText();
            yield return null;

            Assert.That(gameObjectList[0].GetComponent<Text>().text, Is.EqualTo(text1), string.Format("text should be equal to {0}", text1));
            Assert.That(gameObjectList[1].GetComponent<Text>().text, Is.EqualTo(text2), string.Format("text should be equal to {0}", text2));
            Assert.That(gameObjectList[2].GetComponent<Text>().text, Is.EqualTo(text3), string.Format("text should be equal to {0}", text3));

            (store as TestDataStore).UpdateData(bindingId1, text1v2);
            (store as TestDataStore).UpdateData(bindingId2, text2v2);

            Assert.That(gameObjectList[0].GetComponent<Text>().text, Is.EqualTo(text1v2), string.Format("text should be updated to {0}", text1v2));
            Assert.That(gameObjectList[1].GetComponent<Text>().text, Is.EqualTo(text2v2), string.Format("text should be updated to {0}", text2v2));
            Assert.That(gameObjectList[2].GetComponent<Text>().text, Is.EqualTo(text3), string.Format("text should be unchanged at {0}", text3));
        }

        // Utility /////////////////////////////////// 
        void TestGameObjectList(string id)
        {
            for (int a = 0; a < gameObjectList.Count; a++) {
                if (gameObjectList[a].name == id) {
                    Assert.IsTrue(gameObjectList[a].activeSelf, string.Format("{0} should be active", id));
                } else {
                    Assert.IsFalse(gameObjectList[a].activeSelf, string.Format("{0} should be inactive", id));
                }
            }
        }

        // Setup ///////////////////////////////////////

        void SetupWorld()
        {
            store = new TestDataStore();
            dispatcher = new TestDispatcher();
            provider = StateSystemProvider.GetInstance();
            provider.SetDataStore(store);
            provider.SetDispatcher(dispatcher);

        }

        void SetupContent()
        {
            gameObjectList = new List<GameObject>();
            List<string> bindings = new List<string> { bindingId1, bindingId2, bindingId3, bindingId4 };
            for (int a = 0; a < bindings.Count; a++) {
                GameObject gameObject = new GameObject();
                gameObject.name = bindings[a];
                gameObjectList.Add(gameObject);

                AdaptiveContent adaptiveContent = gameObject.AddComponent<AdaptiveContent>();
                adaptiveContent.bindingIds.Add(bindings[a]);
            }
        }

        void SetupContentWithRespect()
        {
            gameObjectList = new List<GameObject>();
            List<string> bindings = new List<string> { bindingId1, bindingId2, bindingId3, bindingId4 };
            for (int a = 0; a < bindings.Count; a++) {
                GameObject gameObject = new GameObject();
                gameObject.name = bindings[a];
                gameObjectList.Add(gameObject);

                AdaptiveContent adaptiveContent = gameObject.AddComponent<AdaptiveContent>();
                adaptiveContent.bindingIds.Add(bindings[a]);
                adaptiveContent.respectRemoteIsActive = (a % 2 == 0);
            }
        }

        void SetupText()
        {
            (store as TestDataStore).m_Data = new Dictionary<string, string> {
                { bindingId1, text1 },
                { bindingId2, text2 },
                { bindingId3, text3 }
            };


            gameObjectList = new List<GameObject>();
            List<string> bindings = new List<string> { bindingId1, bindingId2, bindingId3 };
            for (int a = 0; a < bindings.Count; a++) {
                GameObject gameObject = new GameObject();
                gameObject.name = bindings[a];
                gameObjectList.Add(gameObject);

                gameObject.AddComponent<Text>();
                AdaptiveText adaptiveText = gameObject.AddComponent<AdaptiveText>();
                adaptiveText.bindingIds.Add(bindings[a]);
            }
        }
    }

    class TestDispatcher : IFSMDispatcher
    {
        public string m_State;
        public string state {
            get {
                return m_State;
            }
        }

        public event OnEnterStateHandler OnEnterState;
        public event OnExitStateHandler OnExitState;

        public void DispatchEnterState(string id)
        {
            m_State = id;
            OnEnterState(id);
        }

        public void DispatchExitState(string id)
        {
            m_State = null;
            OnExitState(id);
        }
    }

    class TestDataStore : IDataStore
    {
        public Dictionary<string,string> m_Data;

        public event OnDataUpdateHandler OnDataUpdate;

        public bool remoteDecision = true;

        public string GetString(string id, string defaultValue = null)
        {
            return m_Data[id];
        }

        public bool GetBool(string id, bool defaultValue = false)
        {
            return remoteDecision;
        }

        public void UpdateData(string key, string value)
        {
            m_Data[key] = value;
            OnDataUpdate();
        }
    }
}
#endif
