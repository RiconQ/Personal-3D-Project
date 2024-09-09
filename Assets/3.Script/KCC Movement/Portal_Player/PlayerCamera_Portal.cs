using UnityEngine;
using DG.Tweening;

public class PlayerCamera_Portal : MonoBehaviour
{
    private Camera _mainCamera;

    [SerializeField] private Player_Portal _player;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float _sensitivity = 0.1f;

    private Vector3 _eulerAngles;
    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngles = transform.eulerAngles;

        _mainCamera = Camera.main;
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

    public void DoFov(float endValue)
    {
        _mainCamera.DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        _mainCamera.transform.DOLocalRotate(new Vector3(transform.rotation.x, transform.rotation.y, zTilt), 0.25f);
    }
}
