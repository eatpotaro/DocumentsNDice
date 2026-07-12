using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance;
    public CameraController camControl;
    
    public GameObject GeneralUIParent;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Should only be one instance of playerUIManager. deleting");
            Destroy(this);
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
