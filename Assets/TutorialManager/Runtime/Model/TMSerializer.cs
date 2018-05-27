using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{

    internal static class TMSerializer
    {
        internal static void ReadFromDisk<T>(ref T model)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            if (File.Exists(GetSavePath<T>()))
            {
                FileStream file = File.Open(GetSavePath<T>(), FileMode.Open);
                model = (T)binaryFormatter.Deserialize(file);
                file.Close();
            }
        }

        internal static void WriteToDisk<T>(ref T model)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Create(GetSavePath<T>());

            binaryFormatter.Serialize(file, model);
            file.Close();
        }

        static string GetSavePath<T>()
        {
            if (typeof(T) == typeof(TutorialManagerModel)) {
                return Path.Combine(Application.persistentDataPath, "unity_tutorial_manager.dat");
            } else {
                return Path.Combine(Application.persistentDataPath, "unity_tutorial_manager_state.dat");
            }
        }
    }
}