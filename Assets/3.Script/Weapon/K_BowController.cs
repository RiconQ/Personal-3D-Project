using UnityEngine;

public class K_BowController : K_WeaponController
{
    private bool _canShoot = true;
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

        if (!K_WeaponHolder.instance.isCharging && !K_KickController.instance.kickCharging)
        {
            //Debug.Log($"_requestedInput.RightMouse {_requestedInput.RightMouse}");
            if (_requestedInput.RightMouse)
            {
                _animator.SetTrigger("Charge");
                _animator.SetInteger("AttackIndex", 3);
                Charge();
            }
            else if (_requestedInput.LeftMouse && _canShoot)
            {
                _canShoot = false;
                _animator.SetTrigger("Charge");
                _animator.SetInteger("AttackIndex", 0);
                Charge();
            }
        }
        else if(K_WeaponHolder.instance.isCharging && !K_KickController.instance.kickCharging)
        {
            if (_requestedInput.RightMouseReleased)
            {
                _animator.SetTrigger("Release");
                Release();
            }
            else if (_requestedInput.LeftMouseReleased)
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

    public void ResetShoot()
    {
        _canShoot = true;
    }

    public override void ResetVar()
    {
        ResetShoot();
    }
}
