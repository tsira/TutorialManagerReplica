using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics
{

    public class AdaptiveStateDispatcherTests
    {

        const string state1 = "state-1";
        const string state2 = "state-2";
        const string state3 = "state-3";

        [Test]
        public void TestSingleton()
        {
            var dispatcher = AdaptiveStateDispatcher.GetInstance();
            Assert.IsNotNull(dispatcher, "singleton dispatcher should exist");
            var dispatcher2 = AdaptiveStateDispatcher.GetInstance();
            Assert.AreSame(dispatcher, dispatcher2, "singleton dispatcher should be the same object");
        }

        [Test]
        public void TestDispatcherListener()
        {
            var dispatcher = AdaptiveStateDispatcher.GetInstance();
            var listener = new TestDispatcherListener(dispatcher);

            dispatcher.DispatchEnterState(state1);

            Assert.That(state1, Is.EqualTo(listener.lastEnterId), string.Format("last entered state should be {0}", state1));
            Assert.IsTrue(string.IsNullOrEmpty(listener.lastExitId), "should be no exit state yet");
            Assert.AreEqual(1, listener.count, "should have fired 1 event");

            dispatcher.DispatchExitState(state1);
            Assert.That(state1, Is.EqualTo(listener.lastExitId), string.Format("last exited state should be {0}", state1));
            Assert.AreEqual(2, listener.count, "should have fired 2 events");

            dispatcher.DispatchEnterState(state2);
            Assert.That(state2, Is.EqualTo(listener.lastEnterId), string.Format("last entered state should be {0}", state2));
            Assert.That(state1, Is.EqualTo(listener.lastExitId), string.Format("last exited state should be {0}", state1));
            Assert.AreEqual(3, listener.count, "should have fired 3 events");

            dispatcher.DispatchEnterState(state3);
            Assert.That(state3, Is.EqualTo(listener.lastEnterId), string.Format("last entered state should be {0}", state3));
            Assert.That(state1, Is.EqualTo(listener.lastExitId), string.Format("last exited state should be {0}", state1));
            Assert.AreEqual(4, listener.count, "should have fired 4 events");

            dispatcher.DispatchExitState(state3);
            Assert.That(state3, Is.EqualTo(listener.lastEnterId), string.Format("last entered state should be {0}", state3));
            Assert.That(state3, Is.EqualTo(listener.lastExitId), string.Format("last exited state should be {0}", state3));
            Assert.AreEqual(5, listener.count, "should have fired 5 events");

            dispatcher.DispatchExitState(state1);
            Assert.That(state3, Is.EqualTo(listener.lastEnterId), string.Format("last entered state should be {0}", state3));
            Assert.That(state1, Is.EqualTo(listener.lastExitId), string.Format("last exited state should be {0}", state1));
            Assert.AreEqual(6, listener.count, "should have fired 6 events");
        }

    }

    class TestDispatcherListener {

        public string lastEnterId;

        public string lastExitId;

        public int count;

        IFSMDispatcher dispatcher;

        public TestDispatcherListener(IFSMDispatcher theDispatcher)
        {
            count = 0;
            dispatcher = theDispatcher;
            dispatcher.OnEnterState += OnEnter;
            dispatcher.OnExitState += OnExit;
        }

        void OnEnter(string id)
        {
            count++;
            lastEnterId = id;
        }

        void OnExit(string id)
        {
            count++;
            lastExitId = id;
        }
    }
}
