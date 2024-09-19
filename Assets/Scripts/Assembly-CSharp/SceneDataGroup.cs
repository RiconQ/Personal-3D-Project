using System;
using System.Collections.Generic;

[Serializable]
public class SceneDataGroup : IComparable<SceneDataGroup>
{
	public string name;

	public int index;

	public bool show = true;

	public HubData hub;

	public List<SceneData> levels;

	public int progress;

	public int progressS;

	public int progressSSS;

	public SceneDataGroup(string name, int index)
	{
		this.name = name;
		this.index = index;
		levels = new List<SceneData>();
	}

	public void Clear()
	{
		levels.Clear();
	}

	public void CheckProgress()
	{
		int num = 0;
		progress = (progressS = (progressSSS = 0));
		for (int i = 1; i < levels.Count; i++)
		{
			if (levels[i].sceneType >= SceneData.SceneType.Tutorial)
			{
				num++;
				if (levels[i].sceneType == SceneData.SceneType.Tutorial)
				{
					progress += ((levels[i].results.time != 0f) ? 1 : 0);
					continue;
				}
				int rank = levels[i].results.rank;
				progress += ((rank >= 1) ? 1 : 0);
				progressS += ((rank >= 4) ? 1 : 0);
				progressSSS += ((rank >= 6) ? 1 : 0);
			}
		}
		progress = progress.PercentOf(num);
		progressS = progressS.PercentOf(num);
		progressSSS = progressSSS.PercentOf(num);
	}

	public int CompareTo(SceneDataGroup other)
	{
		return index.CompareTo(other.index);
	}
}
