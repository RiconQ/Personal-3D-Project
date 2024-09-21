using UnityEngine;

public class K_KickController : K_WeaponController
{
    public static K_KickController instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    [SerializeField] private float _kickCoolDown = 1f;

    public bool canKick { get; private set; }
    public override void Initialize()
    {
        base.Initialize();
        canKick = true;
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

        if (!K_WeaponHolder.instance.isCharging)
        {
            //Debug.Log("ischarging false");
            if (_requestedInput.Kick && canKick)
            {
                Charge();
            }
        }
        else
        {
            if (_requestedInput.KickReleased)
            {
                Release();
            }
        }
    }

    public override void Charge()
    {
        canKick = false;
        K_WeaponHolder.instance.isCharging = true;
        _animator.Play("Kick Charge", -1, 0f);
        var currentWeapon = K_WeaponHolder.instance.GetCurrentWeapon();
        if (currentWeapon != -1)
        {
            K_WeaponHolder.instance.weaponArray[currentWeapon].animator.Play("Kick Charge", -1, 0f);
        }
    }

    public override void Release()
    {
        K_WeaponHolder.instance.isCharging = false;
        _animator.Play("Kick Released", -1, 0f);
        var currentWeapon = K_WeaponHolder.instance.GetCurrentWeapon();
        if (currentWeapon != -1)
        {
            K_WeaponHolder.instance.weaponArray[currentWeapon].animator.Play("Kick Released", -1, 0f);
        }
        Invoke(nameof(ResetKick), _kickCoolDown);
    }

    private void ResetKick()
    {
        canKick = true;
    }

    private void OnEnable()
    {
        PlayAnimation("Idle");
    }

    public override void DropWeapon()
    {
    }
}
