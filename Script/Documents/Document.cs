using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Document : Interactable
{
    public int DocID;
    public GameObject UIPanel;
    public TMP_InputField title;
    private string titleText;

    void Start()
    {
        if(DocID != 0)
        {
            if(PlayerPrefs.GetInt("lastID", 0) < DocID)
            {
                PlayerPrefs.SetInt("lastID", DocID);
            }
            DocumentManager.Docs.Add(DocID, this);
            return;
        }
        DocID = DocumentManager.getNextID();
        DocumentManager.Docs.Add(DocID, this);
    }

    public override void OnInteract()
    {
        UIPanel.SetActive(true);
        PlayerUIManager.instance.HideUI();
    }
    public void EndInteract()
    {
        UIPanel.SetActive(false);
        PlayerUIManager.instance.ShowUI();
    }
    public void Save()
    {
        titleText = title.text;
        DocumentManager.SaveToPrefs();
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
        return titleText;
    }
    public virtual void Load(int key, string serialised)
    {
        
    }
}
