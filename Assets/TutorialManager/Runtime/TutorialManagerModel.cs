using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{
    [System.Serializable]
    public class TutorialManagerModel
    {
        public List<TutorialEntity> tutorials = new List<TutorialEntity>();
        public List<StepEntity> steps = new List<StepEntity>();
        public List<ContentEntity> content = new List<ContentEntity>();

        // Lookup tables
        public Dictionary<string, TutorialEntity> tutorialTable = new Dictionary<string, TutorialEntity>();
        public Dictionary<string, StepEntity> stepTable = new Dictionary<string, StepEntity>();
        public Dictionary<string, ContentEntity> contentTable = new Dictionary<string, ContentEntity>();
    }

    public class TutorialManagerModelMiddleware
    {
        private static TutorialManagerModelMiddleware m_Instance;
        public static TutorialManagerModelMiddleware GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = new TutorialManagerModelMiddleware();
                m_Instance.TMData = new TutorialManagerModel();
                TMSerializer.ReadFromDisk(ref m_Instance.TMData);
                m_Instance.TMData.tutorialTable = m_Instance.TMData.tutorials.ToDictionary(t => t.id, t => t);
                m_Instance.TMData.stepTable = m_Instance.TMData.steps.ToDictionary(s => s.id, s => s);
                m_Instance.TMData.contentTable = m_Instance.TMData.content.ToDictionary(c => c.id, c => c);
            }
            return m_Instance;
        }

        public TutorialManagerModel TMData;

#if UNITY_EDITOR
        public string CreateTutorialEntity(string id)
        {
            string result = null;
            // Enforce tutorial rule #1.
            // Tutorial id must be unique.
            if (TMData.tutorialTable.ContainsKey(id) == false)
            {
                var tutorial = new TutorialEntity(id);

                TMData.tutorials.Add(tutorial);
                TMData.tutorialTable.Add(id, tutorial);
                result = tutorial.id;
                Save();
            }
            return result;
        }
#endif
        public string UpdateTutorialEntity(string oldId, string newId)
        {
            var result = oldId;
            if (TMData.tutorialTable.ContainsKey(oldId) && TMData.tutorialTable.ContainsKey(newId) == false)
            {
                var tutorial = TMData.tutorialTable[oldId];
                TMData.tutorialTable.Remove(oldId);
                tutorial.id = newId;
                TMData.tutorialTable.Add(newId, tutorial);
                result = newId;

                // migrate associated steps
                for (int a = 0; a < tutorial.steps.Count; a++)
                {
                    string oldStepId = tutorial.steps[a];
                    string stepBase = oldStepId.Substring(oldId.Length + 1);
                    string newStepId = GetStepId(stepBase, newId);

                    UpdateStepEntity(tutorial.steps[a], newStepId);
                    Save();
                }
            }
            return result;
        }
