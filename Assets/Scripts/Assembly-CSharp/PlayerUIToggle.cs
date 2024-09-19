using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PlayerUIToggle : MonoBehaviour
{
	public CanvasGroup cg;

	private float alpha;

	private bool showUI;

	private void Awake()
	{
		showUI = Game.gamePrefs.GetValue("PlayerUI") == 1;
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Combine(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Combine(Game.OnPlayableLevelLoaded, new Action<string>(Check));
	}

	private void OnDestroy()
	{
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Remove(Game.OnPlayableLevelLoaded, new Action<string>(Check));
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Remove(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
	}

	private void CheckSettings(string prefs)
	{
		if (prefs == "PlayerUI")
		{
			showUI = Game.gamePrefs.GetValue("PlayerUI") == 1;
		}
	}

	private void Check(string name)
	{
		alpha = ((LevelsData.currentPlayableLevel.results.time != 0f) ? 1 : 0);
	}

	private void Update()
	{
		cg.alpha = ((!showUI) ? 0f : (Game.paused ? 0f : (alpha - Game.wideMode.cg.alpha)));
	}
}
