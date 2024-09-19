using UnityEngine;

public class MenuItem_VSync : MenuItem_Toggle
{
	public override void Awake()
	{
		base.Awake();
		index = Game.gamePrefs.GetValue("VSync");
		if (QualitySettings.vSyncCount != index)
		{
			QualitySettings.vSyncCount = index;
		}
		Refresh();
	}

	public override bool Accept()
	{
		base.Accept();
		if (QualitySettings.vSyncCount != index)
		{
			QualitySettings.vSyncCount = index;
		}
		Game.gamePrefs.UpdateValue("VSync", index);
		return true;
	}
}
