using UnityEngine;

public class K_KickController : K_WeaponController
{
    private K_WeaponHolder _weaponHolder;
    public bool isCharging {  get; private set; }
    public override void Initialize()
    {
        base.Initialize();
        _weaponHolder = FindObjectOfType<K_WeaponHolder>();
    }

    public override void UpdateController(float deltaTime)
    {
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

        if(!isCharging)
        {
            if(_requestedInput.Kick)
            {
                Charge();
            }
        }
        else if(_requestedInput.KickReleased)
        {
            Release();
        }
    }

    private void Charge()
    {
        isCharging = true;
        _animator.Play("Kick Charge", -1, 0f);
        var currentWeapon = _weaponHolder.GetCurrentWeapon();
        if (currentWeapon != -1)
        {
            _weaponHolder.weaponArray[currentWeapon].animator.Play("Kick Charge", -1, 0f);
        }
    }

    private void Release()
    {
        isCharging = false;
        _animator.Play("Kick Released", -1, 0f);
        var currentWeapon = _weaponHolder.GetCurrentWeapon();
        if (currentWeapon != -1)
        {
            _weaponHolder.weaponArray[currentWeapon].animator.Play("Kick Released", -1, 0f);
        }
    }

    private void OnEnable()
    {
        PlayAnimation("Idle");
    }
}
