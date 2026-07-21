using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;

public class BasicDocument : Document
{
    public TMP_InputField BodyText;
    private string text;

    void Start()
    {
        base.Start();
        NetworkManager.Singleton.OnConnectionEvent += UpdateNewClient;
    }
    public override string Serialize()
    {
        return $"B{GetTitle()}\n{BodyText.text}";
    }
    public override string GetDocType()
    {
        return "B";
    }

    public override void Load(int key, string serialised)
    {
        if(serialised == "B")
        {
            return;
        }

        string[] parts = serialised.Substring(1).Split("\n");
        title.text = parts[0];
        BodyText.text = parts[1];
        DocID = key;
    }

    public void OnEditText()
    {
        ServerUpdateTextRPC(BodyText.text);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ServerUpdateTextRPC(string newtext)
    {
        text = newtext;
        BodyText.text = newtext;

        UpdateTextRPC(newtext);
    } 

    [Rpc(SendTo.Everyone)]
    public void UpdateTextRPC(string newText)
    {
        UpdateText(newText);
    }
        
    public void UpdateText(string newText)
    {
        BodyText.text = newText;
        text = newText;
    } 
    
    public void UpdateNewClient(NetworkManager manager, ConnectionEventData data)
    {
        if(!IsServer)
        {
            return;
        }

        if(data.EventType != ConnectionEvent.ClientConnected)
        {
            return;
        }

        ApplyUpdatesRPC(data.ClientId, text);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission =RpcInvokePermission.Everyone)]
    public void ApplyUpdatesRPC(ulong client, string newText)
    {
        if(NetworkManager.Singleton.LocalClientId != client)
        {
            return;
        }

        UpdateText(newText);
    }
}
