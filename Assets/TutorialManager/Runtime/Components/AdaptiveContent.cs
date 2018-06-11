using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Analytics.TutorialManagerRuntime;

namespace UnityEngine.Analytics {
    public class AdaptiveContent : MonoBehaviour, IStateSystemSubject
    {
        public List<string> bindingIds = new List<string>();

        public bool respectRemoteIsActive = true;

        public IFSMDispatcher dispatcher { get; set; }

        public IDataStore dataStore { get; set; }

        protected string bindingId;

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

        protected virtual void OnEnterState(string id)
        {
            int index = bindingIds.IndexOf(id);
            bool toShow = (index > -1);
            if (toShow) {
                bindingId = id;
                toShow = respectRemoteIsActive ? dataStore.GetBool(id) : true;
            }
            gameObject.SetActive(toShow);
        }

        void OnExitState(string id)
        {
            gameObject.SetActive(false);
        }
    }
}
