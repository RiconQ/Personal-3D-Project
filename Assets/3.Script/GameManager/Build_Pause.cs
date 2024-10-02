using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Build_Pause : MonoBehaviour
{
    private void Start()
    {
        transform.gameObject.SetActive(false);
    }

    public void PauseGame()
    {
        Player.instance.PlayerCamera.isPause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        this.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Player.instance.PlayerCamera.isPause = false;
        Time.timeScale = 1f;
        GameManager.instance.ReloadScene();
    }

    public void ToHub()
    {
        Player.instance.PlayerCamera.isPause = false;
        Time.timeScale = 1f;
        GameManager.instance.ChangeScene("Main Hub");
    }

    public void Continue()
    {
        Player.instance.PlayerCamera.isPause = false;
        Time.timeScale = 1f;
        this.gameObject.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
