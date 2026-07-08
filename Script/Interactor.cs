using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class Interactor : NetworkBehaviour
{
    public PlayerInput playerInput;
    InputAction interactAction;
    InputAction altInteractAction;

    Interactable selected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {       
        interactAction = playerInput.actions.FindAction("Interact");
        altInteractAction = playerInput.actions.FindAction("AltInteract");
        interactAction.performed += Interact;
        altInteractAction.performed += AltInteract;
    }

    void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        if(Physics.Raycast(ray, out hit))
        {
            if(Interactable.Interactables.ContainsKey(hit.collider))
            {
                selected = Interactable.Interactables[hit.collider];
            }
            else
            {
                selected = null;
            }
        }
    }

    void Interact(InputAction.CallbackContext ctx)
    {
        if(selected == null)
        {
            return;
        }

        selected.OnInteract();
    }

    void AltInteract(InputAction.CallbackContext ctx)
    { 
        if(selected == null)
        {
            return;
        }

        selected.OnInteract();
    }
}
