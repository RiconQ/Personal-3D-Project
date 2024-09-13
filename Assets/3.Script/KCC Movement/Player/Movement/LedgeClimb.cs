using System.Collections;
using UnityEngine;

public class LedgeClimb : MonoBehaviour
{
    private PlayerCharacter _pm;

    [Header("Ledge Climb")]
    [SerializeField] private LayerMask _ledgeLayer;
    [SerializeField] private float _climbDuration;
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private float _ledgeDetectionDistance = 2f;

    private bool _isInLedge = false;
    public bool IsInLedge => _isInLedge;
    private bool _isLedgeClimbing = false;
    public bool IsLedgeClimbing => _isLedgeClimbing;
    private RaycastHit _ledgeHit;
    public void Initialize()
    {
        _pm = GetComponent<PlayerCharacter>();
    }

    public void UpdateLedgeClimb()
    {
        CheckLedge();
    }

    private void CheckLedge()
    {
        var radius = _pm.Motor.Capsule.radius;
        var height = _pm.Motor.Capsule.height;
        
        if(Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out _ledgeHit, _ledgeDetectionDistance, _ledgeLayer))
        {
            Debug.Log("Ledge Found");
            _isInLedge = true;
        }
        else
        {
            _isInLedge = false;
        }
    }

    public void StartLedgeClimb()
    {
        if (Physics.Raycast(
            _ledgeHit.point +
            (_playerCamera.forward * _pm.Motor.Capsule.radius) +
            (Vector3.up * 0.6f * _pm.Motor.Capsule.height),
            Vector3.down, out var groundHit, _pm.Motor.Capsule.height))
        {
            StartCoroutine(LedgeClimb_co(groundHit.point));
        }
    }

    private IEnumerator LedgeClimb_co(Vector3 targetPosition)
    {
        _isLedgeClimbing = true;
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < _climbDuration)
        {
            _pm.SetPosition(Vector3.Lerp(startPosition, targetPosition, time / _climbDuration));
            time += Time.deltaTime;
            yield return null;
        }

        _pm.SetPosition(targetPosition);
        _isLedgeClimbing = false;
    }
}
