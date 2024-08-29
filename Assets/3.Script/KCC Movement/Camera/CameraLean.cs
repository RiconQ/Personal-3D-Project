using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float _attackDamping = 0.5f;
    [SerializeField] private float _decayDamping = 0.3f;
    [SerializeField] private float _walkStrength = 0.075f;
    [SerializeField] private float _slideStrength = 0.2f;
    [SerializeField] private float _strengthResponse = 5f;

    private Vector3 _dampedAcceleration;
    private Vector3 _dampedAccelerationVel;
    private float _smoothStrength;
    public void Initialize()
    {
        _smoothStrength = _walkStrength;
    }

    public void UpdateLean(float deltaTime, bool sliding, Vector3 acceleration, Vector3 up)
    {
        var planarAccleration = Vector3.ProjectOnPlane(acceleration, up);
        var damping = planarAccleration.magnitude > _dampedAcceleration.magnitude
            ? _attackDamping : _decayDamping;

        _dampedAcceleration = Vector3.SmoothDamp
        (
            current: _dampedAcceleration,
            target: planarAccleration,
            currentVelocity: ref _dampedAccelerationVel,
            smoothTime: damping,
            maxSpeed: float.PositiveInfinity,
            deltaTime: deltaTime
        );

        // Get the rotation axis based on the acceleration Vector
        var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;

        // Reset the rotation to that of its parent
        transform.localRotation = Quaternion.identity;

        // Rotate around the lean axis
        var targetStrength = sliding ? _slideStrength : _walkStrength;

        _smoothStrength = Mathf.Lerp(_smoothStrength, targetStrength, 1f - Mathf.Exp(-_strengthResponse * deltaTime));
        transform.rotation = Quaternion.AngleAxis(_dampedAcceleration.magnitude * _smoothStrength, leanAxis) * transform.rotation;
    }
}
