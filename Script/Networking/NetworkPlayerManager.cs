using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using System.Collections;
using UnityEngine.Networking;

public class NetworkPlayerManager : NetworkBehaviour
{

    public TMP_Text NameTag;
    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>();
    private Transform playerCam;

    public GameObject ScreenPromptParent;
    public GameObject ShowHideIPParent;
    private bool IPHidden;
    private string publicIP;
    public TMP_Text ShowHideText;
    public TMP_Text IPText;

    void Start()
    {
        if(!NetworkManager.Singleton.IsHost)
        {
            ShowHideIPParent.SetActive(false);
        }

        IPText.text = "Fetching IP...";
        StartCoroutine(nameof(GetPublicIP));
        ShowHideIP();
    }

    public override void OnNetworkSpawn()
    {   
        PlayerName.OnValueChanged += OnNameChanged;

        OnNameChanged(default, PlayerName.Value);

        if(playerCam == null)
        {
            try
            {
                playerCam = Camera.main.transform;
            }
            catch
            {
                TryGetCamera();
            }
        }

        if (IsOwner)
        {
            SubmitNameServerRpc(PlayerPrefs.GetString("PName", "Player"));
        }
    }
    
    [ServerRpc]
    private void SubmitNameServerRpc(string name)
    {
        PlayerName.Value = name;    
    }

    void OnNameChanged(FixedString64Bytes old, FixedString64Bytes newValue)
    {
        NameTag.text = newValue.ToString();
    }

    public void TryGetCamera()
    {
        if(playerCam != null)
        {
            return;
        }
        try
        {
            playerCam = Camera.main.transform;
        }
        catch
        {
            Invoke(nameof(TryGetCamera), 1f);
        }
    }

    void Update()
    {
        if(!IsOwner && playerCam != null)
        {
            NameTag.transform.forward = NameTag.transform.position - playerCam.transform.position;
        }
    }

    public void InitLeave()
    {
        ScreenPromptParent.SetActive(true);
        PlayerUIManager.instance.HideUI();
    }

    public void CancelLeave()
    {
        ScreenPromptParent.SetActive(false);
        PlayerUIManager.instance.ShowUI();
    }

    public void Leave()
    {
        NetworkManager.Singleton.Shutdown();
    }

    IEnumerator GetPublicIP()
    {
        using UnityWebRequest request = UnityWebRequest.Get("https://api.ipify.org");
        {
            DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
        }

        yield return request.SendWebRequest();

        while (request.result != UnityWebRequest.Result.Success)
        {
            yield return request.SendWebRequest();
        }

        publicIP = request.downloadHandler.text;

        ShowHideIP();
        ShowHideIP();
    }

    public void ShowHideIP()
    {
        IPHidden = !IPHidden;

        if(IPHidden)
        {
            ShowHideText.text = "Show IP";
            IPText.text = "IP Address Hidden";
        }
        else
        {
            IPText.text = publicIP;
            ShowHideText.text = "Hide IP";
        }
    }
}
