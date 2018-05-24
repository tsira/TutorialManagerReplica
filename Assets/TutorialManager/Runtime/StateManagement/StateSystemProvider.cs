using UnityEngine;
using System.Collections;

namespace UnityEngine.Analytics
{
    public class StateSystemProvider
    {

        private IFSMDispatcher m_Dispatcher;
        private IDataStore m_DataStore;

        private static StateSystemProvider _instance;
        public static StateSystemProvider GetInstance()
        {
            if (_instance == null) {
                _instance = new StateSystemProvider();
            }
            return _instance;
        }

        public void SetDispatcher(IFSMDispatcher dispatcher)
        {
            m_Dispatcher = dispatcher;
        }

        public void SetDataStore(IDataStore dataStore)
        {
            m_DataStore = dataStore;
        }

        public void Provide(IStateSystemSubject subject)
        {
            if (m_Dispatcher != null)
                subject.dispatcher = m_Dispatcher;
            if (m_DataStore != null)
                subject.dataStore = m_DataStore;
        }
    }
}
