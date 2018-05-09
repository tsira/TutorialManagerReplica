using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    public class TutorialManagerModel : ScriptableObject
    {
        private static TutorialManagerModel m_Instance;
        public static TutorialManagerModel GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = ScriptableObject.CreateInstance<TutorialManagerModel>();
            }
            return m_Instance;
        }

        public bool enabled;
        public List<TutorialEntity> tutorials = new List<TutorialEntity>();
        public List<StepEntity> steps = new List<StepEntity>();
        public List<ContentEntity> content = new List<ContentEntity>();

        // Lookup tables
        public Dictionary<string, TutorialEntity> tutorialTable = new Dictionary<string, TutorialEntity>();
        public Dictionary<string, StepEntity> stepTable = new Dictionary<string, StepEntity>();
        public Dictionary<string, ContentEntity> contentTable = new Dictionary<string, ContentEntity>();

#if UNITY_EDITOR
        public string CreateTutorialEntity(string id)
        {
            string result = null;
            // Enforce tutorial rule #1.
            // Tutorial id must be unique.
            if (tutorialTable.ContainsKey(id) == false)
            {
                var tutorial = new TutorialEntity(id);

                tutorials.Add(tutorial);
                tutorialTable.Add(id, tutorial);
                result = tutorial.id;
                Save();
            }
            return result;
        }
#endif
        public string UpdateTutorialEntity(string oldId, string newId)
        {
            var result = oldId;
            if (tutorialTable.ContainsKey(oldId) && tutorialTable.ContainsKey(newId) == false)
            {
                var tutorial = tutorialTable[oldId];
                tutorialTable.Remove(oldId);
                tutorial.id = newId;
                tutorialTable.Add(newId, tutorial);
                result = newId;

                // migrate associated steps
                for (int a = 0; a < tutorial.steps.Count; a++)
                {
                    string oldStepId = tutorial.steps[a];
                    string stepBase = oldStepId.Substring(oldId.Length + 1);
                    string newStepId = string.Format("{0}-{1}", newId, stepBase);

                    UpdateStepEntity(tutorial.steps[a], newStepId);
                    Save();
                }
            }
            return result;
        }
#if UNITY_EDITOR
        public void DestroyTutorialEntity(string id)
        {
            if (tutorialTable.ContainsKey(id))
            {
                var tutorial = tutorialTable[id];
                while (tutorial.steps.Count > 0)
                {
                    DestroyStepEntity(tutorial.steps[tutorial.steps.Count - 1]);
                }
                tutorialTable.Remove(id);
                tutorials.Remove(tutorial);
                Save();
            }
        }

        public string CreateStepEntity(string id, string tutorialId)
        {
            string result = null;

            // Enforce step rule #1.
            // Must be attached to existing tutorial.
            if (tutorialTable.ContainsKey(tutorialId))
            {
                // Enforce step rule #2.
                // Tutorial must not contain a reference to a step with this id.
                var tutorial = tutorialTable[tutorialId];
                if (tutorial.steps.Contains(id) == false)
                {
                    // Enforce step rule #3.
                    // stepId must be unique in the stepTable.
                    if (stepTable.ContainsKey(id) == false)
                    {
                        var step = new StepEntity(id);
                        steps.Add(step);
                        stepTable.Add(id, step);
                        tutorial.steps.Add(id);
                        result = id;
                        Save();
                    }
                }
            }
            return result;
        }
#endif

        public string UpdateStepEntity(string oldId, string newId)
        {
            var result = oldId;
            if (stepTable.ContainsKey(oldId) && stepTable.ContainsKey(newId) == false)
            {
                var step = stepTable[oldId];
                step.id = newId;

                foreach (var tutorialEntity in GetTutorialsThatContainStepWithId(oldId))
                {
                    tutorialEntity.steps[tutorialEntity.steps.IndexOf(oldId)] = newId;
                }

                // Update content reference, if any
                string oldContentId = GetTextId(oldId);
                string newContentId = GetTextId(newId);
                if (contentTable.ContainsKey(oldContentId))
                {
                    var contentEntity = contentTable[oldContentId];
                    contentEntity.id = newContentId;
                    contentTable.Remove(oldContentId);
                    contentTable.Add(newContentId, contentEntity);
                }

                // Remove from stepTable
                stepTable.Remove(oldId);
                // Add to stepTable under new id
                stepTable.Add(newId, step);
                result = newId;
                Save();
            }
            return result;
        }
