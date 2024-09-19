using System;
using UnityEngine;

public class SecondCamera : MonoBehaviour
{
	public AnimationCurve curve;

	private Camera cam;

	private AudioListener audioListener;

	private Transform t;

	private float aAngle;

	private float bAngle;

	private float angle;

	private float timer;

	private bool hashed_wideMode;

	private void Awake()
	{
		t = base.transform;
		aAngle = t.localEulerAngles.y;
		bAngle = t.localEulerAngles.y + 5f;
		cam = GetComponentInChildren<Camera>();
		audioListener = GetComponentInChildren<AudioListener>();
		curve = new AnimationCurve();
		curve.AddKey(new Keyframe(0f, 0f, 0f, 2f));
		curve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
		Game.OnPause = (Action<bool>)Delegate.Combine(Game.OnPause, new Action<bool>(OnGamePaused));
	}

	private void Start()
	{
		cam.enabled = false;
		audioListener.enabled = false;
	}

	private void OnDestroy()
	{
		Game.OnPause = (Action<bool>)Delegate.Remove(Game.OnPause, new Action<bool>(OnGamePaused));
	}

	private void OnEnable()
	{
		timer = 0f;
		cam.fieldOfView = 60f;
		t.localEulerAngles = t.localEulerAngles.With(null, aAngle);
	}

	private void OnGamePaused(bool gamePaused)
	{
		if (Game.loading == null && (!Game.player || !Game.player.isDead))
		{
			if (gamePaused)
			{
				timer = 0f;
				cam.fieldOfView = 60f;
				t.localEulerAngles = t.localEulerAngles.With(null, aAngle);
				Game.player.camController.EnableCameraAndListener(value: false);
				cam.enabled = true;
				audioListener.enabled = true;
				LastActiveCamera.Set(cam);
				Game.wideMode.Show();
			}
			else if ((bool)Game.player)
			{
				cam.enabled = false;
				audioListener.enabled = false;
				Game.player.camController.EnableCameraAndListener(value: true);
				CameraController.shake.Shake(1);
				Game.player.fov.AddToFOV();
				Game.wideMode.Hide();
			}
		}
	}

	private void Update()
	{
		if (cam.enabled)
		{
			if (timer != 1f)
			{
				timer = Mathf.MoveTowards(timer, 1f, Time.unscaledDeltaTime);
				angle = Mathf.LerpUnclamped(aAngle, bAngle, curve.Evaluate(timer));
				t.localEulerAngles = t.localEulerAngles.With(null, angle);
			}
			cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 75f, Time.unscaledDeltaTime * 4f);
		}
	}
}
