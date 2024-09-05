using UnityEngine;

public class Dash : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform _playerCamera;
    private PlayerCharacter _pm;

    [Header("Dash")]
    [SerializeField] private float _dashDistance = 20f;
    [SerializeField] private float _dashSpeed = 150f;
    [SerializeField] private float _dashCooldownTime = 0.4f;

    private Vector3 _startPosition;
    private Vector3 _dashDirection;
    private bool _isDashing = false;
    public bool IsDashing => _isDashing;
    private float _dashCooldownTimer;
    private bool _mustStop = false;

    public void Initialize(PlayerCharacter pm)
    {
        _pm = pm;
    }

    public void UpdateDash(float deltaTime)
    {
        if (_dashCooldownTimer >= 0)
            _dashCooldownTimer -= deltaTime;
    }

    public void StartDash()
    {
        if (_dashCooldownTimer <= 0)
        {
            _startPosition = transform.position;
            _dashDirection = _playerCamera.forward;
            _isDashing = true;
        }
    }

    public void DashMovement(ref Vector3 currentVelocity)
    {
        var distance = Vector3.Distance(_startPosition, transform.position);

        if (distance < _dashDistance)
        {
            currentVelocity = _dashDirection.normalized * _dashSpeed;
        }
        else if (distance >= _dashDistance || _mustStop)
        {
            currentVelocity = Vector3.zero;
            StopDash();
        }
    }

    private void StopDash()
    {
        _isDashing = false;
        _mustStop = false;
        _dashCooldownTimer = _dashCooldownTime;
    }

    public void CheckWall()
    {
        if(Physics.Raycast(_playerCamera.position, _dashDirection, 1f))
        {
            _mustStop = true;
            StopDash();
        }
    }
}
