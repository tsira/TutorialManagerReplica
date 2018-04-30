using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TutorialManager {
    public interface IBindingResolver
    {
        bool IsResolved(string id);
        void Resolve(IBindable target, string id);

        void WriteString(List<Binding> bindings, string dataType, string fieldName, string content);
        void WriteBool(bool content, string fieldName, string locationId);
    }
}
