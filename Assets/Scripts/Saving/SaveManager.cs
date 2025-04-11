using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    private Dictionary<string, object> saveData = new Dictionary<string, object>();

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public void Save()
    {
        saveData.Clear();

        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (var s in saveables)
        {
            saveData[s.SaveKey] = s.CaptureData();
        }

        string json = JsonUtility.ToJson(new SerializationWrapper(saveData));
        File.WriteAllText(SavePath, json);
        Debug.Log("Game saved to: " + SavePath);
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("Save file not found.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var wrapper = JsonUtility.FromJson<SerializationWrapper>(json);
        
        ISaveable[] saveables = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISaveable>()
            .ToArray();

        foreach (var s in saveables)
        {
            if (wrapper.Data.TryGetValue(s.SaveKey, out var saved))
            {
                s.RestoreData(saved);
            }
        }

        Debug.Log("Game loaded.");
    }

    [System.Serializable]
    private class SerializationWrapper
    {
        public List<string> keys = new();
        public List<string> jsonValues = new();

        public SerializationWrapper(Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                jsonValues.Add(JsonUtility.ToJson(kvp.Value));
            }
        }

        public Dictionary<string, object> Data
        {
            get
            {
                var result = new Dictionary<string, object>();
                for (int i = 0; i < keys.Count; i++)
                {
                    result[keys[i]] = JsonUtility.FromJson<object>(jsonValues[i]);
                }
                return result;
            }
        }
    }
}