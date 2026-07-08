using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections;

public class PlayerCreator : MonoBehaviour
{
    public TMP_InputField IpField;
    public TMP_InputField HostPortField;
    public TMP_InputField ClientPortField;
    private TMP_InputField portField;
    private ushort portNum;
    public TMP_InputField NameField;
    public Camera MenuCam;
    private UnityTransport networkManagerTransport;
    public string GameSceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManagerTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        SetPortField(true);
        ValidateIpField();
        UpdatePort();
    }
    public void SetPortField(bool isHost)
    {
        if(isHost)
        {
            portField = HostPortField;
        }
        else
        {
            portField = ClientPortField;
        }
        
        UpdatePort();
    }

    public void ValidateIpField()
    {
        string startText = IpField.text;
        string[] parts = startText.Split(new char[]{'.'});
        int[] finalAddress = new int[4];
        bool valid = true;

        for(int i = 0; i < parts.Length; i++)
        {
            int newpart = 0;
            bool success = int.TryParse(parts[i], out newpart);

            if(!success)
            {
                finalAddress[i] = 0;
                valid = false;
                continue;
            }

            if(newpart < 0 || newpart > 255)
            {
                finalAddress[i] = 0;  
                valid = false;
                continue;
            }

            finalAddress[i] = newpart;
        }

        if(valid)
        {
            IpField.text = $"{finalAddress[0]}.{finalAddress[1]}.{finalAddress[2]}.{finalAddress[3]}";
        }
        else
        {            
            IpField.text = $"127.0.0.1";
        }
    }

    public void UpdatePort()
    {
        bool success = ushort.TryParse(portField.text, out portNum);
        if(!success)
        {
            portField.text = "7776";
            portNum = 7776;
        }
    }
    public void StartHost()
    {
        networkManagerTransport.SetConnectionData("0.0.0.0", portNum);

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    public void StartClient()
    {
        networkManagerTransport.SetConnectionData(IpField.text, portNum);

        NetworkManager.Singleton.StartClient();   
    }

    public void StopJoin()
    {
        NetworkManager.Singleton.Shutdown();
    }
    public void SetPlayerName()
    {
        PlayerPrefs.SetString("PName", NameField.text);
        PlayerPrefs.Save();
    }
}
