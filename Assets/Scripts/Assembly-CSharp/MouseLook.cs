using System;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
	private Transform t;

	private const float mouseDeadzone = 1E-10f;

	private const float joystickDeadzone = 0.1f;

	private const float xAngleLimit = 89.9f;

	private bool inverted;

	private bool rawInputs;

	private float sensitivity = 1.75f;

	private string xMouseName = "Mouse X";

	private string yMouseName = "Mouse Y";

	[HideInInspector]
	public Vector2 rotation;

	public AnimationCurve stickResponseCurve;

	public Vector2 stickInput;

	public BaseEnemy enemy;

	public float AimHelperTimer;

	public float mgt => stickInput.magnitude;

	public float x { get; private set; }

	public float y { get; private set; }

	private void Awake()
	{
		t = base.transform;
		rotation = t.eulerAngles;
		int value = Game.gamePrefs.GetValue("MouseSensitivity");
		sensitivity = (float)value * 0.1f + 0.5f;
		int value2 = Game.gamePrefs.GetValue("MouseInvert");
		inverted = value2 == 1;
		int value3 = Game.gamePrefs.GetValue("MouseNoAcceleration");
		rawInputs = value3 == 1;
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Combine(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
	}

	private void OnDestroy()
	{
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Remove(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
	}

	private void CheckSettings(string prefs)
	{
		switch (prefs)
		{
		case "MouseSensitivity":
		{
			int value3 = Game.gamePrefs.GetValue(prefs);
			sensitivity = (float)value3 * 0.1f + 0.5f;
			break;
		}
		case "MouseInvert":
		{
			int value2 = Game.gamePrefs.GetValue(prefs);
			inverted = value2 == 1;
			break;
		}
		case "MouseNoAcceleration":
		{
			int value = Game.gamePrefs.GetValue(prefs);
			rawInputs = value == 1;
			break;
		}
		}
	}

	private void Update()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (!PlayerController.gamepad)
		{
			y = (rawInputs ? Input.GetAxisRaw(xMouseName) : Input.GetAxis(xMouseName));
			x = (rawInputs ? Input.GetAxisRaw(yMouseName) : Input.GetAxis(yMouseName));
			if (y.Abs() > 1E-10f)
			{
				rotation.y += y * sensitivity;
			}
			if (x.Abs() > 1E-10f)
			{
				rotation.x += (0f - x) * sensitivity * (float)((!inverted) ? 1 : (-1));
			}
		}
		else
		{
			stickInput = new Vector2(0f - Input.GetAxisRaw("Mouse Y JOY"), Input.GetAxisRaw("Mouse X JOY"));
			float magnitude = stickInput.magnitude;
			magnitude = stickResponseCurve.Evaluate(magnitude);
			stickInput = stickInput.normalized * magnitude;
			x = stickInput.x * 100f * Time.unscaledDeltaTime;
			y = stickInput.y * 100f * Time.unscaledDeltaTime;
			if ((Game.player.AttackPressed() || (Game.player.AltPressed() && Game.player.weapons.currentWeapon != -1) || Game.player.KickPressed()) && CrowdControl.instance.GetClosestEnemyToNormal(t.position - t.forward * 0.5f, t.forward, 45f, 20f, out enemy))
			{
				AimHelperTimer = 1f;
			}
			if (stickInput.x == 0f && stickInput.y == 0f)
			{
				if ((bool)enemy && (Game.player.AttackHolded() || Game.player.AltHolded() || Game.player.KickHolded()))
				{
					if (enemy.dead)
					{
						enemy = null;
						return;
					}
					LookInDirSmooth(t.position.DirTo(enemy.GetActualPosition()), 8f - (1f - AimHelperTimer) * 4f);
					AimHelperTimer = Mathf.MoveTowards(AimHelperTimer, 0f, Time.deltaTime);
					if (AimHelperTimer == 0f)
					{
						enemy = null;
					}
				}
			}
			else
			{
				rotation.y += y * sensitivity;
				rotation.x += (0f - x) * sensitivity * (float)((!inverted) ? 1 : (-1));
				if ((bool)enemy)
				{
					enemy = null;
				}
			}
		}
		rotation.x = Mathf.Clamp(rotation.x, -89.9f, 89.9f);
		t.rotation = Quaternion.Euler(rotation);
	}

	public void LookInDir(Vector3 dir)
	{
		SetRotation(Quaternion.LookRotation(dir));
	}

	public void LookInDirSmooth(Vector3 dir, float speed = 4f, bool unscaled = false)
	{
		_ = t.rotation;
		t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(dir), speed * (unscaled ? Time.unscaledDeltaTime : Time.deltaTime));
		rotation.x = t.eulerAngles.x;
		rotation.y = t.eulerAngles.y;
		if (rotation.x > 90f)
		{
			rotation.x -= 360f;
		}
		else if (rotation.x < -90f)
		{
			rotation.x += 360f;
		}
	}

	public void LookAt(Vector3 pos)
	{
		SetRotation(Quaternion.LookRotation(t.position.DirTo(pos)));
	}

	public void LookAtSmooth(Vector3 pos, float speed = 4f, bool unscaled = false)
	{
		_ = t.rotation;
		t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(t.position.DirTo(pos)), speed * (unscaled ? Time.unscaledDeltaTime : Time.deltaTime));
		rotation.x = t.eulerAngles.x;
		rotation.y = t.eulerAngles.y;
		if (rotation.x > 90f)
		{
			rotation.x -= 360f;
		}
		else if (rotation.x < -90f)
		{
			rotation.x += 360f;
		}
	}

	public void SetRotation(Quaternion rot)
	{
		if (!t)
		{
			t = base.transform;
		}
		t.rotation = rot;
		rotation.x = t.eulerAngles.x;
		rotation.y = t.eulerAngles.y;
		if (rotation.x > 90f)
		{
			rotation.x -= 360f;
		}
		else if (rotation.x < -90f)
		{
			rotation.x += 360f;
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		angle %= 360f;
		if (angle >= -360f && angle <= 360f)
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
		}
		return Mathf.Clamp(angle, min, max);
	}
}
