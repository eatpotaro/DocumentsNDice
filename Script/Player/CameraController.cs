using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [Header("Camera Movement")]
    public PlayerInput playerInput;
    InputAction moveAction;
    public Rigidbody rb;
    Vector3 moveValue;
    public float speed;
    
    [Header("Mouse Locked")]
    bool mouseLocked = true;
    InputAction escapeAction;


 
    [Header("Camera Rotation")]
    InputAction lookAction;
    Vector2 lookValue;
    private float rotX, rotY;

    public float mouseSensitivity;
    public Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //sets camera to be locked in center and invisible
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //assigns correct inputs
        moveAction = playerInput.actions.FindAction("Move");
        lookAction = playerInput.actions.FindAction("Look");
        escapeAction = playerInput.actions.FindAction("Escape");

        //change mouse locked if escape action is performed
        escapeAction.performed += ChangeMouseLocked;
    }

    // Update is called once per frame
    void Update()
    {
        //if not owner dont run
        if(!IsOwner)
        {
            return;
        }

        //if mouse is not locked then dont move camera
        if(!mouseLocked)
        {
            return;
        }

        //reads values of move and look
        moveValue = moveAction.ReadValue<Vector3>();
        lookValue = lookAction.ReadValue<Vector2>();

        //calculate the target rotation (x is up/down, y = left/right)
        rotX += -lookValue.y * mouseSensitivity;
        rotY += lookValue.x * mouseSensitivity;
        
        //clamp rotation to be just under 90 degrees up and down
        rotX = Mathf.Clamp(rotX, -89f, 89f);
        rotY %= 360;
    
        //set the new rotation
        rb.MoveRotation(Quaternion.Euler(rotX, rotY, 0f));
    }

    void FixedUpdate()
    {
        if(!IsOwner || !IsSpawned)
        {
            return;
        }

        //calculate the move vector and add the force
        Vector3 moveVector = transform.forward * moveValue.z + Vector3.up * moveValue.y + transform.right * moveValue.x;
        rb.AddForce(moveVector * speed);
    }

    void ChangeMouseLocked(InputAction.CallbackContext ctx)
    {
        //change mouse locked state
        mouseLocked = !mouseLocked;
        if(mouseLocked)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void SetMouseLockState(bool isLocked)
    {
        mouseLocked = !isLocked;
        if(isLocked)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
