using UnityEngine;
using System.Collections;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics {
    public class AdaptiveContent : MonoBehaviour, IStateSystemSubject
    {
        [SerializeField]
        public string bindingId;

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

        protected virtual void OnDataUpdate()
        {
        }

        void OnEnterState(string id)
        {
            gameObject.SetActive(id == bindingId);
        }

        void OnExitState(string id)
        {
            gameObject.SetActive(false);
        }
    }
}