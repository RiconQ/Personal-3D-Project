using UnityEngine;

public class MenuItem_PrefsToggle : MenuItem_Toggle
{
	[SerializeField]
	private string prefs;

	private int value;

	public override void Awake()
	{
		base.Awake();
		if (prefs.Length != 0)
		{
			if (!GamePrefs.cached.ContainsKey(prefs))
			{
				value = PlayerPrefs.GetInt(prefs);
				GamePrefs.cached.Add(prefs, value);
			}
			else
			{
				value = GamePrefs.cached[prefs];
			}
			Refresh();
		}
	}

	public override void Refresh()
	{
		index = value;
		base.Refresh();
	}

	public override bool Accept()
	{
		base.Accept();
		value = ((value != 1) ? 1 : 0);
		Game.gamePrefs.UpdateValue(prefs, value);
		return true;
	}

	private void OnApplicationQuit()
	{
		if (prefs.Length > 0)
		{
			PlayerPrefs.SetInt(prefs, value);
		}
	}
}
