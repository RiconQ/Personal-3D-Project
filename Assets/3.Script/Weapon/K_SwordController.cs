using UnityEngine;

public class K_SwordController : K_WeaponController
{
    private int _slashIndex = 0;
    private bool _isAttacking = false;
    private PlayerCharacter _pm;
    private bool _isAirStrike = false;
    public bool IsAirStrike => _isAirStrike;

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
            else if(_requestedInput.LeftMouse && !_isAttacking && !_pm.Motor.GroundingStatus.IsStableOnGround)
            {
                _isAttacking = true;
                _isAirStrike = true;
                _lastInput = EInput.LeftMouse;

                _animator.SetInteger("AttackIndex", 5);
                _animator.SetTrigger("Charge");
                Charge();
            }
        }
        else if(K_WeaponHolder.instance.isCharging && !K_KickController.instance.kickCharging)
        {
            if (_requestedInput.RightMouseReleased && _lastInput == EInput.RightMouse)
            {
                _lastInput = EInput.RightMouseReleased;
                _animator.SetTrigger("Release");
                Release();
            }
            else if(_requestedInput.LeftMouseReleased && _lastInput == EInput.LeftMouse && !_isAirStrike)
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
}
