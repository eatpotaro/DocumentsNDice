using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class Document : Interactable
{
    public int DocID;
    public GameObject UIPanel;
    public TMP_InputField title;
    private NetworkVariable<FixedString4096Bytes> titleText = new();

    public void Start()
    {
        titleText.OnValueChanged += OnTitleChanged;
        if(DocID != 0)
        {
            if(PlayerPrefs.GetInt("lastID", 0) < DocID)
            {
                PlayerPrefs.SetInt("lastID", DocID);
                PlayerPrefs.Save();
            }
            DocumentManager.Docs.Add(DocID, this);
            return;
        }
        DocID = DocumentManager.getNextID();
        DocumentManager.Docs.Add(DocID, this);
    }

    void OnDestroy()
    {
        base.OnDestroy();
        DocumentManager.Docs.Remove(DocID);
    }

    public override void OnInteract()
    {
        if(UIPanel.activeInHierarchy)
        {
            return;
        }
        
        UIPanel.SetActive(true);
        PlayerUIManager.instance.HideUI();
    }
    public override void AltInteract()
    {
        if(UIPanel.activeInHierarchy)
        {
            return;
        }
        PlayerUIManager.instance.ShowUI();
        SendDestroyRPC();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void LoadRPC(int key, string data)
    {
        Load(key, data);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SendDestroyRPC()
    {
        NetworkBehaviour.Destroy(gameObject);
    }

    public void EndInteract()
    {
        UIPanel.SetActive(false);
        PlayerUIManager.instance.ShowUI();
    }
    public void Save()
    {
        SaveRPC(title.text);
        DocumentManager.SaveToPrefs();
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SaveRPC(string newTitle)
    {
        titleText.Value = newTitle;
        UpdateTitleRPC(newTitle);
    }
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdateTitleRPC(string newTitle)
    {
        title.text = newTitle;
    }

    void OnTitleChanged(FixedString4096Bytes oldTitle, FixedString4096Bytes newTitle)
    {
        SaveRPC(title.text);
    }
    public virtual string Serialize()
    {
        return "";
    }
    public virtual string GetDocType()
    {
        return "";
    }
    public string GetTitle()
    {
        return titleText.Value.Value;
    }
    public virtual void Load(int key, string serialised)
    {
        
    }
}
