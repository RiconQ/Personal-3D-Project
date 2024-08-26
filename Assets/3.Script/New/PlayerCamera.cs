using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
