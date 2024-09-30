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
                _lastInput = EInput.RightMouse;
                _animator.SetTrigger("Charge");
                _animator.SetInteger("AttackIndex", 3);
                Charge();
            }
            else if (_requestedInput.LeftMouse && _canShoot)
            {
                _lastInput = EInput.LeftMouse;
                _canShoot = false;
                _animator.SetTrigger("Charge");
                _animator.SetInteger("AttackIndex", 0);
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
            else if (_requestedInput.LeftMouseReleased && _lastInput == EInput.LeftMouse)
            {
                _lastInput = EInput.LeftMouseReleased;
                _animator.SetTrigger("Release");
                Release();
            }
        }
    }

    public void Shoot()
    {
        //Enable Arrow
        var arrow = ArrowPooling.GetObject();
        arrow.Shoot();
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

    private void OnEnable()
    {
        ResetVar();
    }

    public void ResetShoot()
    {
        _canShoot = true;
    }

    public override void ResetVar()
    {
        ResetShoot();
    }

    public void Throw()
    {
        var vector = Player.instance.PlayerCamera.transform.forward;
        var throwedSword = K_WeaponHolder.instance.bowPool.GetObject();

        //Get Target

        throwedSword.ThrowWeapon(Quaternion.LookRotation(vector));
    }
}
