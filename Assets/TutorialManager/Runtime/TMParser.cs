using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TutorialManager;
using System.IO;

namespace TutorialManager {
    [InitializeOnLoad]
    public static class TMParser {

        public static TMManifest manifest;

        static TMParser()
        {
            ForceUpdate();
        }

        static public void ForceUpdate() {
            TextAsset textAsset = Resources.Load<TextAsset>("TutorialManifest");
            manifest = JsonUtility.FromJson<TMManifest>(textAsset.text);
            manifest = ProcessManifest(manifest);
        }

        static public void SaveToJson() {
            string json = JsonUtility.ToJson(manifest);

            Debug.Log(json);

            //File.WriteAllText ("./Assets/Resources/Output.json", json);
            //Debug.Log("saved to: " + Path.GetFullPath("."));
        }

        static public TMManifest ProcessManifest(TMManifest data) {
            foreach (Tutorial t in data.tutorials) {
                data.tutorialTable[t.id] = t;
                data.allIds[t.id] = t;
            }

            foreach (Step s in data.steps)
            {
                data.stepTable[s.id] = s;
                data.allIds[s.id] = s;
            }

            foreach (ContentEntity c in data.content)
            {
                data.contentTable[c.id] = c;
                data.allIds[c.id] = c;
            }

            return data;
        }

        static public void RegisterNewEntity(ContentEntity contentEntity) {
            ContentEntity[] newContent = new ContentEntity[manifest.content.Length + 1];
            for (int a = 0; a < newContent.Length; a++) {
                if (a < newContent.Length-1) {
					newContent[a] = manifest.content[a];
                }
                else {
                    newContent[a] = contentEntity;
                }
            }
            manifest.content = newContent;
            manifest.contentTable.Add(contentEntity.id, contentEntity);
            manifest.allIds.Add(contentEntity.id, contentEntity);
        }

        static public void UpdateEntity(ContentEntity contentEntity) {
            for (int a = 0; a < manifest.content.Length; a++)
            {
                if (manifest.content[a].id == contentEntity.id) {
                    manifest.content[a] = contentEntity;
                    break;
                }
            }

            manifest.contentTable[contentEntity.id] = contentEntity;
            manifest.allIds[contentEntity.id] = contentEntity;
        }
    }

    [System.Serializable]
    public class TMManifest
    {
        public bool enabled;
        public Tutorial[] tutorials;
        public Step[] steps;
        public ContentEntity[] content;

        public Dictionary<string, Tutorial> tutorialTable = new Dictionary<string, Tutorial>();
        public Dictionary<string, Step> stepTable = new Dictionary<string, Step>();
        public Dictionary<string, ContentEntity> contentTable = new Dictionary<string, ContentEntity>();

        public Dictionary<string, object> allIds = new Dictionary<string, object>();
    }

    [System.Serializable]
    public struct Tutorial
    {
        public string id;
        public string name;
        public bool isActive;
        public string[] steps;
    }

    [System.Serializable]
    public struct Step
    {
        public string id;
        public string name;
        public bool isActive;
        public Messaging messaging;
    }

    [System.Serializable]
    public struct Messaging
    {
        public bool isActive;
        public string[] content;
    }

    [System.Serializable]
    public struct ContentEntity
    {
        public string type;
        public string id;
        public string text;
    }
}
