using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DisableOnNetworkSpawn : NetworkBehaviour
{
    public List<GameObject> gameObjectsToDisable;
    public List<Behaviour> behavioursToDisable;
    //on spawned by server
    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            foreach(Behaviour behaviour in behavioursToDisable)
            {
                behaviour.enabled = false;
            }
            foreach(GameObject obj in gameObjectsToDisable)
            {
                obj.SetActive(false);
            }
        }
    }
}
