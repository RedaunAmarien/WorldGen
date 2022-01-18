using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SFB;

public static class FileIO
{
    public static void SaveFile()
    {
        SaveData data = GameRam.ToSaveData();
        string saveName = "Planet " + GameRam.planet.seed;
        string savePath = StandaloneFileBrowser.SaveFilePanel("Save File", Application.persistentDataPath, saveName, "mgsv");
        if (savePath != string.Empty)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
    }

    public static void AutoSaveFile()
    {
        SaveData data = GameRam.ToSaveData();
    }

    public static SaveData LoadFile()
    {
        string[] path = StandaloneFileBrowser.OpenFilePanel("Open File", Application.persistentDataPath, "mgsv", false);
        SaveData data = new SaveData();
        if (path[0] != string.Empty)
        {
            string json = File.ReadAllText(path[0]);
            data = JsonUtility.FromJson<SaveData>(json);
        }
        else return null;
        
        return data;
    }
}
