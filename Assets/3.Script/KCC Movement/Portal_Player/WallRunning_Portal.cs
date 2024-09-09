using UnityEngine;

public class WallRunning_Portal : MonoBehaviour
{
    [Header("Wall Run - Detection")]
    [SerializeField] private LayerMask _whatIsWall;
    [SerializeField] private float _wallDetectionDistance = 1f;

    [Header("Wall Run")]
    [SerializeField] private float _wallRunSpeed = 40f;
    [SerializeField] private float _cameraZTilt = 7.5f;
    [SerializeField] private bool _useWallCounterForce = true;
    [SerializeField] private float _wallCounterForce = 20f;
    [Range(0, 1)]
    [SerializeField] private float _blendFactor = 0.5f;

    [Header("Wall Jump")]
    [SerializeField] private float _wallJumpForce = 10f;
    [SerializeField] private float _wallSideJumpForce = 20f;

    [Header("Camera")]
    [SerializeField] private PlayerCamera_Portal _playerCamera;

    //Wall Detection
    private RaycastHit _rightWallHit;
    private RaycastHit _leftWallHit;
    private bool _isWallRight = false;
    private bool _isWallLeft = false;

    //Wall Run & Jump
    private bool _isWallRunning = false;
    public bool IsWallRunning => _isWallRunning;

    //PlayerCharacter
    private PlayerCharacter_Portal _pm; //Player Movement

    public void Initialize(PlayerCharacter_Portal playerCharacter)
    {
        _pm = playerCharacter;
    }

    public void UpdateWallRun(float deltaTime)
    {
        WallCheck();
        WallRunState(deltaTime);
    }

    private void WallRunState(float deltaTime)
    {
        if ((_isWallLeft || _isWallRight) && _pm.MoveInput.y > 0 && _pm.AboveGound() && !_pm.isExitingWall)
        {
            if (!_isWallRunning && _pm.CurrentState.Stance == EStance.Stand)
            {
                StartWallRun();
            }
        }
        else if (_pm.isExitingWall)
        {
            if (_isWallRunning)
                StopWallRun();

            if (_pm.exitWallTimer > 0)
                _pm.exitWallTimer -= deltaTime;

            if (_pm.exitWallTimer <= 0)
                _pm.isExitingWall = false;
        }
        else
        {
            if (_isWallRunning)
                StopWallRun();
        }
    }

    private void WallCheck()
    {
        _isWallRight = Physics.Raycast(transform.position, transform.right, out _rightWallHit, _wallDetectionDistance, _whatIsWall);
        _isWallLeft = Physics.Raycast(transform.position, -transform.right, out _leftWallHit, _wallDetectionDistance, _whatIsWall);
    }

    public void WallRunningMovement(ref Vector3 currentVelocity)
    {
        var wallNormal = _isWallRight ? _rightWallHit.normal : _leftWallHit.normal;
        var wallForward = Vector3.Cross(wallNormal, transform.up);

        // Modify Wall Forward to correct direction
        {
            if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
            {
                wallForward = -wallForward;
            }
        }

        // Blend Wall run speed And player current speed
        {
            Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

            currentVelocity = Vector3.Lerp(horizontalVelocity, wallForward * _wallRunSpeed, _blendFactor);
        }

        //Add Velocity for stick on wall
        {
            if (_useWallCounterForce)
            {
                if (!(_isWallLeft && _pm.MoveInput.x > 0) && !(_isWallRight && _pm.MoveInput.x < 0))
                {
                    currentVelocity += -wallNormal * _wallCounterForce;
                }
            }
        }

        currentVelocity.y = 0;
    }
    public void WallJump(ref Vector3 currentVelocity)
    {
        _pm.isExitingWall = true;
        _pm.exitWallTimer = _pm.ExitWallTime;

        Vector3 wallNormal = _isWallRight ? _rightWallHit.normal : _leftWallHit.normal;
        Vector3 forceToApply = _pm.Motor.CharacterUp * _wallJumpForce + wallNormal * _wallSideJumpForce;

        //Set Minumum Vertical Speed to th Jump Speed
        var currentVerticalSpeed = Vector3.Dot(currentVelocity, _pm.Motor.CharacterUp);
        var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _wallJumpForce);

        //Add the difference in current and target vertical speed to the character's velocity
        currentVelocity += forceToApply * (targetVerticalSpeed - currentVerticalSpeed);
    }
    private void StartWallRun()
    {
        _isWallRunning = true;

        //Change Fov
        _playerCamera.DoFov(100f);
        //Do Tilt
        if (_isWallLeft)
            _playerCamera.DoTilt(-_cameraZTilt);
        else if (_isWallRight)
            _playerCamera.DoTilt(_cameraZTilt);
    }

    private void StopWallRun()
    {
        _isWallRunning = false;

        //Change Fov
        _playerCamera.DoFov(80f);
        //Do Tilt
        _playerCamera.DoTilt(0f);
    }
}
