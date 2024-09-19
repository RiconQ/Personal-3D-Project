using System;
using System.Collections;
using UnityEngine;

public class PortalDock : HubPortal
{
	public GameObject prefabLockedPath;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve curveSpawning = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AudioSource source;

	public AudioClip sfxBoatON;

	public AudioClip sfxBoatOFF;

	public AudioClip sfxBoared;

	public Transform tLocked;

	public Transform tOrb;

	public Transform tBoat;

	public Transform tPivot;

	private float boatFade;

	private Vector3 boatPos;

	private Vector3 boatRot;

	private MeshRenderer[] boatRends;

	private MaterialPropertyBlock boatBlock;

	private MeshRenderer orbRend;

	private MaterialPropertyBlock orbBlock;

	public override void Setup()
	{
		base.Setup();
		boatPos.z = -15f;
		boatPos.x = 0f;
		boatPos.y = -0.5f;
		tBoat.localPosition = boatPos;
		boatBlock = new MaterialPropertyBlock();
		boatRends = tBoat.GetComponentsInChildren<MeshRenderer>();
		boatRends[0].GetPropertyBlock(boatBlock);
		orbBlock = new MaterialPropertyBlock();
		orbRend = tOrb.GetComponent<MeshRenderer>();
		orbRend.GetPropertyBlock(orbBlock);
	}

	public override void SpawnPlayer()
	{
		base.SpawnPlayer();
		StartCoroutine(Spawning());
	}

	public override void Check()
	{
		base.Check();
		int num = LevelsData.currentHub.levels.IndexOf(data);
		if (num > 1 && LevelsData.currentHub.levels[num - 1].results.time == 0f && !isLocked)
		{
			isLocked = true;
		}
		if (isLocked)
		{
			source.volume = 0f;
			boatBlock.SetFloat("_Fade", 1f);
			MeshRenderer[] array = boatRends;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetPropertyBlock(boatBlock);
			}
			tOrb.gameObject.SetActive(value: false);
		}
		bool flag = data.results.time == 0f;
		orbBlock.SetFloat("_AlphaPower", (!flag) ? 1 : 0);
		orbRend.SetPropertyBlock(orbBlock);
		ui.CheckIcon();
	}

	public override void Toggle(bool value)
	{
		base.Toggle(value);
		if (!isLocked)
		{
			tBoat.GetComponentInChildren<ParticleSystem>().Play();
			if (value)
			{
				Game.sounds.PlayClip(sfxBoatON, 0.9f);
			}
			else
			{
				Game.sounds.PlayClip(sfxBoatOFF, 0.9f);
			}
		}
	}

	public override void OnTriggerEnter(Collider other)
	{
		if (Game.player.inputActive && !isLocked)
		{
			Hub.lastPortal = this;
			StartCoroutine(EnteringBoat());
		}
	}

	private void Update()
	{
		if (!isLocked)
		{
			source.volume = Mathf.Lerp(0f, 0.9f, boatFade);
			boatFade = Mathf.MoveTowards(boatFade, isFocused ? 1 : 0, Time.deltaTime);
			boatBlock.SetFloat("_Fade", 1f - boatFade * boatFade);
			MeshRenderer[] array = boatRends;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetPropertyBlock(boatBlock);
			}
			float time = Time.time;
			boatPos.z = Mathf.Lerp(boatPos.z, isFocused ? (-10) : (-15), Time.deltaTime);
			boatPos.x = 0f;
			boatPos.y = -0.5f + Mathf.Sin(time / 2f) * 0.5f;
			tBoat.localPosition = boatPos;
			boatRot.x = Mathf.Sin(time / 2f) * 4f;
			boatRot.y = 180f;
			boatRot.z = Mathf.Sin(time) * 7.5f;
			tBoat.localEulerAngles = boatRot;
		}
	}

	private IEnumerator EnteringBoat()
	{
		Game.player.SetKinematic(value: true);
		Game.player.Deactivate();
		Game.wideMode.Show();
		Game.sounds.PlayClip(sfxBoared);
		Vector3 startPos = Game.player.t.position;
		float timer = 0f;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
			Vector3 position = Vector3.LerpUnclamped(startPos, tPivot.position, curve.Evaluate(timer));
			position.y += Mathf.Sin(curve.Evaluate(timer) * (float)Math.PI) * 0.5f;
			Game.player.camController.Angle(Mathf.Sin(timer * (float)Math.PI) * 10f);
			Game.player.t.position = position;
			Game.player.mouseLook.LookInDirSmooth(tPivot.forward);
			yield return null;
		}
		Game.fading.InstantFade(1f);
		Game.instance.LoadLevel(data.sceneReference.ScenePath, quickLoad);
	}

	private IEnumerator Spawning()
	{
		Game.player.t.position = tPivot.position;
		Game.player.SetKinematic(value: true);
		Game.player.Deactivate();
		Game.fading.InstantFade(1f);
		Game.fading.speed = 0.3f;
		Game.wideMode.Set(1f);
		if (Hub.instance.progress == 1 && !Hub.instance.dontShowProgressCutscene && PlayerPrefs.GetInt($"{Hub.lastHub}_progresscutscene") != 1)
		{
			PlayerPrefs.SetInt($"{Hub.lastHub}_progresscutscene", 1);
			Game.player.fov.kinematicFOV = 20f;
			HubPortal p = Hub.instance.GetHubPortal();
			Game.player.tHead.position = p.t.position + Vector3.up + p.t.forward * 6f;
			Game.player.mouseLook.LookAt(p.tMesh.position);
			yield return new WaitForEndOfFrame();
			Game.fading.Fade(0f);
			float timer = 0f;
			while (timer != 3f)
			{
				Game.player.tHead.Translate(-p.t.forward * (Time.deltaTime * 0.5f), Space.World);
				timer = Mathf.MoveTowards(timer, 3f, Time.deltaTime);
				yield return null;
			}
			Game.fading.InstantFade(1f);
			yield return new WaitForSeconds(0.3f);
			Game.player.tHead.localPosition = new Vector3(0f, 0.75f, 0f);
			Game.fading.Fade(0f);
		}
		Game.player.fov.AddToFOV(-30f);
		Game.player.t.position = tPivot.position;
		Game.player.SetKinematic(value: false);
		Game.player.rb.AddBallisticForce(base.t.position, 1f, -40f, resetVelocity: true);
		Game.player.mouseLook.LookInDir(base.t.forward);
		CameraController.shake.Shake(1);
		while (!Game.player.grounder.grounded)
		{
			Camera.main.fieldOfView = Mathf.LerpUnclamped(Camera.main.fieldOfView, 90f, Time.deltaTime * 4f);
			yield return null;
		}
		Game.player.Activate();
		Game.wideMode.Hide();
		CameraController.shake.Shake(1);
	}
}
