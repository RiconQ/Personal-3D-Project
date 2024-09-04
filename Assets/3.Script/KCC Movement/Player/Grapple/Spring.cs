using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class Spring
{
    private float _strength;
    private float _damper;
    private float _target;
    private float _velocity;
    private float _value;

    public void Update(float deltaTime)
    {
        var direction = _target - _value >= 0 ? 1f : -1f;
        var force = Mathf.Abs(_target - _value) * _strength;

        _velocity += (force * direction - _velocity * _damper) * deltaTime;
        _value += _velocity * deltaTime;
    }

    public void Reset()
    {
        _velocity = 0f;
        _value = 0f;
    }

    public void SetStrength(float value)
    {
        _strength = value;
    }

    public void SetDamper(float value)
    {
        _damper = value;
    }

    public void SetTarget(float value)
    {
        _target = value;
    }

    public void SetVelocity(float value)
    {
        _velocity += value;
    }

    public float Value => _value;
}
