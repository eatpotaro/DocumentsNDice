using System.Collections.Generic;
using UnityEngine;

public static class DocumentManager
{
    public static Dictionary<int, Document> Docs = new Dictionary<int, Document>();
    private static int lastID;

    public static void SaveToPrefs()
    {
        foreach((int key, Document doc) in Docs)
        {
            PlayerPrefs.SetString($"Document{key}", doc.Serialize());
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
