using System;
using System.Collections.Generic;

[Serializable]
public class HubsUIEntry
{
	public HubData data;

	public SceneData[] extraLevels;

	public List<HubsUILevelEntry> items = new List<HubsUILevelEntry>();

	public int index = -1;

	public void Tick()
	{
		if (index > -1 && index < items.Count)
		{
			items[index].Tick();
		}
	}

	public bool Load()
	{
		if (index > -1 && items[index].data.sceneType != SceneData.SceneType.Hub && items[index].data.results.reached == 0)
		{
			return false;
		}
		Hub.lastHub = ((extraLevels.Length == 0) ? data.levels[0].sceneName : extraLevels[0].sceneName);
		Game.instance.LoadLevel((index == -1) ? data.GetFirstLevel().sceneName : items[index].data.sceneReference.ScenePath);
		return true;
	}

	public bool NextItem(int sign = 1)
	{
		if (items.Count == 0)
		{
			return false;
		}
		int i = index;
		i = ((sign != 0) ? i.NextClamped(items.Count, -sign) : 0);
		if (index != i || sign == 0)
		{
			index = i;
			for (int j = 0; j < items.Count; j++)
			{
				items[j].Select(j == i);
			}
			return true;
		}
		return false;
	}
}
