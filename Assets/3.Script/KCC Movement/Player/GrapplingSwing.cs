using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GrapplingSwing : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerCharacter _playerMovement;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _gunTip;
    [SerializeField] private LineRenderer _lr;

    [Header("Grappling")]
    [SerializeField] private LayerMask _whatIsGrapple;
    [SerializeField] private float _maxGrappleDistance = 50f;
    [SerializeField] private float _swingDelayTime = 0.25f;
    [SerializeField] private float _swingCooldownTime = 0f;
    [SerializeField] private float _swingForce = 10f;

    private Vector3 _swingPoint;
    private float _swingCooldownTimer;
    private Vector3 _initialVelocity;
    public bool isSwinging;
    public bool isGrappling;

    private void LateUpdate()
    {
        if (isGrappling)
            _lr.SetPosition(0, _gunTip.position);
    }

    public void StartGrapple()
    {
        if (_swingCooldownTimer > 0) return;

        isGrappling = true;

        RaycastHit hit;
        if (Physics.Raycast(
            _cameraTransform.position,
            _cameraTransform.forward,
            out hit, _maxGrappleDistance,
            _whatIsGrapple
            ))
        {
            Debug.Log("Start Grapple");
            //Grapple Hit
            _swingPoint = hit.point;

            _initialVelocity = _playerMovement.transform.GetComponent<KinematicCharacterMotor>().Velocity;

            Invoke(nameof(ExecuteGrapple), _swingDelayTime);

            _lr.enabled = true;
            _lr.SetPosition(1, _swingPoint);
        }

    }

    public void ExecuteGrapple()
    {
        isSwinging = true;
        Debug.Log("Execute Grapple");
    }

    public Vector3 GetSwingVelocity()
    {
        Vector3 directionToSwingPoint = _swingPoint - _playerMovement.transform.position;
        directionToSwingPoint.Normalize();

        Vector3 tangentVelocity = Vector3.Cross(directionToSwingPoint, _initialVelocity);
        tangentVelocity = Vector3.Cross(tangentVelocity, directionToSwingPoint).normalized * _initialVelocity.magnitude;

        Vector3 velocityToApply = tangentVelocity;
        velocityToApply += directionToSwingPoint * _swingForce;

        return velocityToApply;
    }

    public void StopGrapple()
    {
        Debug.Log("Stop Grapple");
        isGrappling = false;
        isSwinging = false;

        _swingCooldownTimer = _swingCooldownTime;
        _lr.enabled = false;
    }
}
