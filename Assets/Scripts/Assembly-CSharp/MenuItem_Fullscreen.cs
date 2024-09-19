using UnityEngine;

public class MenuItem_Fullscreen : MenuItem_Toggle
{
	public override void Awake()
	{
		base.Awake();
		Refresh();
	}

	public override void Refresh()
	{
		index = (Screen.fullScreen ? 1 : 0);
		base.Refresh();
	}

	public override bool Accept()
	{
		base.Accept();
		Screen.fullScreen = index > 0;
		return true;
	}
}
