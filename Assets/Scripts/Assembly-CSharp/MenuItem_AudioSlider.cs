using UnityEngine;

public class MenuItem_AudioSlider : MenuItem_Slider
{
	public override void Next(int sign)
	{
		base.Next(sign);
		Game.audioManager.SetVolume01(gamePrefs, (float)index / 10f);
	}

	public override void Refresh()
	{
		float volume = Game.audioManager.GetVolume01(gamePrefs);
		index = Mathf.RoundToInt(volume * 10f);
		base.Refresh();
	}
}
