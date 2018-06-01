using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnityEngine.Analytics.TutorialManagerRuntime
{

    internal static class TMSerializer
    {
        internal static void ReadFromDisk<T>(ref T model)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
#if UNITY_EDITOR
            string filePath = GetEditorModelPath();
#else
            string filePath = GetPath<T>();
#endif
            if (File.Exists(filePath)) {
                FileStream file = File.Open(filePath, FileMode.Open);
                model = (T)binaryFormatter.Deserialize(file);
                file.Close();
            }
        }

        internal static void WriteToDisk<T>(ref T model)
        {
#if UNITY_EDITOR
            string filePath = GetEditorModelPath();
#else
            string filePath = GetPath<T>();
#endif

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Create(filePath);

            binaryFormatter.Serialize(file, model);
            file.Close();
        }

        static string GetPath<T>()
        {
            if (typeof(T) == typeof(TutorialManagerModel)) {
                string persistentPath = GetPersistentModelPath();
                if (File.Exists(persistentPath)) {
                    return persistentPath;
                }
                return GetEditorModelPath();
            } else {
                return Path.Combine(Application.persistentDataPath, "unity_tutorial_manager_state.dat");
            }
        }

        static string GetEditorModelPath()
        {
            string path = Path.Combine("Assets", "Resources");
            path = Path.Combine(path, "unity_tutorial_manager.dat");
            return path;
        }

        static string GetPersistentModelPath()
        {
            return Path.Combine(Application.persistentDataPath, "unity_tutorial_manager.dat");
        }
    }
}
