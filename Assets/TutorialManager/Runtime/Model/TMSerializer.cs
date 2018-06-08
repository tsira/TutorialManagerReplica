using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{

    internal static class TMSerializer
    {
        static string k_ModelFileName = "unity_tutorial_manager.dat";
        static string k_StateFileName = "unity_tutorial_manager_state.dat";

        internal static void ReadFromDisk<T>(ref T model)
        {
            string filePath = GetPath<T>(false);
            if (File.Exists(filePath)) {
                try {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    FileStream file = File.Open(filePath, FileMode.Open);
                    model = (T)binaryFormatter.Deserialize(file);
                    file.Close();
                } catch(Exception e) {
                    Debug.LogWarning("Exception at: " + filePath);
                    Debug.LogWarning("            : " + e.ToString());
                }
            } else {
                // If we get here, the model never existed...write it.
                WriteToDisk<T>(ref model);
            }
        }

        internal static void WriteToDisk<T>(ref T model)
        {
            string filePath = GetPath<T>(true);
            FileStream file = File.Create(filePath);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(file, model);
            file.Close();
        }

        static string GetPath<T>(bool toWrite)
        {
            if (typeof(T) == typeof(TutorialManagerModel)) {
                if (Application.isPlaying) {
                    string persistentModelPath = GetPersistentModelPath();
                    if (toWrite) {
                        return persistentModelPath;
                    }
                    if (File.Exists(persistentModelPath)) {
                        return persistentModelPath;
                    }
                    return GetBundledModelPath();
                } else {
                    return GetBundledModelPath();
                }
            } else {
                return GetPersistentStatePath();
            }
        }

        static string GetPersistentStatePath()
        {
            return Path.Combine(Application.persistentDataPath, k_StateFileName);
        }

        static string GetBundledModelPath()
        {
            string path = Path.Combine("Assets", "Resources");
            if (Directory.Exists(path) == false) {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(path, k_ModelFileName);
            return path;
        }

        static string GetPersistentModelPath()
        {
            return Path.Combine(Application.persistentDataPath, k_ModelFileName);
        }
    }
}
