using UnityEngine;

public class Jump : MonoBehaviour
{
    private PlayerCharacter _pm;

    [Header("Jump")]
    [SerializeField] private float _jumpSpeed = 30f;
    [SerializeField] private float _coyoteTime = 0.2f;
    public float CoyoteTime => _coyoteTime;

    [HideInInspector] public float timeSinceUngrounded;
    [HideInInspector] public float timeSinceJumpRequest;
    [HideInInspector] public bool ungroundedDueToJump;

    /*
    [Header("ObjectJump")]
    [SerializeField] private float _objectJumpSpeed = 40f;
    [SerializeField] private float _objectTime = 0.2f;

    private bool _isTicking = false;
    private float _objectTimer;
    */
    public void Initialize()
    {
        _pm = GetComponent<PlayerCharacter>();
        //_objectTimer = _objectTime;
    }
    /*
    public void UpdateJump(float deltaTime)
    {
        if(_objectTimer <= _objectTime)
        {
            _isTicking = true;
            _objectTimer += deltaTime;
        }
        else
        {
            _isTicking = false;
        }
    }
    */

    public void JumpMovement(ref Vector3 currentVelocity)
    {
        // Set Minimum Vertical Speed to the Jump Speed
        var currentVerticalSpeed = Vector3.Dot(currentVelocity, _pm.Motor.CharacterUp);
        var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _jumpSpeed);

        // Add the difference in current and target vertical speed to the character's velocity
        currentVelocity += _pm.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
    }
    
    /*
    public void CheckObject(Collider hitCollider, Vector3 hitNormal)
    {
        
    }

    public void ObjectJump(ref Vector3 currentVelocity)
    {
        // Set Minimum Vertical Speed to the Jump Speed
        var currentVerticalSpeed = Vector3.Dot(currentVelocity, _pm.Motor.CharacterUp);
        var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _objectJumpSpeed);

        // Add the difference in current and target vertical speed to the character's velocity
        currentVelocity += _pm.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
    }
    */
}
