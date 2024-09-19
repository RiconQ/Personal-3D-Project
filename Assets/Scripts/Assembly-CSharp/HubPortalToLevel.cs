using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubPortalToLevel : HubPortal
{
	public GameObject objEffect;

	public GameObject objLockedEffect;

	public GameObject objGlow;

	public List<HubPortal> lockedBy = new List<HubPortal>();

	public AudioSource Source;

	public LevelProgress progressOrb;

	public ParticleSystem LockedParticle;

	public ParticleSystem OpenedParticle;

	public AnimationCurve curve;

	public AudioClip clip;

	public Transform tBoat;

	public Transform tPivot;

	public Color LockedColor = Color.red;

	public Color OpenedColor = Color.green;

	public MeshRenderer Rend;

	public GameObject LockedPathPrefab;

	private Vector3 boatPos;

	private Vector3 boatRot;

	private MaterialPropertyBlock _block;

	public override void Setup()
	{
		base.Setup();
		if ((bool)tBoat)
		{
			tBoat.gameObject.SetActive(value: false);
		}
	}

	public override void OnTriggerEnter(Collider other)
	{
		if (Game.player.inputActive && !isLocked)
		{
			Hub.lastPortal = this;
			if (!tBoat)
			{
				Game.fading.InstantFade(1f);
				Game.instance.LoadLevel(data.sceneReference.ScenePath, quickLoad);
			}
			else
			{
				StartCoroutine(Entering());
			}
		}
	}

	public override void Check()
	{
		base.Check();
		if (lockedBy.Count > 0)
		{
			foreach (HubPortal item in lockedBy)
			{
				if (item.data.results.time == 0f)
				{
					if (!isLocked)
					{
						isLocked = true;
					}
					GameObject obj = UnityEngine.Object.Instantiate(LockedPathPrefab, objLockedEffect.transform);
					obj.GetComponent<LockedPortalPath>().Setup(base.transform.position, item.transform.position);
					obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
				}
			}
		}
		objGlow.SetActive(!isLocked);
		_block = new MaterialPropertyBlock();
		Rend.GetPropertyBlock(_block);
		_block.SetColor("_TintColor", isLocked ? LockedColor : OpenedColor);
		Rend.SetPropertyBlock(_block);
		if (isLocked)
		{
			progressOrb.gameObject.SetActive(value: false);
		}
		else
		{
			progressOrb.Set((data.results.time != 0f) ? 1 : 0);
		}
		ui.CheckIcon();
	}

	public override void Toggle(bool value)
	{
		base.Toggle(value);
		if (isLocked)
		{
			if (value)
			{
				objLockedEffect.GetComponentInChildren<ParticleSystem>().Play();
			}
			else
			{
				objLockedEffect.GetComponentInChildren<ParticleSystem>().Stop();
			}
		}
		else if (value)
		{
			Source.Play();
			OpenedParticle.Play();
		}
		else
		{
			Source.Stop();
			OpenedParticle.Stop();
		}
		if ((bool)tBoat)
		{
			tBoat.gameObject.SetActive(value);
			tBoat.localPosition = (boatPos = new Vector3(0f, 0f, -15f));
		}
		if (value && !tBoat)
		{
			QuickEffectsPool.Get("Portal Enable", base.t.position).Play();
		}
	}

	public override void SpawnPlayer()
	{
		base.SpawnPlayer();
		StartCoroutine(Spawning());
	}

	private void Update()
	{
		if (!isLocked)
		{
			progressOrb.t.localPosition = new Vector3(0f, 2.5f + Mathf.Sin(Time.time) * 0.5f, 0f);
		}
		if ((bool)tBoat)
		{
			boatPos.z = Mathf.Lerp(boatPos.z, -10f, Time.deltaTime);
			boatPos.x = 0f;
			boatPos.y = Mathf.Sin(Time.time) * 0.5f;
			tBoat.localPosition = boatPos;
			boatRot.x = Mathf.Sin(Time.time) * 5f;
			boatRot.y = 180f;
			boatRot.z = Mathf.Sin(Time.time * 2f) * 10f;
			tBoat.localEulerAngles = boatRot;
		}
	}

	private IEnumerator Entering()
	{
		Game.sounds.PlayClip(clip);
		Game.player.SetKinematic(value: true);
		Game.player.Deactivate();
		Game.wideMode.Show();
		Game.player.mouseLook.enabled = false;
		Vector3 aPos = Game.player.t.position;
		float timer = 0f;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
			Vector3 position = Vector3.LerpUnclamped(aPos, tPivot.position, curve.Evaluate(timer));
			position.y += Mathf.Sin(curve.Evaluate(timer) * (float)Math.PI) * 1.5f;
			Game.player.camController.Angle(Mathf.Sin(timer * (float)Math.PI) * 10f);
			Game.player.t.position = position;
			Game.player.mouseLook.LookInDirSmooth(tPivot.forward);
			yield return null;
		}
		Debug.Log("DJD");
		Game.fading.InstantFade(1f);
		Game.instance.LoadLevel(data.sceneReference.ScenePath, quickLoad);
	}

	private IEnumerator Spawning()
	{
		Game.player.t.position = base.t.position + base.t.forward * 2f + Vector3.up * 2f;
		Game.fading.InstantFade(1f);
		Game.wideMode.Set(1f);
		Game.player.Deactivate();
		Game.player.SetKinematic(value: true);
		Game.fading.speed = 0.3f;
		if (Hub.instance.progress == 1 && PlayerPrefs.GetInt($"{Hub.lastHub}_progresscutscene") != 1)
		{
			PlayerPrefs.SetInt($"{Hub.lastHub}_progresscutscene", 1);
			Game.player.fov.kinematicFOV = 20f;
			Game.player.t.position = Hub.instance.t.position;
			Game.player.mouseLook.LookInDir(Hub.instance.t.forward);
			yield return new WaitForEndOfFrame();
			Game.fading.Fade(0f);
			float timer = 0f;
			while (timer != 3f)
			{
				timer = Mathf.MoveTowards(timer, 3f, Time.deltaTime);
				Game.player.t.Translate(Hub.instance.t.forward * (0f - Time.deltaTime));
				yield return null;
			}
			Game.fading.InstantFade(1f);
			yield return new WaitForSeconds(0.3f);
			Game.player.t.position = base.t.position + base.t.forward * 2f + Vector3.up * 2f;
		}
		yield return new WaitForEndOfFrame();
		Game.player.SetKinematic(value: false);
		Game.fading.Fade(0f);
		Game.player.fov.AddToFOV(-30f);
		Game.player.rb.AddBallisticForce(base.t.position + base.t.forward * 6f + Vector3.up, 1f, -40f);
		CameraController.shake.Shake(1);
		while (!Game.player.grounder.grounded)
		{
			Game.player.mouseLook.LookInDir(Game.player.tHead.position.DirTo(base.t.position + Vector3.up * 2f));
			yield return null;
		}
		Game.player.Activate();
		Game.wideMode.Hide();
		CameraController.shake.Shake(2);
	}
}
