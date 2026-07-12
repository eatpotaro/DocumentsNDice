using UnityEngine;

public class LockToTransformOffset : MonoBehaviour
{
    
    public Transform target;
    public Vector3 offset;

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
    }
}
