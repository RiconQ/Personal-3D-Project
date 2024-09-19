using System.Collections.Generic;
using UnityEngine;

public class MenuItem_List : QuickMenuOptionItem
{
	public string prefsName = "";

	public List<string> names = new List<string>();

	public override void Awake()
	{
		values.Clear();
		for (int i = 0; i < names.Count; i++)
		{
			values.Add(names[i]);
		}
		base.Awake();
		index = Game.gamePrefs.GetValue(prefsName);
	}

	public override void Next(int sign)
	{
		base.Next(sign);
		Game.gamePrefs.UpdateValue(prefsName, index);
	}

	public override bool Accept()
	{
		base.Accept();
		Game.gamePrefs.UpdateValue(prefsName, index);
		return true;
	}

	private void OnApplicationQuit()
	{
		if (prefsName.Length > 0)
		{
			PlayerPrefs.SetInt(prefsName, index);
		}
	}
}
