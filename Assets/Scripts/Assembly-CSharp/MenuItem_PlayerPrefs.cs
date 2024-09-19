using UnityEngine;

public class MenuItem_PlayerPrefs : MenuItem_Toggle
{
	[SerializeField]
	private string playerPrefsName;

	public override void Awake()
	{
		base.Awake();
	}

	public override void Refresh()
	{
		base.Refresh();
	}

	public override bool Accept()
	{
		return false;
	}
}
