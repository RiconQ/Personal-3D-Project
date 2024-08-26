using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter _playerCharacter;
    [SerializeField] private PlayerCamera _playerCamera;
    private void Start()
    {
        _playerCharacter.Initialize();
        _playerCamera.Initialize(_playerCharacter.GetCameraTarget());
    }
}
