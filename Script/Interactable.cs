using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.Netcode;
using UnityEngine;

public class Interactable : NetworkBehaviour
{
    public static Dictionary<Collider, Interactable> Interactables = new Dictionary<Collider, Interactable>();
    public Collider thisCollider;

    void Awake()
    {
        if(thisCollider == null)
        {
            thisCollider = transform.GetComponent<Collider>();
        }

        if(Interactables.ContainsKey(thisCollider))
        {
            return;
        }

        Interactables.Add(thisCollider, this);
    }

    virtual public void OnInteract()
    {
        
    }

    virtual public void OnDelete()
    {
        
    }

    public override void OnDestroy()
    {
        Interactables.Remove(thisCollider);
    }
}
