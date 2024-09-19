using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CameraTrigger : MonoBehaviour
{
	public bool ManualReset;

	public Transform tTarget;

	public Vector3 Offset = Vector3.up;

	public Vector3 targetLocalPos = new Vector3(0f, 1f, 5f);

	public CameraTriggerPreset preset;

	public Collider Clldr;

	public UnityEvent ExitEvent;

	private void Awake()
	{
		Clldr = GetComponent<Collider>();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	[Button]
	public void Setup()
	{
		base.gameObject.layer = 12;
		Clldr = base.gameObject.AddComponent<BoxCollider>();
		Clldr.isTrigger = true;
		(Clldr as BoxCollider).size = new Vector3(24f, 24f, 6f);
	}

	private void Reset()
	{
		if (!ManualReset)
		{
			Clldr.enabled = true;
		}
	}

	private IEnumerator LockingOnTarget()
	{
		Clldr.enabled = false;
		yield return null;
		CameraController.shake.Shake(2);
		Game.player.sway.Sway(0f, 0f, 5f, 2f);
		Game.player.Deactivate();
		Game.wideMode.Show();
		Game.time.SlowMotion(preset.TimeScale, preset.Duration + preset.FocusDuration);
		Game.player.weapons.gameObject.SetActive(value: false);
		float timer2 = 0f;
		float fov2 = Camera.main.fieldOfView;
		if (preset.FocusDuration > 0f)
		{
			Game.sounds.PlaySound(Game.player.sfxFocusSwooshLight, 2);
			while (timer2 != preset.FocusDuration)
			{
				if (Time.timeScale != 0f && Game.player.isActiveAndEnabled)
				{
					timer2 = Mathf.MoveTowards(timer2, preset.FocusDuration, Time.unscaledDeltaTime);
					Game.player.mouseLook.LookInDirSmooth(Game.player.tHead.position.DirTo(tTarget.position + Offset), preset.LookSpeed, unscaled: true);
					if (preset.FocusFOV != 0f)
					{
						Camera.main.fieldOfView = Mathf.LerpUnclamped(fov2, preset.FocusFOV, preset.FocusCurve.Evaluate(timer2 / preset.FocusDuration));
					}
				}
				yield return null;
			}
		}
		if (preset.Duration > 0f)
		{
			Game.sounds.PlaySound(Game.player.sfxFocusSwoosh, 2);
			Vector3 startPos = Game.player.tHead.position;
			Vector3 endPos = tTarget.TransformPoint(targetLocalPos);
			Vector3 cachedVel = Game.player.rb.velocity;
			Game.player.SetKinematic(value: true);
			timer2 = 0f;
			fov2 = Camera.main.fieldOfView;
			while (timer2 != preset.Duration)
			{
				if (Time.timeScale != 0f && Game.player.isActiveAndEnabled)
				{
					timer2 = Mathf.MoveTowards(timer2, preset.Duration, Time.unscaledDeltaTime);
					Game.player.camController.Angle(preset.Curve.Evaluate(timer2 / preset.Duration) * 10f);
					Game.player.tHead.position = Vector3.LerpUnclamped(startPos, endPos, preset.Curve.Evaluate(timer2 / preset.Duration));
					Game.player.mouseLook.LookInDirSmooth(Game.player.tHead.position.DirTo(tTarget.position + Offset), preset.LookSpeed, unscaled: true);
					Camera.main.fieldOfView = Mathf.LerpUnclamped(fov2, preset.FOV, preset.FOVCurve.Evaluate(timer2 / preset.Duration));
				}
				yield return null;
			}
			Game.player.SetKinematic(value: false);
			Game.player.rb.velocity = cachedVel;
		}
		Game.player.Activate();
		Game.wideMode.Hide();
		Game.time.StopSlowmo();
		Game.player.weapons.gameObject.SetActive(value: true);
		ExitEvent.Invoke();
	}

	private void OnTriggerStay(Collider other)
	{
		if ((bool)tTarget && !other.attachedRigidbody.isKinematic)
		{
			StartCoroutine(LockingOnTarget());
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawIcon(base.transform.position, "CameraTrigger.png");
		if ((bool)tTarget)
		{
			Gizmos.DrawLine(base.transform.position, tTarget.position);
		}
	}
}
