using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuItem_Resolution : QuickMenuOptionItem
{
	public static Action OnResolutionChange = delegate
	{
	};

	private List<Resolution> resolutions = new List<Resolution>();

	public override void Awake()
	{
		values.Clear();
		Resolution[] array = Screen.resolutions;
		for (int i = 0; i < array.Length; i++)
		{
			if (i == 0)
			{
				resolutions.Add(array[i]);
			}
			else if (array[i - 1].width != array[i].width || array[i - 1].height != array[i].height)
			{
				resolutions.Add(array[i]);
			}
		}
		for (int j = 0; j < resolutions.Count; j++)
		{
			string item = $"{resolutions[j].width}X{resolutions[j].height}";
			values.Add(item);
		}
		base.Awake();
		Refresh();
	}

	public override void Refresh()
	{
		for (int i = 0; i < resolutions.Count; i++)
		{
			if (Screen.width == resolutions[i].width && Screen.height == resolutions[i].height)
			{
				index = i;
				base.Refresh();
				return;
			}
		}
		index = 0;
		base.Refresh();
	}

	public override bool Accept()
	{
		if (Screen.width != resolutions[index].width || Screen.height != resolutions[index].height)
		{
			Screen.SetResolution(resolutions[index].width, resolutions[index].height, Screen.fullScreen);
			if (OnResolutionChange != null)
			{
				OnResolutionChange();
			}
			return true;
		}
		return false;
	}
}
