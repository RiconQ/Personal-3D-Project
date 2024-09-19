using System;
using UnityEngine;

public class ShadyKnightTitle : QUI_Title
{
	public GameObject objLogo;

	public GameObject objPrompt;

	public override void Accept()
	{
		if (base.active)
		{
			foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
			{
				if (Input.GetKey(value))
				{
					PlayerController.gamepad = value == KeyCode.JoystickButton0;
					break;
				}
			}
		}
		base.Accept();
	}

	public override void Activate()
	{
		objLogo.SetActive(value: true);
		objPrompt.SetActive(value: false);
		base.Activate();
	}

	public override void Deactivate()
	{
		objLogo.SetActive(value: false);
		objPrompt.SetActive(value: true);
		if ((bool)Game.message)
		{
			Game.message.Hide();
		}
		base.Deactivate();
	}
}
