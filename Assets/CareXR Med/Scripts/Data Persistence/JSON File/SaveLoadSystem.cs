using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Debug = XRDebug;

public static class SaveLoadSystem
{
    [SerializeField] static string savePath => $"{Application.persistentDataPath}/Data";

    [ContextMenu("Save")]
    public static void Save()
    {
        var state = LoadFile();
        SaveState(state);   
        SaveFile(state);
    }

    [ContextMenu("Load")]
    public static void Load()
    {
        var state = LoadFile();
        LoadState(state);   
    }

    static void SaveFile(object state)
    {
        using (var stream = File.OpenWrite(savePath))
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, state); 
        }
    }

    static Dictionary<string, object> LoadFile()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No save file founded");
            return new Dictionary<string, object>();
        }

        using (FileStream stream = File.Open(savePath, FileMode.Open))
        {
            var formatter = new BinaryFormatter();
            return (Dictionary<string, object>)formatter.Deserialize(stream);  
        }
    }

    static void SaveState(Dictionary<string, object> state)
    {
        foreach (var saveable in Object.FindObjectsOfType<SaveableEntity>())
        {
            state[saveable.EntityID] = saveable.SaveState();
        }
    }

    static void LoadState(Dictionary<string, object> state)
    {
        foreach (var saveable in Object.FindObjectsOfType<SaveableEntity>())
        {
            if (state.TryGetValue(saveable.EntityID, out var savedState))
                saveable.LoadState(savedState);
        }
    }
}
