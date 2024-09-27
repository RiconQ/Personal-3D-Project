using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class K_Jumper : MonoBehaviour
{
    public enum EJumperState
    {
        None,
        Start,
        Jumping,
        End
    }
    private EJumperState _state;

    private Vector3 _direction;
    private float _speed;
    public bool isJumping {  get; private set; }

    private void Start()
    {
        _state = EJumperState.None;
        _direction = Vector3.zero;
        _speed = 0f;
        isJumping = false;
    }

    public void StartJumper(Vector3 direction, float speed)
    {
        _state = EJumperState.Start;
        _direction = direction;
        _speed = speed;
        isJumping = true;
    }

    public void Jumping(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (_state)
        {
            case EJumperState.Start:
                Player.instance.PlayerCharacter.Motor.ForceUnground(0f);
                currentVelocity = Vector3.zero;
                _state = EJumperState.Jumping;
                break;
            case EJumperState.Jumping:
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, Player.instance.PlayerCharacter.Motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, _speed);

                currentVelocity += _direction * (targetVerticalSpeed - currentVerticalSpeed);

                _state = EJumperState.End;
                break;
            case EJumperState.End:
                isJumping = false;
                _state = EJumperState.None;
                break;
        }
    }
}