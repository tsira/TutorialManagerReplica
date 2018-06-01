using UnityEngine;
using System.Collections;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics {
    public class AdaptiveContent : MonoBehaviour, IStateSystemSubject
    {
        public string bindingId;

        public bool respectRemoteIsActive = true;

        public IFSMDispatcher dispatcher { get; set; }

        public IDataStore dataStore { get; set; }

        protected virtual void Start()
        {
            StateSystemProvider.GetInstance().Provide(this);

            dispatcher.OnEnterState += OnEnterState;
            dispatcher.OnExitState += OnExitState;
            dataStore.OnDataUpdate += OnDataUpdate;

            OnEnterState(dispatcher.state);
        }

        void OnDestroy()
        {
            dispatcher.OnEnterState -= OnEnterState;
            dispatcher.OnExitState -= OnExitState;
            dataStore.OnDataUpdate -= OnDataUpdate;
        }

        public virtual void OnDataUpdate()
        {
        }

        void OnEnterState(string id)
        {
            bool toShow = (id == bindingId);
            if (toShow) {
                toShow = respectRemoteIsActive ? dataStore.GetBool(bindingId) : true;
            }
            gameObject.SetActive(toShow);
        }

        void OnExitState(string id)
        {
            gameObject.SetActive(false);
        }
    }
}
