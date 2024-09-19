using UnityEngine;

public class MenuItem_Slider : QuickMenuOptionItem
{
	[SerializeField]
	private string format = "{0}%";

	[SerializeField]
	public float step = 10f;

	[SerializeField]
	public int maxCount = 10;

	[SerializeField]
	public float startsFrom;

	[SerializeField]
	public string gamePrefs;

	public override void Awake()
	{
		values.Clear();
		for (int i = 0; i < maxCount + 1; i++)
		{
			string item = string.Format(format, (startsFrom + (float)i * step).ToString());
			values.Add(item);
		}
		if (gamePrefs.Length != 0 && gamePrefs != "")
		{
			if (GamePrefs.cached.ContainsKey(gamePrefs))
			{
				GamePrefs.cached.TryGetValue(gamePrefs, out index);
			}
			else
			{
				index = Mathf.Clamp(PlayerPrefs.GetInt(gamePrefs), 0, values.Count);
				GamePrefs.cached.Add(gamePrefs, index);
			}
		}
		base.Awake();
	}

	private void UpdatePrefsValue()
	{
		if (gamePrefs.Length != 0 && !(gamePrefs == ""))
		{
			if (!GamePrefs.cached.ContainsKey(gamePrefs))
			{
				GamePrefs.cached.Add(gamePrefs, index);
			}
			else
			{
				Game.gamePrefs.UpdateValue(gamePrefs, index);
			}
		}
	}

	public override void Refresh()
	{
		UpdatePrefsValue();
		base.Refresh();
	}

	public override void Next(int sign)
	{
		base.Next(sign);
		UpdatePrefsValue();
	}

	public override bool Accept()
	{
		return false;
	}
}
