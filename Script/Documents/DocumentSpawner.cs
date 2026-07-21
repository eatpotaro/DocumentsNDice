using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;
using UnityEngine.EventSystems;
using System.IO;

public class DocumentSpawner : NetworkBehaviour, IPointerEnterHandler
{
    public TMP_Dropdown DocList;
    public Transform playerCam;
    public BasicDocument basicDocumentPrefab;
    public Whiteboard whiteboardPrefab;
    public void LoadOptions()
    {
        int lastID = PlayerPrefs.GetInt("lastID", 0);
        Dictionary<int, string> DocNameWithKey = new Dictionary<int, string>();
        for(int i = 1; i <= lastID+1; i++)
        {
            string serializedDoc = PlayerPrefs.GetString($"Document{i}", "-1");

            if(serializedDoc == "-1")
            {
                try
                {                    
                    serializedDoc = File.ReadAllText(Path.Combine(Application.persistentDataPath, $"Document{i}.png"));
                }
                catch
                {
                    serializedDoc = "-1";
                }
            }

            if(serializedDoc == "-1")
            {
                continue;
            }

            if(DocumentManager.Docs.ContainsKey(i))
            {
                continue;
            }

            DocNameWithKey.Add(i, serializedDoc);
        }

        List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>(0);
        optionData.Add(new TMP_Dropdown.OptionData("New Basic Document"));
        optionData.Add(new TMP_Dropdown.OptionData("New Character Sheet"));
        optionData.Add(new TMP_Dropdown.OptionData("New Whiteboard"));

        foreach(var (key, value) in DocNameWithKey)
        {
            optionData.Add(new TMP_Dropdown.OptionData($"{value.Split("\n")[0].Substring(1)}:{key}"));
        }

        DocList.ClearOptions();
        DocList.AddOptions(optionData);
    }

    void Start()
    {
        DocumentManager.Docs = new Dictionary<int, Document>();
        LoadOptions();
    }

    public void SpawnDocument()
    {
        string text = DocList.options[DocList.value].text;
        switch(text)
        {
            default:
                {
                    int key = int.Parse(text.Split(":")[1]);
                    SpawnDoc(key);
                    break;
                }
            case "New Basic Document":
                {
                    SpawnDoc("B");
                    break;
                }
            case "New Whiteboard":
                {
                    SpawnDoc("W");
                    break;
                }
            case "New Character Sheet":
                {
                    break;
                }
        }
    }

    void SpawnDoc(int key)
    {
        if (!IsOwner) 
        {
            return;
        }

        Vector3 position = playerCam.position + playerCam.forward * 15f;

        string docText;
        try
        {                    
            docText = File.ReadAllText(Path.Combine(Application.persistentDataPath, $"Document{key}.png"));
        }
        catch
        {
            docText = PlayerPrefs.GetString($"Document{key}");
        }
        SpawnDocServerRPC(position, key, docText);
    }
    void SpawnDoc(string type)
    {
        if (!IsOwner) 
        {
            return;
        }

        Vector3 position = playerCam.position + playerCam.forward * 15f;
        SpawnDocServerRPC(position, 0, type);
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnDocServerRPC(Vector3 pos, int key, string serialisedData)
    {   
        //get the document instance and spawn it
        char docType = serialisedData[0];
        Document newDoc;
        if(docType == 'B')
        {
            newDoc = Instantiate(basicDocumentPrefab, pos, quaternion.identity);
        }
        else if(docType == 'W')
        {
            newDoc = Instantiate(whiteboardPrefab, pos, quaternion.identity);
        }
        else
        {
            newDoc = Instantiate(basicDocumentPrefab, pos, quaternion.identity);
        }

        newDoc.NetworkObject.Spawn();
        newDoc.LoadRPC(key, serialisedData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LoadOptions();
    }
}
