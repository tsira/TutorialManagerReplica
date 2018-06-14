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
            bool isModel = (typeof(T) == typeof(TutorialManagerModel));
            string filePath = string.Empty;

            if (isModel) {
                filePath = Application.isPlaying ? GetPersistentModelPath() : GetBundledModelPath();
                if (ReadBinary<T>(filePath, ref model) == false) {
                    // If we get here, the model never existed...write it.
                    WriteToDisk<T>(ref model);
                }
            } else {
                filePath = GetPersistentStatePath();
                ReadBinary<T>(filePath, ref model);
            }
        }

        static bool ReadBinary<T>(string filePath, ref T model) {
            if (File.Exists(filePath)) {
                try {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    FileStream file = File.Open(filePath, FileMode.Open);
                    model = (T)binaryFormatter.Deserialize(file);
                    file.Close();
                    return true;
                } catch (Exception e) {
                    Debug.LogWarning("Exception at: " + filePath);
                    Debug.LogWarning("            : " + e.ToString());
                    return false;
                }
            } else {
                return false;
            }
        }

        internal static void WriteToDisk<T>(ref T model)
        {
            bool isModel = (typeof(T) == typeof(TutorialManagerModel));
            if (isModel) {
                if (Application.isPlaying) {
                    string filePath = GetPersistentModelPath();
                    WriteBinary<T>(filePath, ref model);

                } else {
                    // Write to resources
                    string filePathEditor = GetBundledModelPath();
                    WriteBinary<T>(filePathEditor, ref model);

                    // AND write to persistent
                    string filePathPersistent = GetPersistentModelPath();
                    WriteBinary<T>(filePathPersistent, ref model);
                }
            } else {
                string filePathPersistent = GetPersistentStatePath();
                WriteBinary<T>(filePathPersistent, ref model);
            }
        }

        static void WriteBinary<T>(string filePath, ref T model)
        {
            FileStream file = File.Create(filePath);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(file, model);
            file.Close();
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
