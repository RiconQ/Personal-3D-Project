using UnityEngine;

public class MenuItem_TargetFPS : QuickMenuOptionItem
{
	public int[] FPS = new int[4] { 60, 120, 240, 1000 };

	private string prefsName = "TargetFPS";

	public override void Awake()
	{
		values.Clear();
		for (int i = 0; i < FPS.Length; i++)
		{
			values.Add(FPS[i].ToString());
		}
		base.Awake();
		index = Game.gamePrefs.GetValue(prefsName);
		Application.targetFrameRate = FPS[index];
	}

	public override void Next(int sign)
	{
		base.Next(sign);
		Game.gamePrefs.UpdateValue(prefsName, index);
		Application.targetFrameRate = FPS[index];
	}

	public override bool Accept()
	{
		base.Accept();
		Game.gamePrefs.UpdateValue(prefsName, index);
		Application.targetFrameRate = FPS[index];
		return true;
	}

	private void OnApplicationQuit()
	{
		PlayerPrefs.SetInt(prefsName, index);
	}
}
