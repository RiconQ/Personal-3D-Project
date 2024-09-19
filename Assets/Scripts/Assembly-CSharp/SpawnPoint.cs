using System;
using System.Collections;
using UnityEngine;

public class SpawnPoint : SavePoint
{
	public static Action OnLand = delegate
	{
	};

	private bool activated;

	private float timer;

	private float duration;

	public bool startMissionOnLand = true;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public bool slowSpawn;

	public Vector3 startLookDir = Vector3.up;

	private PlayerController player;

	private bool spawned;

	private Vector3 startPos;

	private Vector3 targetPos;

	private Vector3 lookAtPos;

	private Vector3 startDir;

	private Vector3 dir;

	public void Setup()
	{
		base.gameObject.layer = 12;
		base.gameObject.tag = "Entrance";
		base.transform.SetAsFirstSibling();
	}

	protected override void Reset()
	{
		base.Reset();
		activated = false;
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: true);
		}
		Spawn();
	}

	public override void Spawn()
	{
		if (!base.t)
		{
			base.t = base.transform;
		}
		if (!player)
		{
			player = PlayerController.instance;
		}
		if (!slowSpawn)
		{
			StartCoroutine(Spawning());
		}
		else
		{
			StartCoroutine(Spawning2());
		}
	}

	private IEnumerator Spawning2()
	{
		Game.wideMode.Set(1f);
		Game.fading.InstantFade(1f);
		yield return new WaitForEndOfFrame();
		Game.fading.Fade(0f);
		activated = true;
		Vector3 aDir = startLookDir;
		Vector3 bDir = base.t.forward;
		player.t.position = base.t.position;
		player.Deactivate();
		float timer = 0f;
		while (timer != 1f)
		{
			Vector3 vector = Vector3.LerpUnclamped(aDir, bDir, curve.Evaluate(timer));
			Game.player.mouseLook.LookInDir(vector);
			Game.player.camController.Angle(Mathf.Sin(timer * (float)Math.PI) * 15f);
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 0.25f);
			Game.player.fov.cam.fieldOfView = Mathf.LerpUnclamped(40f, 90f, curve.Evaluate(timer));
			yield return null;
		}
		player.Activate();
		Game.wideMode.Hide();
	}

	private IEnumerator Spawning()
	{
		if (!Application.isPlaying)
		{
			yield break;
		}
		Game.wideMode.Set(1f);
		activated = true;
		startPos = base.t.position - base.t.forward * (((bool)SpawnBoat.instance & !spawned) ? 75 : 45);
		startPos.y -= 6f;
		targetPos = (lookAtPos = base.t.position);
		duration = 1.75f;
		player.SetKinematic(value: true);
		player.Deactivate();
		startDir = startPos.DirTo(targetPos);
		Debug.DrawLine(targetPos, startPos, Color.yellow, 3f);
		player.grounder.Ungrounded();
		player.t.position = startPos;
		player.mouseLook.LookInDir(Vector3.down + Vector3.Cross(startDir, Vector3.up) * UnityEngine.Random.Range(-0.1f, 0.1f));
		if ((bool)SpawnBoat.instance && !spawned)
		{
			SpawnBoat.instance.SetPosAndRot(startPos, base.t.position);
			Game.sounds.PlayClip(SpawnBoat.instance.sfxPaddling1);
			timer = 0f;
			while (timer != 1f)
			{
				Game.player.camController.Angle(SpawnBoat.instance.curve.Evaluate(timer) * 10f);
				Game.player.t.position = SpawnBoat.instance.tPivot.position;
				player.mouseLook.LookAt(Vector3.LerpUnclamped(base.t.position - Vector3.up * 20f, base.t.position + Vector3.up * 30f, SpawnBoat.instance.curve.Evaluate(timer)));
				player.camController.worldCam.fieldOfView = Mathf.LerpUnclamped(90f, 100f, SpawnBoat.instance.curve.Evaluate(timer));
				timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 0.25f);
				yield return null;
			}
			SpawnBoat.instance.targetSpeed = 0f;
			SpawnBoat.instance.targetHeight = -10f;
			Game.sounds.PlayClip(SpawnBoat.instance.sfxPaddling2);
			spawned = true;
		}
		Game.fading.Fade(0f);
		Game.sounds.PlayClipAtPosition(Game.player.sfxSpawnSwoosh, 1f, Game.player.t.position);
		player.SetKinematic(value: false);
		player.rb.AddBallisticForce(targetPos, duration, -40f);
		CameraController.shake.Shake(1);
		timer = 0f;
		while (!player.grounder.grounded)
		{
			timer = Mathf.MoveTowards(timer, duration, Time.deltaTime);
			dir = Vector3.LerpUnclamped(player.tHead.position.DirTo(lookAtPos), (startDir + Vector3.up / 3f).normalized, timer / duration);
			player.mouseLook.LookInDirSmooth(dir);
			yield return null;
		}
		if (startMissionOnLand)
		{
			Game.mission.SetState(1);
		}
		Deactivate();
	}

	private void Deactivate()
	{
		player.mouseLook.enabled = true;
		player.Activate();
		CameraController.shake.Shake();
		QuickEffectsPool.Get("HardLanding", player.grounder.gPoint, Quaternion.LookRotation(base.t.forward)).Play();
		Game.time.SetDefaultTimeScale(1f);
		Game.wideMode.Hide();
		if (OnLand != null)
		{
			OnLand();
		}
	}
}
