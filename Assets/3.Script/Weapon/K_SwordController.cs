using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SocialPlatforms.Impl;

public class K_SwordController : K_WeaponController
{
    private int _slashIndex = 0;

    public override void Initialize()
    {
        base.Initialize();
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

        if (_requestedInput.RightMouse)
        {
            _animator.SetInteger("AttackIndex", 3);
            _animator.SetTrigger("Hold");
        }
        else if (_requestedInput.RightMouseReleased)
        {
            _animator.SetTrigger("Release");
        }
    }

    private void OnEnable()
    {
        PlayAnimation("PickUp");
        _slashIndex = 0;
    }
}
