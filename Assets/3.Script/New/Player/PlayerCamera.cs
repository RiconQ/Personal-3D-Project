using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}

public class PlayerCamera : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    [SerializeField] private float _sensitivity = 0.1f;

    private Vector3 _eulerAngles;
    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngles = transform.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        _eulerAngles += new Vector3(-input.Look.y, input.Look.x) * _sensitivity;
        //debug
        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -89, 89);
        
        transform.eulerAngles = _eulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}
