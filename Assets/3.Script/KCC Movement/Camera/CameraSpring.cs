using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    [Min(0.01f)]
    [SerializeField] private float _halfLife = 0.075f;
    [Space]
    [SerializeField] private float _frequency = 18f;
    [Space]
    [SerializeField] private float _angularDisplacement = 2f;
    [SerializeField] private float _linearDisplacement = 0.05f;

    private Vector3 _springPosition;
    private Vector3 _springVelocity;

    private PlayerCharacter _pm;
    public void Initialize(PlayerCharacter pm)
    {
        _pm = pm;
        _springPosition = transform.position;
        _springVelocity = Vector3.zero;
    }

    public void UpdateSpring(float deltaTime, Vector3 up)
    {
        transform.localPosition = Vector3.zero;

        Spring(ref _springPosition, ref _springVelocity, transform.position, _halfLife, _frequency, deltaTime);

        var localSpringPosition = _springPosition - transform.position;
        var springHeight = Vector3.Dot(localSpringPosition, up);
        if (_pm.IsDashing())
        {
            transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            Debug.Log("asdas");
        }
        else
            transform.localEulerAngles = new Vector3(-springHeight * _angularDisplacement, 0f, 0f);
        transform.localPosition = localSpringPosition * _linearDisplacement;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, _springPosition);
        Gizmos.DrawSphere(_springPosition, 0.1f);
    }

    private static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halfLife, float frequency, float timeStep)
    {
        var dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
        var f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
        var oo = frequency * frequency;
        var hoo = timeStep * oo;
        var hhoo = timeStep * hoo;
        var detInv = 1.0f / (f + hhoo);
        var detX = f * current + timeStep * velocity + hhoo * target;
        var detV = velocity + hoo * (target - current);
        current = detX * detInv;
        velocity = detV * detInv;
    }
}
