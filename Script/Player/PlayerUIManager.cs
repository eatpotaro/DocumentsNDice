using Unity.Netcode;
using UnityEngine;

public class PlayerUIManager : NetworkBehaviour
{
    public static PlayerUIManager instance;
    public CameraController camControl;
    
    public GameObject GeneralUIParent;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            instance = this;
        }
    }
    
    public void HideUI()
    {
        GeneralUIParent.SetActive(false);
        camControl.SetMouseLockState(false);
    }

    public void ShowUI()
    {
        GeneralUIParent.SetActive(true);
        camControl.SetMouseLockState(true);
    }    
}
