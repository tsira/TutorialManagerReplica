using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Analytics {
	public class TutorialManagerModel : ScriptableObject
	{
        private static TutorialManagerModel m_Instance;
        public static TutorialManagerModel GetInstance()
        {
            if (m_Instance == null) {
                m_Instance = new TutorialManagerModel();
            }
            return m_Instance;
        }


        public bool enabled;
        public List<TutorialEntity> tutorials;
        public List<StepEntity> steps;
        public List<ContentEntity> content;

        // Lookup tables
        public Dictionary<string, TutorialEntity> tutorialTable = new Dictionary<string, TutorialEntity>();
        public Dictionary<string, StepEntity> stepTable = new Dictionary<string, StepEntity>();
        public Dictionary<string, ContentEntity> contentTable = new Dictionary<string, ContentEntity>();

        public void CreateTutorialEntity(string id)
        {

        }

        public void UpdateTutorialEntity(string oldId, string newId)
        {

        }

        public void DestroyTutorialEntity(string id)
        {

        }

        public void CreateStepEntity(string id, string tutorialId)
        {

        }

        public void UpdateStepEntity(string oldId, string newId)
        {

        }

        public void DestroyStepEntity(string id)
        {

        }

        public void CreateContentEntity(string id, string contentType, string stepId)
        {

        }

        public void UpdateContentEntity(string id, string content)
        {

        }

        public void DestroyContentEntity(string id)
        {

        }

        void RelateEntity<T>(string id, string relatesTo) where T : Entity
        {

        }
	}

    [System.Serializable]
    public class Entity {}


    [System.Serializable]
    public class TutorialEntity : Entity
    {
        public string id;
        public bool isActive;
        public string[] steps;
    }

    [System.Serializable]
    public class StepEntity : Entity
    {
        public string id;
        public bool isActive;
        public Messaging messaging;
    }

    [System.Serializable]
    public class Messaging
    {
        public bool isActive;
        public string[] content;
    }

    [System.Serializable]
    public class ContentEntity : Entity
    {
        public string type;
        public string id;
        public string text;
    }
}