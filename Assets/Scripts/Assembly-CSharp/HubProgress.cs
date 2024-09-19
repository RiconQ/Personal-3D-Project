using System;
using UnityEngine;

public class HubProgress : MonoBehaviour
{
	public HubData hub;

	public TextMesh textMesh;

	private void Awake()
	{
		textMesh = GetComponent<TextMesh>();
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Combine(Game.OnPlayableLevelLoaded, new Action<string>(OnLevelLoaded));
	}

	private void OnDestroy()
	{
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Remove(Game.OnPlayableLevelLoaded, new Action<string>(OnLevelLoaded));
	}

	private void OnLevelLoaded(string sceneName)
	{
		hub = LevelsData.instance.GetHubByLevelName(sceneName);
		if (!hub)
		{
			return;
		}
		hub.CheckProgress();
		for (int i = 0; i < hub.levels.Count; i++)
		{
			if (i == 0)
			{
				textMesh.text = $"<b>{hub.levels[0].publicName}</b> {hub.levels.Count}";
			}
			else
			{
				textMesh.text += $"\n{hub.levels[i].GetLevelPublicName()} R{hub.levels[i].results.rank}, T{hub.levels[i].results.time}";
			}
		}
		textMesh.text += "\n";
		textMesh.text += $"\nProgress By Time {hub.ProgressByTime}%";
		textMesh.text += $"\nProgress By SSS Rank {hub.ProgressBySSSRank}%";
	}
}
