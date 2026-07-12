using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;
using UnityEngine.EventSystems;

public class DocumentSpawner : NetworkBehaviour, IPointerEnterHandler
{
    public TMP_Dropdown DocList;
    public Transform playerCam;
    public BasicDocument basicDocumentPrefab;
    public void LoadOptions()
    {
        int lastID = PlayerPrefs.GetInt("lastID", 0);
        Dictionary<int, string> DocNameWithKey = new Dictionary<int, string>();
        for(int i = 1; i <= lastID+1; i++)
        {
            string serializedDoc = PlayerPrefs.GetString($"Document{i}", "-1");

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
                    break;
                }
            case "New Whiteboard":
                {
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
        SpawnDocServerRPC(position, key, PlayerPrefs.GetString($"Document{key}"));
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnDocServerRPC(Vector3 pos, int key, string serialisedData)
    {   
        //get the document instance and spawn it
        char docType = serialisedData[0];
        Document newDoc;
        /*if(docType == 'C')
        {
            newDoc = Instantiate(basicDocumentPrefab);
        }
        else if(docType == 'W')
        {
            newDoc = Instantiate(basicDocumentPrefab);
        }
        else
        {*/
            newDoc = Instantiate(basicDocumentPrefab, pos, quaternion.identity);
        //}

        newDoc.NetworkObject.Spawn();
        newDoc.Load(key, serialisedData);

        SendReloadRPC();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendReloadRPC()
    {
        LoadOptions();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log(DocumentManager.Docs.Keys.Count);
        LoadOptions();
    }
}
