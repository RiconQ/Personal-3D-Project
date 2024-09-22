using System.Net.Mail;
using Unity.VisualScripting;
using UnityEngine;

public struct ControllerInput
{
    public bool Kick;
    public bool KickReleased;
    public bool LeftMouse;
    public bool LeftMouseReleased;
    public bool RightMouse;
    public bool RightMouseReleased;
    public bool Crouch;
}

public struct RequestedControllerInput
{
    public bool Kick;
    public bool KickReleased;
    public bool LeftMouse;
    public bool LeftMouseReleased;
    public bool RightMouse;
    public bool RightMouseReleased;
    public bool Crouch;
}

public enum EInput
{
    Kick,
    KickReleased,
    LeftMouse,
    LeftMouseReleased,
    RightMouse,
    RightMouseReleased,
    Crouch,
    None
}

public abstract class K_WeaponController : MonoBehaviour
{
    [SerializeField] protected Animator _animator;
    public Animator animator => _animator;
    protected RequestedControllerInput _requestedInput;
    protected PlayerInputAction _inputAction;
    protected EInput _lastInput;

    public virtual void Initialize()
    {
        _inputAction = new PlayerInputAction();
        _inputAction.Enable();
    }
    public virtual void UpdateController(float deltaTime)
    {
    }
    public abstract void DropWeapon();
    public abstract void LateUpdateController(float deltaTime);
    public abstract void UpdateInput(ControllerInput weaponInput, float deltaTime);
    public abstract void Charge();
    public abstract void Release();
    public abstract void ResetVar();
    public void PlayAnimation(string animationName, float fixedTransitionDuration = 0.25f)
    {
        _animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);
    }

    private void OnDestroy()
    {
        _inputAction.Dispose();
    }

    public bool IsAnimationEnd()
    {
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.normalizedTime >= 1.0f && !_animator.IsInTransition(0);
    }

    public bool IsPlaying(string clipName)
    {
        AnimatorStateInfo currentAnimatorState = _animator.GetCurrentAnimatorStateInfo(0);
        return currentAnimatorState.IsName(clipName);
    }

    public void SetKickingTrue()
    {
        _animator.SetBool("Kicking", true);
    }
    public void SetKickingFalse()
    {
        _animator.SetBool("Kicking", false);
    }
}
