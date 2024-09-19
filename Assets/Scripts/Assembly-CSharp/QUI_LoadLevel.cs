using System;
using UnityEngine;

public class QUI_LoadLevel : QuickMenuItem
{
	public string levelToLoad;

	private QuickMenu menu;

	public override void Awake()
	{
		base.Awake();
		menu = GetComponentInParent<QuickMenu>();
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Combine(Game.OnPlayableLevelLoaded, new Action<string>(OnPlayableLevelLoaded));
	}

	private void OnDestroy()
	{
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Remove(Game.OnPlayableLevelLoaded, new Action<string>(OnPlayableLevelLoaded));
	}

	private void OnPlayableLevelLoaded(string name)
	{
		if (base.gameObject.activeInHierarchy != (levelToLoad != name))
		{
			base.gameObject.SetActive(levelToLoad != name);
		}
		if (levelToLoad == "Hub")
		{
			base.gameObject.SetActive(LevelsData.instance.GetLevelByName(name).IsPlayebleNotHub());
		}
		if (levelToLoad == "Skip")
		{
			base.gameObject.SetActive(LevelsData.instance.GetLevelByName(name).sceneType == SceneData.SceneType.Cutscene);
		}
	}

	public override bool Accept()
	{
		base.Accept();
		if (levelToLoad == "Restart")
		{
			Game.instance.Restart();
		}
		else if (levelToLoad == "Continue")
		{
			string text;
			if (Hub.lastHub.Length > 0)
			{
				text = Hub.lastHub;
			}
			else
			{
				text = PlayerPrefs.GetString("Last Hub");
				if (text.Length == 0)
				{
					text = LevelsData.instance.hubs[0].levels[0].sceneReference.ScenePath;
				}
			}
			Game.instance.LoadLevel(text);
		}
		else if (levelToLoad == "Quit")
		{
			if (!Application.isEditor)
			{
				Game.instance.Quit();
			}
			else
			{
				Game.instance.LoadLevel(Game.fallbackScene);
			}
		}
		else if (levelToLoad == "Last")
		{
			Game.instance.LoadLevel(Game.mission.GetLastLevel());
		}
		else if (levelToLoad == "Hub")
		{
			Game.instance.LoadLevel(Hub.lastHub);
		}
		else if (levelToLoad == "Skip")
		{
			CutsceneLevel.instance.Skip();
		}
		else if (levelToLoad == "Next")
		{
			string next = LevelsData.instance.GetHubByAnyLevel(Game.lastSceneWithPlayer).GetNext(Game.lastSceneWithPlayer);
			Game.instance.LoadLevel(next);
		}
		else
		{
			Game.instance.LoadLevel(levelToLoad);
		}
		menu.locked = true;
		return true;
	}
}