#if UNITY_EDITOR
        public void DestroyTutorialEntity(string id)
        {
            if (TMData.tutorialTable.ContainsKey(id))
            {
                var tutorial = TMData.tutorialTable[id];
                while (tutorial.steps.Count > 0)
                {
                    DestroyStepEntity(tutorial.steps[tutorial.steps.Count - 1]);
                }
                TMData.tutorialTable.Remove(id);
                TMData.tutorials.Remove(tutorial);
                Save();
            }
        }

        public string CreateStepEntity(string id, string tutorialId)
        {
            string result = null;

            // Enforce step rule #1.
            // Must be attached to existing tutorial.
            if (TMData.tutorialTable.ContainsKey(tutorialId))
            {
                // Enforce step rule #2.
                // Tutorial must not contain a reference to a step with this id.
                var tutorial = TMData.tutorialTable[tutorialId];
                var stepId = GetStepId(id, tutorialId);
                if (tutorial.steps.Contains(stepId) == false)
                {
                    // Enforce step rule #3.
                    // stepId must be unique in the stepTable.
                    if (TMData.stepTable.ContainsKey(stepId) == false)
                    {
                        var step = new StepEntity(stepId);
                        TMData.steps.Add(step);
                        TMData.stepTable.Add(stepId, step);
                        tutorial.steps.Add(stepId);
                        result = stepId;
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
            if (TMData.stepTable.ContainsKey(oldId) && TMData.stepTable.ContainsKey(newId) == false)
            {
                var step = TMData.stepTable[oldId];
                step.id = newId;

                foreach (var tutorialEntity in GetTutorialsThatContainStepWithId(oldId))
                {
                    tutorialEntity.steps[tutorialEntity.steps.IndexOf(oldId)] = newId;
                }

                // Update content reference, if any
                string oldContentId = GetTextId(oldId);
                string newContentId = GetTextId(newId);
                if (TMData.contentTable.ContainsKey(oldContentId))
                {
                    var contentEntity = TMData.contentTable[oldContentId];
                    contentEntity.id = newContentId;
                    TMData.contentTable.Remove(oldContentId);
                    TMData.contentTable.Add(newContentId, contentEntity);
                }

                // Remove from stepTable
                TMData.stepTable.Remove(oldId);
                // Add to stepTable under new id
                TMData.stepTable.Add(newId, step);
                result = newId;
                Save();
            }
            return result;
        }
#if UNITY_EDITOR
        public void DestroyStepEntity(string id)
        {
            if (TMData.stepTable.ContainsKey(id))
            {
                // Remove from Tutorial
                foreach (var tutorialEntity in GetTutorialsThatContainStepWithId(id)) 
                {
                    tutorialEntity.steps.RemoveAt(tutorialEntity.steps.IndexOf(id));
                }

                // Remove related content entries
                List<string> contentToRemove = new List<string>();
                foreach (ContentEntity c in TMData.content)
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
                var step = TMData.stepTable[id];
                TMData.steps.Remove(step);
                TMData.stepTable.Remove(id);
                Save();
		    }
        }
#endif
        private IEnumerable<TutorialEntity> GetTutorialsThatContainStepWithId (string id)
        {
            return TMData.tutorials.Select(t => t).Where(t => t.steps.Any(s => s == id));
        }
#if UNITY_EDITOR
        public string CreateContentEntity(string stepId, ContentType contentType, string contentValue = null)
        {
            string result = null;
            // Enforce content rule #1.
            // Must be attached to existing step.
            if (TMData.stepTable.ContainsKey(stepId))
            {
                // Enforce content rule #2.
                // Step must not contain a reference to content with this type.
                var step = TMData.stepTable[stepId];
                string contentId = GetTextId(stepId);
                if (step.messaging.content.Contains(contentId) == false)
                {
                    // Enforce step rule #3.
                    // contentId must be unique in the contentTable.
                    if (TMData.contentTable.ContainsKey(contentId) == false)
                    {
                        var contentEntity = new ContentEntity(contentId, contentType.ToString(), contentValue);

                        TMData.content.Add(contentEntity);
                        TMData.contentTable.Add(contentId, contentEntity);
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
            if (TMData.contentTable.ContainsKey(id))
            {
                ContentEntity contentEntity = TMData.contentTable[id];
                contentEntity.text = contentInfo;
                Save();
            }
        }
#if UNITY_EDITOR
        public void DestroyContentEntity(string id)
        {
            if (TMData.contentTable.ContainsKey(id))
            {
                var contentEntity = TMData.contentTable[id];
                TMData.contentTable.Remove(id);
                TMData.content.Remove(contentEntity);

                // Remove from Step
                foreach (StepEntity step in TMData.steps)
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
            while (TMData.tutorials.Count > 0)
            {
                TMData.tutorials.RemoveAt(0);
            }
            while (TMData.steps.Count > 0)
            {
                TMData.steps.RemoveAt(0);
            }
            while (TMData.content.Count > 0)
            {
                TMData.content.RemoveAt(0);
            }
            TMData.tutorialTable.Clear();
            TMData.stepTable.Clear();
            TMData.contentTable.Clear();
            Save();
        }
#endif

        void Save()
        {
            TMSerializer.WriteToDisk(ref TMData);
        }

        string GetStepId (string stepId, string tutorialId)
        {
            return string.Format("{0}-{1}", tutorialId, stepId); 
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