using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class K_SwordController : K_WeaponController
{
    private int _slashIndex = 0;
    private bool _isAttacking = false;
    private PlayerCharacter _pm;
    private bool _isAirStrike = false;
    public bool IsAirStrike => _isAirStrike;

    [Header("Reference")]
    [SerializeField] private Transform _cameraTrans;

    [Header("Normal Attack Raycast")]
    [SerializeField] private float _normalRayDistance = 3f;
    [SerializeField] private int _normalNumberOfRays = 40;
    [SerializeField] private float _normalConeAngle = 80f;
    [SerializeField] private LayerMask _attackable;

    [Header("Falling Attack Raycast")]
    [SerializeField] private float _circleRayDistance = 5f;
    [SerializeField] private int _numberOfRaycastPerCircle = 30;
    [SerializeField] private float _cylinderRadius = 5f;
    [SerializeField] private float _cylinderHeight = 10f;
    [SerializeField] private int _numberOfHeightSteps = 5;



    public override void Initialize()
    {
        base.Initialize();
        _pm = FindObjectOfType<PlayerCharacter>();
        this.gameObject.SetActive(false);
    }

    public override void UpdateController(float deltaTime)
    {
        base.UpdateController(deltaTime);
    }

    public override void LateUpdateController(float deltaTime)
    {
    }

    public override void UpdateInput(ControllerInput weaponInput, float deltaTime)
    {
        _requestedInput.Kick = weaponInput.Kick;
        _requestedInput.KickReleased = weaponInput.KickReleased;
        _requestedInput.LeftMouse = weaponInput.LeftMouse;
        _requestedInput.LeftMouseReleased = weaponInput.LeftMouseReleased;
        _requestedInput.RightMouse = weaponInput.RightMouse;
        _requestedInput.RightMouseReleased = weaponInput.RightMouseReleased;
        _requestedInput.Crouch = weaponInput.Crouch;
        if (!K_WeaponHolder.instance.isCharging && !K_KickController.instance.kickCharging)
        {
            if (_requestedInput.RightMouse)
            {
                _lastInput = EInput.RightMouse;
                _animator.SetInteger("AttackIndex", 3);
                _animator.SetTrigger("Charge");
                Charge();
            }
            else if (_requestedInput.LeftMouse && !_isAttacking && _pm.Motor.GroundingStatus.IsStableOnGround)
            {
                _isAttacking = true;
                _lastInput = EInput.LeftMouse;
                _slashIndex += 1;
                _slashIndex %= 2;
                _animator.SetInteger("AttackIndex", _slashIndex);
                _animator.SetTrigger("Charge");
                Charge();
            }
            else if (_requestedInput.LeftMouse && !_isAttacking && !_pm.Motor.GroundingStatus.IsStableOnGround)
            {
                _isAttacking = true;
                _isAirStrike = true;
                _lastInput = EInput.LeftMouse;

                _animator.SetInteger("AttackIndex", 5);
                _animator.SetTrigger("Charge");
                Charge();
            }
        }
        else if (K_WeaponHolder.instance.isCharging && !K_KickController.instance.kickCharging)
        {
            if (_requestedInput.RightMouseReleased && _lastInput == EInput.RightMouse)
            {
                _lastInput = EInput.RightMouseReleased;
                _animator.SetTrigger("Release");
                Release();
            }
            else if (_requestedInput.LeftMouseReleased && _lastInput == EInput.LeftMouse && !_isAirStrike)
            {
                _lastInput = EInput.LeftMouseReleased;
                _animator.SetTrigger("Release");
                Release();
            }
        }
    }

    private void OnEnable()
    {
        PlayAnimation("PickUp");
        ResetVar();
        _slashIndex = 0;
    }

    public override void DropWeapon()
    {
        K_WeaponHolder.instance.DropWeapon(0);
    }

    public override void Charge()
    {
        K_WeaponHolder.instance.isCharging = true;
    }

    public override void Release()
    {
        K_WeaponHolder.instance.isCharging = false;
    }

    public override void ResetVar()
    {
        _isAttacking = false;
    }
    public void AirStrike()
    {
        if (_pm.Motor.GroundingStatus.IsStableOnGround && _isAirStrike)
        {
            _isAirStrike = false;
            _lastInput = EInput.None;
            _animator.SetTrigger("Release");
            Release();
        }
    }

    public void AttackRaycast()
    {
        //Debug.Log("Try To Attack");
        Vector3 forwardDirection = _cameraTrans.forward;

        Vector3 origin = transform.position;

        float angleStep = _normalConeAngle / (_normalNumberOfRays - 1);

        float startAngle = -_normalConeAngle / 2;

        for (int i = 0; i < _normalNumberOfRays; i++)
        {
            float currentAngle = startAngle + (i * angleStep);

            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 rayDirection = rotation * forwardDirection;

            Ray ray = new Ray(origin, rayDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _normalRayDistance, _attackable))
            {
                Debug.DrawRay(origin, rayDirection * _normalRayDistance, Color.red, 1f);
                //Debug.Log("Attack");
            }
            else
            {
                //Debug.Log("False Attack");
                Debug.DrawRay(origin, rayDirection * _normalRayDistance, Color.green, 1f);
            }    
        }
    }
    public void FallingAttackRaycast()
    {
        CastCylinderRays();
    }

    private void CastCylinderRays()
    {
        Vector3 origin = transform.position;

        float heightStep = _cylinderHeight / (_numberOfHeightSteps - 1);

        for (int h = 0; h < _numberOfHeightSteps; h++)
        {
            float currentHeight = origin.y - (_cylinderHeight / 2) + (h * heightStep);
            Vector3 heightPosition = new Vector3(origin.x, currentHeight, origin.z);

            CastCircleRays(heightPosition);
        }
    }

    private void CastCircleRays(Vector3 center)
    {
        float angleStep = 360f / _numberOfRaycastPerCircle;

        for (int i = 0; i < _numberOfRaycastPerCircle; i++)
        {
            float currentAngle = i * angleStep;

            float radians = currentAngle * Mathf.Deg2Rad;
            Vector3 rayDirection = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians));

            rayDirection *= _cylinderRadius;
            Ray ray = new Ray(center, rayDirection.normalized);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _circleRayDistance, _attackable))
            {
                Debug.DrawRay(center, rayDirection.normalized * _circleRayDistance, Color.red, 1f);
                //Debug.Log("Hit");
            }
            else
            {
                Debug.DrawRay(center, rayDirection.normalized * _circleRayDistance, Color.green, 1f);
            }
        }
    }
}
