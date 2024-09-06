using UnityEngine;

public class CrouchSlam : MonoBehaviour
{
    [Header("Crouch Slam")]
    [SerializeField] private float _crouchForce = 10f;

    private PlayerCharacter _pm;

    private bool _isCrouchSlam = false;
    public bool IsCrouchSlam => _isCrouchSlam;

    public void Initialize(PlayerCharacter pm)
    {
        _pm = pm;
    }

    public void StartCrouchSlam()
    {
        _isCrouchSlam = true;
    }

    public void CrouchSlamMovement(ref Vector3 currentVelocity)
    {
        currentVelocity += -_pm.Motor.CharacterUp * _crouchForce;
    }

    public void StopCrouchSlam()
    {
        _isCrouchSlam = false;
    }
}
