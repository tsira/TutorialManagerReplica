using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Analytics.TutorialManagerRuntime; 

namespace UnityEngine.Analytics
{

    public class TMFiniteStateMachineTests
    {

        List<string> aListOfStates = new List<string> { "a", "b", "c", "d", "e" };
        List<string> anotherListOfStates = new List<string> { "aa", "bb", "cb", "dd" };



        [Test]
        public void PrimeStateList()
        {
            var fsm = new TutorialManagerFSM();
            fsm.dispatcher = new MockStateDispatcher();

            Assert.IsEmpty(fsm.stateList);
            Assert.IsTrue(string.IsNullOrEmpty(fsm.state));
            Assert.AreEqual(-1, fsm.index);

            fsm.stateList = aListOfStates;

            Assert.AreEqual(aListOfStates.Count, fsm.stateList.Count, "state list is loaded");
            Assert.IsTrue(string.IsNullOrEmpty(fsm.state), "no state yet");
            Assert.AreEqual(-1, fsm.index, "no state index yet");

            fsm.stateList = anotherListOfStates;

            Assert.AreEqual(anotherListOfStates.Count, fsm.stateList.Count, "another state list is loaded");
            Assert.IsTrue(string.IsNullOrEmpty(fsm.state), "no state yet");
            Assert.AreEqual(-1, fsm.index, "no state index yet");
        }

        [Test]
        public void AutoAdvanceThroughStates()
        {
            var fsm = new TutorialManagerFSM();
            fsm.dispatcher = new MockStateDispatcher();
            var dispatcher = (MockStateDispatcher)(fsm.dispatcher);
            fsm.stateList = aListOfStates;

            fsm.NextState();

            Assert.AreEqual(1, dispatcher.fireCount, "one (enter) event fired");
            Assert.AreEqual(0, fsm.index, "fsm index should be 0");
            Assert.That(aListOfStates[0], Is.EqualTo(dispatcher.lastEnterFired), string.Format("should have entered {0}", aListOfStates[0]));
            Assert.IsNull(dispatcher.lastExitFired, "exit should not have fired");

            fsm.NextState();

            Assert.AreEqual(3, dispatcher.fireCount, "three events should have fired");
            Assert.AreEqual(1, fsm.index, "fsm index should be 1");
            Assert.That(aListOfStates[0], Is.EqualTo(dispatcher.lastExitFired), string.Format("should have exited {0}", aListOfStates[0]));
            Assert.That(aListOfStates[1], Is.EqualTo(dispatcher.lastEnterFired), string.Format("should have entered {0}", aListOfStates[1]));
            Assert.That(fsm.state, Is.EqualTo(aListOfStates[1]), string.Format("current state should be {0}", aListOfStates[1]));

            fsm.NextState();
            Assert.AreEqual(5, dispatcher.fireCount, "5 events should have fired");
            fsm.NextState();
            Assert.AreEqual(7, dispatcher.fireCount, "7 events should have fired");
            fsm.NextState();
            Assert.AreEqual(9, dispatcher.fireCount, "9 events should have fired");

            Assert.AreEqual(4, fsm.index, "fsm index should be 4");
            Assert.That(aListOfStates[3], Is.EqualTo(dispatcher.lastExitFired), string.Format("should have exited {0}", aListOfStates[3]));
            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastEnterFired), string.Format("should have entered {0}", aListOfStates[4]));
            Assert.That(fsm.state, Is.EqualTo(aListOfStates[4]), string.Format("current state should be {0}", aListOfStates[4]));

