using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Analytics {
	public class TutorialManagerModel : ScriptableObject
	{
        public bool enabled;
        public TutorialEntity[] tutorials;
        public StepEntity[] steps;
        public ContentEntity[] content;

        // Lookup tables
        public Dictionary<string, TutorialEntity> tutorialTable = new Dictionary<string, TutorialEntity>();
        public Dictionary<string, StepEntity> stepTable = new Dictionary<string, StepEntity>();
        public Dictionary<string, ContentEntity> contentTable = new Dictionary<string, ContentEntity>();

        public void CreateEntity<T>(string id) where T : Entity
        {

        }

        public void UpdateEntity<T>(string id) where T : Entity
        {

        }

        public void DestroyEntity<T>(string id) where T : Entity
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
        public string name;
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