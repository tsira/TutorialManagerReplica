using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Analytics.TutorialManager {
    public class TutorialManagerResolver : IBindingResolver
    {
        static IBindingResolver resolver;
        public static IBindingResolver GetInstance() {
            if (resolver == null)
                resolver = new TutorialManagerResolver();
            return resolver;
        }

        public bool IsResolved(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            return true;
        }

        public void Resolve(IBindable target, string id)
        {
            // The target is probably some type of AdaptiveContent
            if (target is AdaptiveContent) {
                Binding isActiveBinding = new Binding();
                isActiveBinding.m_Field = "isActive";
                isActiveBinding.m_Type = "step";
                isActiveBinding.m_LocationId = id;
                target.bindings.Add(isActiveBinding);
            }

            // It may ALSO be AdaptiveText
            if (target is AdaptiveText)
            {
                string guid = target.uniqueId.ToString();
                Binding textBinding = new Binding();
                textBinding.m_Field = "text";
                textBinding.m_Type = "content/text";
                textBinding.m_LocationId = guid;
                target.bindings.Add(textBinding);
            }
        }

        public void WriteString(List<Binding> bindings, string dataType, string fieldName, string content) {

            string locationId = "";
            foreach(Binding b in bindings) {
                if (fieldName == b.m_Field && dataType == b.m_Type) {
                    locationId = b.m_LocationId;
                    break;
                }
            }

            if (TMParser.manifest.allIds.ContainsKey(locationId) == false)
            {
                if (dataType == "content/text") {
                    var contentEntity = new ContentEntity();
                    contentEntity.type = "text";
                    contentEntity.id = locationId;
                    TMParser.RegisterNewEntity(contentEntity);
                }
            }

            var entity = TMParser.manifest.allIds[locationId];

            if (entity is Step && fieldName == "name")
            {
                var stepEntity = (Step)entity;
                stepEntity.name = content;
            }
            else if (entity is ContentEntity && fieldName == "text")
            {
                var contentEntity = (ContentEntity)entity;
                contentEntity.text = content;
                TMParser.UpdateEntity(contentEntity);
                
            }
        }

        public void WriteBool(bool content, string fieldName, string locationId){
            var item = TMParser.manifest.allIds[locationId];
            if (item is Step && fieldName == "isActive")
            {
                var stepItem = (Step)item;
                stepItem.isActive = content;
            }
        }
    }
}
