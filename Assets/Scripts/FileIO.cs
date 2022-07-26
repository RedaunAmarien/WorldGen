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
        string saveName = string.Format("Planet [{0}]", GameRam.NoiseSettings.mSeed);
        string savePath = StandaloneFileBrowser.SaveFilePanel("Save File", Application.persistentDataPath, saveName, "mpgn");
        if (savePath != string.Empty)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
    }
    public static void ExportMap(Texture2D tex)
    {
        string saveName = string.Format("Planet [{0}]{1}{2}", GameRam.NoiseSettings.mSeed, GameRam.planet.worldManager.worldMapLocation == Vector2.zero ? "" : " at " + GameRam.planet.worldManager.worldMapLocation.ToString(), GameRam.planet.worldManager.worldMapZoom == 1 ? "" : " x" + GameRam.planet.worldManager.worldMapZoom);
        string savePath = StandaloneFileBrowser.SaveFilePanel("Export Map", Application.persistentDataPath, saveName, "png");
        if (savePath != string.Empty)
        {
            byte[] data = tex.EncodeToPNG();
            File.WriteAllBytes(savePath, data);
        }
    }

    public static void AutoSaveFile()
    {
        SaveData data = GameRam.ToSaveData();
    }

    public static SaveData LoadFile()
    {
        string[] path = StandaloneFileBrowser.OpenFilePanel("Open File", Application.persistentDataPath, "mpgn", false);
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
