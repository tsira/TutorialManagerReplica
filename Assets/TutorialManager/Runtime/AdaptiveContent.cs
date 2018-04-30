using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutorialManager {

    [ExecuteInEditMode]
    public class AdaptiveContent : MonoBehaviour, IBindable {
       
        public IBindingResolver bindingResolver;

        [SerializeField]
        public string uid;
        public string uniqueId {
            get {
                return uid;
            }
        }

        [SerializeField]
		List<Binding> _bindings = new List<Binding>();
        public List<Binding> bindings {
            get {
                return _bindings;
            }
        }

        virtual protected void Start()
        {
            if (string.IsNullOrEmpty(uniqueId)) {
                uid = Guid.NewGuid().ToString();

                Debug.Log("New GUID " + uid);
            }
        }

        public void BindTo(string id) {
            _bindings.Clear();
            GetResolver().Resolve(this, id);
        }

        IBindingResolver GetResolver() {
            if (bindingResolver == null)
            {
                bindingResolver = TutorialManagerResolver.GetInstance();
            }
            return bindingResolver;
        }
    }
}