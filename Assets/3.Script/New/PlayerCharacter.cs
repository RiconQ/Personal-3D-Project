using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor _motor;
    [SerializeField] private Transform _cameraTarget;
    public void Initialize()
    {
        _motor.CharacterController = this;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
    }
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
    }


    public void BeforeCharacterUpdate(float deltaTime)
    {
    }
    public void PostGroundingUpdate(float deltaTime)
    {
    }
    public void AfterCharacterUpdate(float deltaTime)
    {
    }


    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }
    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public Transform GetCameraTarget()
    {
        return _cameraTarget;
    }
}