            fsm.NextState();
            Assert.AreEqual(10, dispatcher.fireCount, "10 events should have fired");
            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastExitFired), string.Format("should have exited {0}", aListOfStates[4]));
            Assert.AreEqual(-1, fsm.index, "fsm index should be -1");
            Assert.IsTrue(string.IsNullOrEmpty(fsm.state), "current state should be empty");

            Assert.IsTrue(fsm.complete, "FSM should be complete");

            // Monkey bashing
            fsm.NextState();
            fsm.NextState();
            fsm.NextState();
            fsm.NextState();

            Assert.AreEqual(10, dispatcher.fireCount, "10 events should have fired (no change)");
            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastExitFired), string.Format("last exit should be unchanged at {0}", aListOfStates[4]));
            Assert.IsTrue(string.IsNullOrEmpty(fsm.state), "current state should be empty");
            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastEnterFired), string.Format("last enter should be unchanged at {0}", aListOfStates[4]));

            Assert.IsTrue(fsm.complete, "FSM should still be complete");
        }

        [Test]
        public void ManualAdvanceThroughStates()
        {
            bool fsmShouldAutoAdvance = false;

            var fsm = new TutorialManagerFSM(fsmShouldAutoAdvance);
            fsm.dispatcher = new MockStateDispatcher();

            var dispatcher = (MockStateDispatcher)(fsm.dispatcher);
            fsm.stateList = aListOfStates;

            // Enter state 0
            fsm.NextState();

            Assert.AreEqual(1, dispatcher.fireCount, "one (enter) event fired");
            Assert.AreEqual(0, fsm.index, "fsm index should be 0");
            Assert.That(aListOfStates[0], Is.EqualTo(dispatcher.lastEnterFired), string.Format("should have entered {0}", aListOfStates[0]));
            Assert.That(fsm.state, Is.EqualTo(aListOfStates[0]), string.Format("current state should be {0}", aListOfStates[0]));
            Assert.IsNull(dispatcher.lastExitFired, "exit should not have fired");

            // Exit state 0
            fsm.NextState();

            Assert.AreEqual(2, dispatcher.fireCount, "two events should have fired");
            Assert.AreEqual(-1, fsm.index, "fsm index should be -1");
            Assert.That(aListOfStates[0], Is.EqualTo(dispatcher.lastExitFired), string.Format("should have exited {0}", aListOfStates[0]));
            Assert.IsTrue(string.IsNullOrEmpty(fsm.state), "current state should be empty");
            Assert.That(aListOfStates[0], Is.EqualTo(dispatcher.lastEnterFired), string.Format("entered should still be {0}", aListOfStates[0]));

            // Enter state 1
            fsm.NextState();

            Assert.AreEqual(3, dispatcher.fireCount, "three events should have fired");
            Assert.AreEqual(1, fsm.index, "fsm index should be 1");
            Assert.That(fsm.state, Is.EqualTo(aListOfStates[1]), string.Format("current state should be {0}", aListOfStates[1]));
            Assert.That(aListOfStates[0], Is.EqualTo(dispatcher.lastExitFired), string.Format("should have exited {0}", aListOfStates[0]));
            Assert.That(aListOfStates[1], Is.EqualTo(dispatcher.lastEnterFired), string.Format("entered should still be {0}", aListOfStates[1]));

            // Exit state 1
            fsm.NextState();

            Assert.AreEqual(4, dispatcher.fireCount, "four events should have fired");
            Assert.AreEqual(-1, fsm.index, "fsm index should be -1");
            Assert.That(aListOfStates[1], Is.EqualTo(dispatcher.lastExitFired), string.Format("should have exited {0}", aListOfStates[1]));
            Assert.IsTrue(string.IsNullOrEmpty(fsm.state), "current state should be empty");
            Assert.That(aListOfStates[1], Is.EqualTo(dispatcher.lastEnterFired), string.Format("entered should still be {0}", aListOfStates[1]));

            // Enter/exit state 2
            fsm.NextState();
            fsm.NextState();
            Assert.AreEqual(6, dispatcher.fireCount, "6 events should have fired");

            // Enter/exit state 3
            fsm.NextState();
            fsm.NextState();
            Assert.AreEqual(8, dispatcher.fireCount, "8 events should have fired");

            // Enter/exit state 4
            fsm.NextState();
            fsm.NextState();
            Assert.AreEqual(10, dispatcher.fireCount, "10 events should have fired");

            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastExitFired), string.Format("should have exited {0}", aListOfStates[4]));
            Assert.IsTrue(string.IsNullOrEmpty(fsm.state), "current state should be empty");
            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastEnterFired), string.Format("entered should still be {0}", aListOfStates[4]));
            Assert.IsTrue(fsm.complete, "FSM should be complete");

            // Monkey-bashing
            fsm.NextState();
            fsm.NextState();
            fsm.NextState();

            Assert.AreEqual(10, dispatcher.fireCount, "10 events should have fired (no change)");
            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastExitFired), string.Format("last exit should be unchanged at {0}", aListOfStates[4]));
            Assert.IsTrue(string.IsNullOrEmpty(fsm.state), "current state should be empty");
            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastEnterFired), string.Format("last enter should be unchanged at {0}", aListOfStates[4]));

            Assert.IsTrue(fsm.complete, "FSM should still be complete");
        }

        [Test]
        public void GoToArbitraryStates()
        {
            var fsm = new TutorialManagerFSM();
            fsm.dispatcher = new MockStateDispatcher();

            var dispatcher = (MockStateDispatcher)(fsm.dispatcher);
            fsm.stateList = aListOfStates;

            // Go to state 3
            fsm.GoToState(aListOfStates[3]);

            Assert.AreEqual(1, dispatcher.fireCount, "one (enter) event fired");
            Assert.AreEqual(3, fsm.index, "fsm index should be 3");
            Assert.That(aListOfStates[3], Is.EqualTo(dispatcher.lastEnterFired), string.Format("should have entered {0}", aListOfStates[3]));
            Assert.That(fsm.state, Is.EqualTo(aListOfStates[3]), string.Format("current state should be {0}", aListOfStates[3]));
            Assert.IsNull(dispatcher.lastExitFired, "exit should not have fired");

            // Advance to state 4
            fsm.NextState();

            Assert.AreEqual(3, dispatcher.fireCount, "three events should have fired");
            Assert.AreEqual(4, fsm.index, "fsm index should be 4");
            Assert.That(fsm.state, Is.EqualTo(aListOfStates[4]), string.Format("current state should be {0}", aListOfStates[4]));
            Assert.That(aListOfStates[3], Is.EqualTo(dispatcher.lastExitFired), string.Format("should have exited {0}", aListOfStates[3]));
            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastEnterFired), string.Format("entered should still be {0}", aListOfStates[4]));

            // Jump to state 0
            fsm.GoToState(aListOfStates[0]);

            Assert.AreEqual(5, dispatcher.fireCount, "five events fired");
            Assert.AreEqual(0, fsm.index, "fsm index should be 0");
            Assert.That(aListOfStates[0], Is.EqualTo(dispatcher.lastEnterFired), string.Format("should have entered {0}", aListOfStates[0]));
            Assert.That(aListOfStates[4], Is.EqualTo(dispatcher.lastExitFired), string.Format("should have exited {0}", aListOfStates[4]));
            Assert.That(fsm.state, Is.EqualTo(aListOfStates[0]), string.Format("current state should be {0}", aListOfStates[0]));

        }
        class MockStateDispatcher : IFSMDispatcher
        {
            public string lastEnterFired;
            public string lastExitFired;
            public int fireCount = 0;
            
            public void DispatchEnterState(string id)
            {
                fireCount++;
                lastEnterFired = id;
            }
            
            public void DispatchExitState(string id)
            {
                fireCount++;
                lastExitFired = id;
            }
        }
    }

}
