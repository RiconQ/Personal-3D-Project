using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerCharacter _playerMovement;

    [Header("Grappling Rope")]
    [SerializeField] private int _quality;
    [SerializeField] private float _damper;
    [SerializeField] private float _strength;
    [SerializeField] private float _velocity;
    [SerializeField] private float _waveCount;
    [SerializeField] private float _waveHeight;
    [SerializeField] private AnimationCurve _affectCurve;

    private LineRenderer _lr;
    private Vector3 _currentGrapplePosition;
    private Spring _spring;


    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _spring = new Spring();
        _spring.SetTarget(0);
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void DrawRope()
    {
        if (!_playerMovement.IsGrappling())
        {
            _currentGrapplePosition = _playerMovement.GetGunTip();
            _spring.Reset();
            if (_lr.positionCount > 0)
                _lr.positionCount = 0;
            return;
        }

        if (_lr.positionCount == 0)
        {
            _spring.SetVelocity(_velocity);
            _lr.positionCount = _quality + 1;
        }

        _spring.SetDamper(_damper);
        _spring.SetStrength(_strength);
        _spring.Update(Time.deltaTime);

        var grapplePoint = _playerMovement.GetGrapplePoint();
        var gunTipPosition = _playerMovement.GetGunTip();
        var up = Quaternion.LookRotation(grapplePoint - gunTipPosition).normalized * Vector3.up;

        _currentGrapplePosition = Vector3.Lerp(_currentGrapplePosition, _playerMovement.GetGrapplePoint(), Time.deltaTime);

        for (int i = 0; i < _quality; i++)
        {
            var delta = i / (float)_quality;
            var offset = up * _waveHeight * Mathf.Sin(delta * _waveCount * Mathf.PI * _spring.Value * _affectCurve.Evaluate(delta));

            _lr.SetPosition(i, Vector3.Lerp(gunTipPosition, _currentGrapplePosition, delta) + offset);
        }
    }
}
