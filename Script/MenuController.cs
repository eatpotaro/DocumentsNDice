using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class MenuController : MonoBehaviour
{
    private bool isHost;
    public GameObject[] MenuParents;
    private PlayerCreator playerCreator;
    public TMP_Text IPText;
    public TMP_Text ShowHideText;
    private bool IPHidden = false;
    private string publicIP;

    void Start()
    {
        playerCreator = GetComponent<PlayerCreator>();
        RefreshIP();
        ShowHideIP();
    }

    public void SetHost()
    {
        playerCreator.SetPortField(true);
        isHost = true;
    }

    public void SetClient()
    {
        playerCreator.SetPortField(false);
        isHost = false;
    }

    public void SetUI(int state)
    {
        for(int i = 0; i < MenuParents.Length; i++)
        {
            if(i == state)
            {
                MenuParents[i].SetActive(true);
            }
            else
            {
                MenuParents[i].SetActive(false);
            }
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void StartServer()
    {
        if(isHost)
        {
            playerCreator.StartHost();
        }
        else
        {
            playerCreator.StartClient();
        }
    }

    public void RefreshIP()
    {
        IPText.text = "Fetching IP...";
        StartCoroutine(nameof(GetPublicIP));
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
    }
}
