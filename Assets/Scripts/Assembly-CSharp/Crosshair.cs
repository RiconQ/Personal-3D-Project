using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class Crosshair : MonoBehaviour
{
	private CanvasGroup cg;

	private int state;

	private int alwaysVisible;

	public GameObject[] styles;

	public RectTransform[] segments;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
		state = Game.gamePrefs.GetValue("Crosshair");
		alwaysVisible = Game.gamePrefs.GetValue("CrosshairMode");
		switch (state)
		{
		case 0:
			cg.alpha = 0f;
			break;
		case 1:
		case 2:
		{
			for (int i = 0; i < styles.Length; i++)
			{
				styles[i].SetActive(state - 1 == i);
			}
			break;
		}
		}
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Combine(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
	}

	private void OnDestroy()
	{
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Remove(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
	}

	private void CheckSettings(string prefs)
	{
		if (prefs == "Crosshair")
		{
			state = Game.gamePrefs.GetValue(prefs);
			switch (state)
			{
			case 0:
				cg.alpha = 0f;
				break;
			case 1:
			case 2:
			{
				for (int i = 0; i < styles.Length; i++)
				{
					styles[i].SetActive(state - 1 == i);
				}
				break;
			}
			}
		}
		else if (prefs == "CrosshairMode")
		{
			alwaysVisible = Game.gamePrefs.GetValue("CrosshairMode");
		}
	}

	private void UpdateAlpha()
	{
		if (alwaysVisible == 0)
		{
			cg.alpha = Mathf.Lerp(cg.alpha, ((Game.player.weapons.IsAttacking() | Game.player.weapons.kickController.isCharging) & Game.player.inputActive) ? 1 : 0, Time.unscaledDeltaTime * 8f);
		}
		else
		{
			cg.alpha = Mathf.Lerp(cg.alpha, Game.player.inputActive ? 1 : 0, Time.unscaledDeltaTime * 8f);
		}
	}

	private void Update()
	{
		switch (state)
		{
		case 1:
			UpdateAlpha();
			break;
		case 2:
		{
			UpdateAlpha();
			RectTransform[] array = segments;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].localPosition = new Vector2(Mathf.Lerp(-16f, -8f, cg.alpha), 0f);
			}
			break;
		}
		}
	}
}
