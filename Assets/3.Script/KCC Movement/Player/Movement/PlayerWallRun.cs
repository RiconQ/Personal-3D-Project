using KinematicCharacterController;
using System;
using UnityEngine;

public class PlayerWallRun : MonoBehaviour, ICharacterController
{
    [Header("Reference")]
    [SerializeField] private PlayerCharacter _playerMovement;
    [SerializeField]private KinematicCharacterMotor _motor;

    [Header("WallRunning")]
    [SerializeField] private LayerMask _whatIsWall;
    [SerializeField] private LayerMask _whatIsGround;
    [Space]
    [SerializeField] private float _wallRunSpeed = 30f;

    [Header("Wall Detection")]
    [SerializeField] private float _wallCheckDistance;
    [SerializeField] private float _minJumpHeight;
    // Reference


    public void Initialize()
    {
    }

    public void UpdateInput(CharacterInput input)
    {
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
    }
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
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
        Debug.Log("CheckWallDectection");
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



}
