using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    private Dictionary<string, string> saveData = new();

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public void Save()
    {
        saveData.Clear();

        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (var s in saveables)
        {
            object captured = s.CaptureData();
            saveData[s.SaveKey] = JsonUtility.ToJson(captured);
        }

        string json = JsonUtility.ToJson(new SerializationWrapper(saveData));
        File.WriteAllText(SavePath, json);
        Debug.Log("Game saved to: " + SavePath);
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("Save file not found. Creating new save.");
            Save();
            return;
        }

        string json = File.ReadAllText(SavePath);
        var wrapper = JsonUtility.FromJson<SerializationWrapper>(json);

        ISaveable[] saveables = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISaveable>()
            .ToArray();

        foreach (var s in saveables)
        {
            if (wrapper.Data.TryGetValue(s.SaveKey, out var savedJson))
            {
                s.RestoreData(savedJson);
            }
        }

        Debug.Log("Game loaded from " + SavePath);
    }

    [System.Serializable]
    private class SerializationWrapper
    {
        public List<string> keys = new();
        public List<string> jsonValues = new();

        public SerializationWrapper(Dictionary<string, string> dict)
        {
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                jsonValues.Add(kvp.Value);
            }
        }

        public Dictionary<string, string> Data
        {
            get
            {
                var result = new Dictionary<string, string>();
                for (int i = 0; i < keys.Count; i++)
                {
                    result[keys[i]] = jsonValues[i];
                }
                return result;
            }
        }
    }
}
