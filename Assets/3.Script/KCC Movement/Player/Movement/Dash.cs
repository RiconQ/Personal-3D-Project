using UnityEngine;

public class Dash : MonoBehaviour
{
    public enum EDashState
    {
        Idle,
        Start,
        Dashing,
        End
    }
    private PlayerCharacter _pm;

    [Header("Dash")]
    [SerializeField] private float _speed = 30f;

    [SerializeField] private EDashState _state;
    private Vector3 _direction;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _timer;
    private Transform _targetTrans;

    private RaycastHit _hit;

    public bool canDash { get; private set; }
    public bool isDashing { get; private set; }
    public void Initialize()
    {
        _pm = GetComponent<PlayerCharacter>();
        canDash = true;
        isDashing = false;
        _state = EDashState.Idle;
    }

    public void DashUpdate()
    {

    }

    public void DashInit()
    {
        Debug.Log("DashInit");
        if (_state != EDashState.Idle || !canDash)
        {
            Debug.Log("_state != EDashState.Idle || !canDash");
            return;
        }
        _targetTrans = K_WeaponControl.instance.GetClosestTarget();
        if (_targetTrans == null || Player.instance.PlayerCharacter.currentState.Stance == EStance.Slide)
        {
            return;
        }
        Physics.Raycast(_targetTrans.position, Vector3.down, out _hit, (!Player.instance.PlayerCharacter.Motor.GroundingStatus.IsStableOnGround) ? 1 : 4);
        if (_hit.distance != 0f)
        {
            _targetPos = _hit.point + Vector3.up;
        }
        else
        {
            _targetPos = _targetTrans.position;
        }

        _state = EDashState.Start;
        canDash = false;
        isDashing = true;

    }

    public void DashUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (_state)
        {
            case EDashState.Start:
                if (_pm.Motor.GroundingStatus.IsStableOnGround)
                {
                    _pm.Motor.ForceUnground(0f);
                }
                //Drop Current Weapon
                if(K_WeaponHolder.instance.HasWaepon())
                {
                    K_WeaponHolder.instance.DropCurrentWeapon();
                }
                _startPos = _pm.transform.position;
                _direction = _startPos.DirTo(_targetPos);
                _speed = (8f - Vector3.Distance(_startPos, _targetPos) / 2f).Abs();
                _speed = Mathf.Clamp(_speed, 2f, 12f);
                _timer = 0f;
                _state = EDashState.Dashing;
                Debug.Log("EDashState.Dashing");
                break;
            case EDashState.Dashing:
                _timer = Mathf.MoveTowards(_timer, 1f, deltaTime * _speed);
                _pm.SetPosition(Vector3.Lerp(_startPos, _targetPos, _timer - 0.2f));
                if (_timer == 1f)
                {
                    _state = EDashState.End;
                }
                break;
            case EDashState.End:

                canDash = true;
                currentVelocity += _direction * 25f;

                _targetTrans.GetComponentInChildren<K_Dashable>().Dash();

                isDashing = false;
                _state = EDashState.Idle;
                break;
        }

    }
}