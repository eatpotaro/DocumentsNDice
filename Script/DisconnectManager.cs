using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

public class DisconnectManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Singleton.OnConnectionEvent += OnDisconnect;
    }

    void OnDisconnect(NetworkManager manager, ConnectionEventData data)
    {
        if(data.EventType != ConnectionEvent.ClientDisconnected)
        {
            //makes sure the event is either when the current client disconnects or the main server disconnects
            return;
        }

        if (data.ClientId == NetworkManager.ServerClientId)
        {
            manager.Shutdown();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene("Lobby");
    }
}