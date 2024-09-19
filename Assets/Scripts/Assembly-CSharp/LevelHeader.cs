using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelHeader : MonoBehaviour
{
	public Text text;

	private void Awake()
	{
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Combine(Game.OnPlayableLevelLoaded, new Action<string>(OnLevelLoaded));
	}

	private void OnDestroy()
	{
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Remove(Game.OnPlayableLevelLoaded, new Action<string>(OnLevelLoaded));
	}

	private void OnLevelLoaded(string sceneName)
	{
		text.text = LevelsData.instance.GetLevelByName(sceneName).publicName;
	}
}
