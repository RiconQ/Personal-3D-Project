using System;
using System.Collections;
using UnityEngine;

public class ArrivalSpawnPoint : SavePoint
{
	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve curveB = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private Quaternion rotA;

	private Quaternion rotB;

	private bool triggered;

	protected override void Reset()
	{
		base.Reset();
		base.Launch();
	}

	public override void Launch()
	{
		if (triggered)
		{
			base.Launch();
		}
		else
		{
			StartCoroutine(Launching());
		}
	}

	private IEnumerator Launching()
	{
		triggered = true;
		Game.fading.Fade(0f);
		Game.wideMode.Show();
		Game.player.rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		Game.player.rb.isKinematic = true;
		Game.player.fov.kinematicFOV = 0f;
		Game.player.Deactivate();
		yield return null;
		Game.player.t.position = base.t.position + Vector3.up * 100f;
		Game.player.mouseLook.LookInDir(Vector3.up);
		rotA = Quaternion.LookRotation(Vector3.up);
		rotB = Quaternion.LookRotation(Vector3.down);
		float timer2 = 0f;
		while (Game.player.t.position != base.t.position)
		{
			timer2 = Mathf.MoveTowards(timer2, 1f, Time.deltaTime * 0.25f);
			Quaternion rotation = Quaternion.SlerpUnclamped(rotA, rotB, curve.Evaluate(timer2));
			Game.player.mouseLook.SetRotation(rotation);
			Game.player.camController.Angle(Mathf.LerpAngle(180f, 0f, curve.Evaluate(timer2)));
			Game.player.t.position = Vector3.MoveTowards(Game.player.t.position, base.t.position, timer2);
			yield return null;
		}
		CameraController.shake.Shake();
		CameraController.shake.Shake(2);
		QuickEffectsPool.Get("HardLanding", base.t.position, Quaternion.LookRotation(base.t.forward)).Play();
		timer2 = 0f;
		rotA = Game.player.tHead.rotation;
		rotB = Quaternion.LookRotation(base.t.forward);
		while (timer2 != 1f)
		{
			timer2 = Mathf.MoveTowards(timer2, 1f, Time.deltaTime);
			Quaternion rotation = Quaternion.SlerpUnclamped(rotA, rotB, curveB.Evaluate(timer2));
			Game.player.t.position = base.t.position + Vector3.up * (0f - Mathf.Sin(timer2 * ((float)Math.PI * 3f)) * (1f - timer2));
			Game.player.mouseLook.SetRotation(rotation);
			Game.player.camController.Angle(Mathf.Sin(timer2 * (float)Math.PI) * -10f * (1f - timer2));
			yield return null;
		}
		Game.player.rb.isKinematic = false;
		Game.player.Activate();
		Game.player.grounder.Grounded(forced: true);
		Game.wideMode.Hide();
	}
}
