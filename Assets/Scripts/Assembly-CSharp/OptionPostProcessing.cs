using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class OptionPostProcessing : MonoBehaviour
{
	private bool value;

	private Camera cam;

	private PostProcessVolume effects;

	private MotionBlur motionBlur;

	private Bloom bloom;

	private Grayscale grayscale;

	private void Awake()
	{
		cam = GetComponent<Camera>();
		effects = GetComponent<PostProcessVolume>();
		effects.profile.TryGetSettings<MotionBlur>(out motionBlur);
		effects.profile.TryGetSettings<Bloom>(out bloom);
		effects.profile.TryGetSettings<Grayscale>(out grayscale);
		value = Game.gamePrefs.GetValue("PostProcessing") == 1;
		motionBlur.enabled.value = value;
		value = Game.gamePrefs.GetValue("Bloom") == 1;
		bloom.enabled.value = value;
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Combine(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
	}

	private void OnDestroy()
	{
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Remove(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
	}

	private void CheckSettings(string prefs)
	{
		if (prefs == "PostProcessing")
		{
			value = Game.gamePrefs.GetValue(prefs) == 1;
			motionBlur.enabled.value = value;
		}
		else if (prefs == "Bloom")
		{
			value = Game.gamePrefs.GetValue(prefs) == 1;
			bloom.enabled.value = value;
		}
	}

	private void Update()
	{
		if ((bool)grayscale && (bool)Game.player)
		{
			grayscale.blend.value = Mathf.Lerp(grayscale.blend.value, (StyleRanking.rage || Game.player.isDamaged) ? 1 : 0, Time.unscaledDeltaTime * 4f);
		}
	}
}
