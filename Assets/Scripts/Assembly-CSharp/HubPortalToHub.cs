using System.Collections;
using UnityEngine;

public class HubPortalToHub : HubPortal
{
	public HubData hub;

	public bool LockedByLevels;

	public HubData lockedByHub;

	public new Transform tMesh;

	public Collider trigger;

	public ParticleSystem lockedParticle;

	public Color lockedColor = Color.black;

	public Color unlockedColor = Color.red;

	public GameObject objParticle;

	public MeshRenderer rend;

	private MaterialPropertyBlock block;

	public AudioClip Sound;

	public AnimationCurve Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private Vector3 angles;

	private Vector3 pos;

	public override void Setup()
	{
		base.Setup();
	}

	public override void Check()
	{
		base.Check();
		if ((bool)lockedByHub)
		{
			lockedByHub.CheckProgress();
			isLocked = lockedByHub.ProgressByTime != 1f;
			Debug.Log(lockedByHub.ProgressByTime);
			objParticle.SetActive(!isLocked);
			block = new MaterialPropertyBlock();
			rend.GetPropertyBlock(block);
			block.SetColor("_RimColor", isLocked ? lockedColor : unlockedColor);
			rend.SetPropertyBlock(block);
		}
		else if (LockedByLevels)
		{
			hub = LevelsData.instance.GetCurrentHub();
			if ((bool)hub)
			{
				hub.CheckProgress();
				isLocked = hub.ProgressByTime != 1f;
				objParticle.SetActive(!isLocked);
				block = new MaterialPropertyBlock();
				rend.GetPropertyBlock(block);
				block.SetColor("_RimColor", isLocked ? lockedColor : unlockedColor);
				rend.SetPropertyBlock(block);
			}
		}
	}

	public override void SpawnPlayer()
	{
		Game.player.SetKinematic(value: true);
		Game.player.t.position = tMesh.position - base.t.forward * 40f;
		Game.player.Deactivate();
		Game.wideMode.Set(1f);
		base.SpawnPlayer();
		StartCoroutine(Spawning());
	}

	public override void OnTriggerEnter(Collider other)
	{
		base.OnTriggerEnter(other);
		if (isLocked)
		{
			lockedParticle.Play();
			lockedParticle.GetComponent<AudioSource>().Play();
			if (Game.player.grounder.grounded)
			{
				Game.player.grounder.Ungrounded(forced: true);
			}
			Game.player.slide.Interrupt();
			Game.player.airControlBlock = 0.2f;
			Game.player.rb.velocity = (base.t.forward + Vector3.up).normalized * 20f;
			CameraController.shake.Shake();
		}
	}

	private void Update()
	{
		angles.x = -90f + (Mathf.PerlinNoise(tMesh.position.x, Time.time / 4f) - 0.5f) * 5f;
		angles.y = (Mathf.PerlinNoise(Time.time / 2f, tMesh.position.y) - 0.5f) * 5f;
		angles.z = (Mathf.PerlinNoise(tMesh.position.z, Time.time / 8f) - 0.5f) * 5f;
		tMesh.localEulerAngles = angles;
		pos.x = 0f;
		pos.z = -6f;
		pos.y = 6f + Mathf.Sin(Time.time) * 0.5f;
		tMesh.localPosition = pos;
	}

	private IEnumerator Spawning()
	{
		yield return new WaitForEndOfFrame();
		trigger.enabled = false;
		Game.sounds.PlayClipAtPosition(Game.player.sfxSpawnSwoosh, 1f, Game.player.t.position);
		Game.player.mouseLook.LookInDir(base.t.forward);
		Game.player.fov.kinematicFOV = 0f;
		Vector3 posA = tMesh.position - base.t.forward * 40f;
		Vector3 posB = tMesh.position + base.t.forward * 4f;
		float fov = Camera.main.fieldOfView;
		float timer = 0f;
		bool swooshPlayed = false;
		bool shakePlayed = false;
		while (timer != 1f)
		{
			timer.MoveTowards(1f);
			Game.player.t.position = Vector3.LerpUnclamped(posA, posB, timer);
			Game.player.camController.Angle(10f * (1f - timer), 0f);
			Camera.main.fieldOfView = Mathf.LerpUnclamped(120f, fov, Curve.Evaluate(timer));
			if (!swooshPlayed && timer > 0.7f)
			{
				Game.sounds.PlaySound(Sound, 0);
				swooshPlayed = true;
			}
			if (!shakePlayed && timer > 0.8f)
			{
				CameraController.shake.Shake();
				shakePlayed = true;
			}
			yield return null;
		}
		Game.player.Activate();
		Game.player.SetKinematic(value: false);
		Game.player.rb.velocity = base.t.forward * 25f;
		Game.player.airControlBlock = 0.1f;
		Game.wideMode.Hide();
		while (!Game.player.grounder.grounded)
		{
			yield return null;
		}
		trigger.enabled = true;
	}
}
