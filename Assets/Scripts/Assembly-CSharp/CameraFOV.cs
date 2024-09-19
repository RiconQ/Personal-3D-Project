using System;
using UnityEngine;

public class CameraFOV : MonoBehaviour
{
	private float defaultFOV;

	private float timer;

	private float fov;

	private float targetFov;

	public float kinematicFOV;

	public AnimationCurve curve;

	public PlayerWeapons weapons { get; private set; }

	public Camera cam { get; private set; }

	public void AddToFOV(float value = 10f)
	{
		cam.fieldOfView = defaultFOV + value;
	}

	private void Awake()
	{
		cam = GetComponent<Camera>();
		weapons = base.transform.parent.GetComponentInChildren<PlayerWeapons>();
		int value = Game.gamePrefs.GetValue("FOV");
		defaultFOV = 80 + value * 5;
		cam.fieldOfView = defaultFOV;
		kinematicFOV = 0f;
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Combine(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
	}

	private void OnDestroy()
	{
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Remove(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
	}

	private void CheckSettings(string prefs)
	{
		int value = Game.gamePrefs.GetValue("FOV");
		defaultFOV = 80 + value * 5;
		cam.fieldOfView = defaultFOV;
	}

	public void Tick()
	{
		fov = cam.fieldOfView;
		targetFov = defaultFOV;
		targetFov += (Game.player.rb.isKinematic ? kinematicFOV : (Game.player.slide.isSliding ? 6.5f : 0f));
		targetFov += (StyleRanking.rage ? 5 : 0);
		fov = Mathf.Lerp(fov, targetFov + weapons.Holding() * 5f, Time.deltaTime * 20f);
		cam.fieldOfView = fov;
	}
}
