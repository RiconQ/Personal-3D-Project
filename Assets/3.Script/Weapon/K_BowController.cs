using UnityEngine;

public class K_BowController : K_WeaponController
{
    public override void Initialize()
    {
        base.Initialize();
        this.gameObject.SetActive(false);
    }

    public override void LateUpdateController(float deltaTime)
    {
        base.UpdateController(deltaTime);
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
       
        if (!K_WeaponHolder.instance.isCharging)
        {
            Debug.Log($"_requestedInput.RightMouse {_requestedInput.RightMouse}");
            if (_requestedInput.RightMouse)
            {
                Debug.Log("adsda");
                _animator.SetTrigger("Charge");
                _animator.SetInteger("AttackIndex", 3);
                Charge();
            }
        }
        else
        {
            if (_requestedInput.RightMouseReleased)
            {
                _animator.SetTrigger("Release");
                Release();
            }
        }
    }


    public override void Charge()
    {
        K_WeaponHolder.instance.isCharging = true;
    }
    public override void Release()
    {
        K_WeaponHolder.instance.isCharging = false;
    }

    public override void DropWeapon()
    {
        K_WeaponHolder.instance.DropWeapon(1);
    }
}
