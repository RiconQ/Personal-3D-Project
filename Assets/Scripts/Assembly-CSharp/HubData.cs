using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HubData", menuName = "Scenes/Hub Data")]
public class HubData : ScriptableObject
{
	public Sprite thumbnail;

	public List<SceneData> levels = new List<SceneData>();

	public EnviromentPreset envPreset;

	[Header("Music")]
	public AudioClip AmbientMusic;

	public MusicSet MusicSet;

	public float ProgressByTime { get; private set; }

	public float ProgressBySRank { get; private set; }

	public float ProgressBySSSRank { get; private set; }

	public SceneData GetFirstLevel()
	{
		return levels[0];
	}

	public bool IsFirstLevelInHub(SceneData level)
	{
		return levels[0] == level;
	}

	public string GetNext(string fromScene)
	{
		bool flag = false;
		for (int i = 0; i < levels.Count; i++)
		{
			if (!flag)
			{
				if (levels[i].sceneName == fromScene)
				{
					flag = true;
				}
			}
			else if (!levels[i].testLevel)
			{
				return levels[i].sceneName;
			}
		}
		return GetFirstLevel().sceneName;
	}

	public void Remove(SceneData level)
	{
		if (levels.Contains(level))
		{
			level.area = null;
			levels.Remove(level);
		}
	}

	public void Add(SceneData level)
	{
		if (!levels.Contains(level))
		{
			if (level.area != null && level.area != this)
			{
				level.area.Remove(level);
			}
			level.area = this;
			if (level.sceneType == SceneData.SceneType.Hub)
			{
				levels.Insert(0, level);
			}
			else
			{
				levels.Add(level);
			}
		}
	}

	public void ClearMissing()
	{
		for (int i = 0; i < levels.Count; i++)
		{
			if (!levels[i])
			{
				levels.RemoveAt(i);
				i--;
			}
		}
	}

	public void CheckProgress()
	{
		int num = 0;
		int num2 = 0;
		float num4 = (ProgressBySSSRank = 0f);
		float progressByTime = (ProgressBySRank = num4);
		ProgressByTime = progressByTime;
		foreach (SceneData level in levels)
		{
			if (level.sceneType != 0 && level.sceneType != SceneData.SceneType.Hub && !level.testLevel)
			{
				num2++;
				ProgressByTime += ((level.results.time != 0f) ? 1 : 0);
				if (level.sceneType != SceneData.SceneType.Tutorial)
				{
					num++;
					int rank = level.results.rank;
					ProgressBySRank += ((rank >= 4) ? 1 : 0);
					ProgressBySSSRank += ((rank >= 6) ? 1 : 0);
				}
			}
		}
		ProgressByTime /= (float)num2;
		ProgressBySRank /= (float)num2;
		ProgressBySSSRank /= (float)num2;
	}
}
