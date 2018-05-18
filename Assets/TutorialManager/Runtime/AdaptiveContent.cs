using UnityEngine;
using System.Collections;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics {
	public class AdaptiveContent : MonoBehaviour, IStateSystemSubject
	{
		public string bindingId;

        public IFSMDispatcher dispatcher { get; set; }

        public IDataStore dataStore { get; set; }

        void Awake()
        {
            StateSystemProvider.GetInstance().Provide(this);

            dispatcher.OnEnterState += OnEnterState;
            dispatcher.OnExitState += OnExitState;

            if (dispatcher.state == bindingId) {
                OnEnterState(dispatcher.state);
            }
        }

        void OnDestroy()
        {
            dispatcher.OnEnterState -= OnEnterState;
            dispatcher.OnExitState -= OnExitState;
        }


        void OnEnterState(string id)
        {
            if (id == bindingId) {
                gameObject.SetActive(true);
            }
        }

        void OnExitState(string id)
        {
            gameObject.SetActive(false);
        }
    }
}
