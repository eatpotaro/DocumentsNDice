using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public static class DocumentManager
{
    public static Dictionary<int, Document> Docs = new Dictionary<int, Document>();
    private static int lastID;

    public static void SaveToPrefs()
    {
        foreach((int key, Document doc) in Docs)
        {
            string serialisedText = doc.Serialize();
            if(serialisedText[0] != 'W')
            {
                PlayerPrefs.SetString($"Document{key}", doc.Serialize());
            }
            else
            {
                File.WriteAllText(Path.Combine(Application.persistentDataPath, $"Document{key}.png"), serialisedText);
            }
            PlayerPrefs.SetInt("lastID", key);
        }
        PlayerPrefs.Save();
    }

    public static int getNextID()
    {
        if(PlayerPrefs.GetInt("lastID", 0) == 0)
        {
            PlayerPrefs.SetInt("lastID", 1);
            PlayerPrefs.Save();
        }
        lastID = PlayerPrefs.GetInt("lastID");
        lastID++;
        PlayerPrefs.SetInt("lastID", lastID);
        return lastID;
    }
}
