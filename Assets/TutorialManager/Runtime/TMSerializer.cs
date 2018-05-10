using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{

    internal static class TMSerializer
    {
        internal static void ReadFromDisk(ref TutorialManagerModel model)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            if (File.Exists(GetSavePath()))
            {
                FileStream file = File.Open(GetSavePath(), FileMode.Open);
                model = (TutorialManagerModel)binaryFormatter.Deserialize(file);
                file.Close();
            }
        }

        internal static void WriteToDisk(ref TutorialManagerModel model)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Create(GetSavePath());

            binaryFormatter.Serialize(file, model);
            file.Close();
        }

        static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, "unity_tutorial_manager.dat");
        }
    }
}