#if UNITY_EDITOR
        public void DestroyStepEntity(string id)
        {
            if (stepTable.ContainsKey(id))
            {
                // Remove from Tutorial
                foreach (var tutorialEntity in GetTutorialsThatContainStepWithId(id)) 
                {
                    tutorialEntity.steps.RemoveAt(tutorialEntity.steps.IndexOf(id));
                }

                // Remove related content entries
                List<string> contentToRemove = new List<string>();
                foreach (ContentEntity c in content)
                {
                    if (c.id.IndexOf(c.id, 0, System.StringComparison.Ordinal) > -1)
                    {
                        contentToRemove.Add(c.id);
                    }
                }
                foreach (string s in contentToRemove)
                {
                    DestroyContentEntity(s);
                }

                // Remove from stepTable
                // Remove from steps
                var step = stepTable[id];
                steps.Remove(step);
                stepTable.Remove(id);
                Save();
		    }
        }
#endif
        private IEnumerable<TutorialEntity> GetTutorialsThatContainStepWithId (string id)
        {
            return tutorials.Select(t => t).Where(t => t.steps.Any(s => s == id));
        }
#if UNITY_EDITOR
        public string CreateContentEntity(string stepId, ContentType contentType, string contentValue = null)
        {
            string result = null;
            // Enforce content rule #1.
            // Must be attached to existing step.
            if (stepTable.ContainsKey(stepId))
            {
                // Enforce content rule #2.
                // Step must not contain a reference to content with this type.
                var step = stepTable[stepId];
                if (step.messaging.content.Contains(contentType.ToString()) == false)
                {
                    string contentId = GetTextId(stepId);

                    // Enforce step rule #3.
                    // contentId must be unique in the contentTable.
                    if (contentTable.ContainsKey(contentId) == false)
                    {
                        var contentEntity = new ContentEntity(contentId, contentType.ToString(), contentValue);

                        content.Add(contentEntity);
                        contentTable.Add(contentId, contentEntity);
                        step.messaging.content.Add(contentId);
                        result = contentId;
                        Save();
                    }
                }
            }
            return result;
        }
#endif
        public void UpdateContentEntity(string id, string contentInfo)
        {
            if (contentTable.ContainsKey(id))
            {
                ContentEntity contentEntity = contentTable[id];
                contentEntity.text = contentInfo;
                Save();
            }
        }
#if UNITY_EDITOR
        public void DestroyContentEntity(string id)
        {
            if (contentTable.ContainsKey(id))
            {
                var contentEntity = contentTable[id];
                contentTable.Remove(id);
                content.Remove(contentEntity);

                // Remove from Step
                foreach (StepEntity step in steps)
                {
                    if (step.messaging.content.Contains(id))
                    {
                        step.messaging.content.Remove(id);
                        break;
                    }
                }
                Save();
            }
        }

        public void Clear()
        {
            while (tutorials.Count > 0)
            {
                tutorials.RemoveAt(0);
            }
            while (steps.Count > 0)
            {
                steps.RemoveAt(0);
            }
            while (content.Count > 0)
            {
                content.RemoveAt(0);
            }
            tutorialTable.Clear();
            stepTable.Clear();
            contentTable.Clear();
            Save();
        }
#endif

        void Save()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
#endif
            TMSerializer.WriteToDisk(this);
        }



        string GetTextId(string stepId)
        {
            return string.Format("{0}-{1}", stepId, ContentType.text.ToString());
        }
    }

    [System.Serializable]
    public class Entity
    {
        public string id;

        public Entity(string entityId)
        {
            id = entityId;
        }
    }


    [System.Serializable]
    public class TutorialEntity : Entity
    {
        public bool isActive;
        public List<string> steps = new List<string>();

        public TutorialEntity(string entityId, bool startActive = true) : base(entityId)
        {
            isActive = startActive;
        }
    }

    [System.Serializable]
    public class StepEntity : Entity
    {
        public bool isActive;
        public Messaging messaging = new Messaging();

        public StepEntity(string entityId, bool startActive = true) : base(entityId)
        {
            isActive = startActive;
        }
    }

    [System.Serializable]
    public class Messaging
    {
        public bool isActive;
        public List<string> content = new List<string>();
    }

    [System.Serializable]
    public class ContentEntity : Entity
    {
        public string type;
        public string text;

        public ContentEntity(string entityId, string contentType, string contentText) : base(entityId)
        {
            type = contentType;
            text = contentText;
        }
    }

    public enum ContentType
    {
        text
    }
}