using UnityEngine;

public class WallCilmb : MonoBehaviour
{
    [Header("Wall Detection")]
    [SerializeField] private LayerMask _whatIsWall;
    [SerializeField] private float _wallClimbDetectionDistance = 1f;
    [SerializeField] private float _maxWallLookAngle = 30f;
    [SerializeField] private float _minWallNormalAngleChange = 5f;

    [Header("Wall Climb")]
    [SerializeField] private float _climbSpeed = 10f;
    [SerializeField] private float _maxClimbTime = 2.5f;

    [Header("Wall Climb Jump")]
    [SerializeField] private float _jumpUpForce = 10f;
    [SerializeField] private float _jumpBackForce = 10f;

    //Reference
    private PlayerCharacter _pm;

    //Wall Detection
    private float _wallLookAngle;

    private GameObject _lastWall;
    private Vector3 _lastWallNormal;

    private RaycastHit _frontWalHit;
    private bool _isWallFront = false;

    //Wall Climb
    private float _climbTimer;
    private bool _isClimbing = false;
    public bool IsClimbing => _isClimbing;


    public void Initialize(PlayerCharacter pm)
    {
        _pm = pm;
    }

    public void UpdateWallClimb(float deltaTime)
    {
        WallClimbCheck();
        WallClimbState(deltaTime);
    }

    private void WallClimbState(float deltaTime)
    {
        if ((_isWallFront && (_pm.MoveInput.y > 0)) && _wallLookAngle < _maxWallLookAngle && !_pm.isExitingWall)
        {
            if (!_isClimbing && _climbTimer > 0)
                StartClimbing();

            if (_climbTimer > 0)
                _climbTimer -= deltaTime;

            if (_climbTimer < 0)
                StopClimbing();
        }
        else if (_pm.isExitingWall)
        {
            if (_isClimbing)
                StopClimbing();

            if (_pm.exitWallTimer > 0)
                _pm.exitWallTimer -= deltaTime;
            if (_pm.exitWallTimer < 0)
                _pm.isExitingWall = false;
        }
        else
        {
            if (_isClimbing)
            {
                StopClimbing();
            }
        }
    }

    private void WallClimbCheck()
    {
        _isWallFront = Physics.Raycast(transform.position, transform.forward, out _frontWalHit, _wallClimbDetectionDistance, _whatIsWall);
        _wallLookAngle = Vector3.Angle(transform.forward, -_frontWalHit.normal);

        bool isNewWall = _frontWalHit.transform != _lastWall ||
            Mathf.Abs(Vector3.Angle(_lastWallNormal, _frontWalHit.normal)) > _minWallNormalAngleChange;

        if ((_isWallFront && isNewWall) || !_pm.AboveGound())
        {
            _climbTimer = _maxClimbTime;
        }
    }

    private void StartClimbing()
    {
        //Reset JumpCount
        _pm.ResetJumpCount();

        _isClimbing = true;

        _lastWall = _frontWalHit.transform.gameObject;
        _lastWallNormal = _frontWalHit.normal;
    }

    private void StopClimbing()
    {
        _isClimbing = false;
    }

    public void ClimbingMovement(ref Vector3 currentVelocity)
    {
        currentVelocity = new Vector3(0, _climbSpeed, _pm.Motor.Velocity.z);
    }

    public void ClimbJump(ref Vector3 currentVelocity)
    {
        _pm.isExitingWall = true;
        _pm.exitWallTimer = _pm.ExitWallTime;

        Vector3 forceToApply =
            transform.up * _jumpUpForce +
            _frontWalHit.normal * _jumpBackForce;

        currentVelocity = new Vector3(_pm.Motor.Velocity.x, 0f, _pm.Motor.Velocity.z);

        // Set Minimum Vertical Speed to the Jump Speed
        var currentVerticalSpeed = Vector3.Dot(currentVelocity, _pm.Motor.CharacterUp);
        var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _jumpUpForce);

        // Add the difference in current and target vertical speed to the character's velocity
        currentVelocity += forceToApply * (targetVerticalSpeed - currentVerticalSpeed);
    }
}
