using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionCube : PullableTarget
{
	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private Transform t;

	private PullablePoint[] points;

	private Vector3[] dirs;

	private Collider clldr;

	private Vector3 aPos;

	private Vector3 bPos;

	private Vector3 startPos;

	private Quaternion startRot;

	private List<Collider> clldrs = new List<Collider>(5);

	private new void Awake()
	{
		t = base.transform;
		clldr = GetComponent<Collider>();
		points = GetComponentsInChildren<PullablePoint>();
		dirs = new Vector3[points.Length];
		startPos = t.position;
		startRot = t.rotation;
		Physics.Raycast(t.position, -t.up, out var hitInfo, 50f, 1);
		if (hitInfo.distance != 0f)
		{
			bPos = hitInfo.point + hitInfo.normal * 2f;
		}
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private new void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private new void Reset()
	{
		clldrs.Clear();
		t.SetPositionAndRotation(startPos, startRot);
		GetComponentInChildren<PullablePoint>(includeInactive: true).gameObject.SetActive(value: true);
	}

	public override void Pull()
	{
		aPos = t.position;
		StartCoroutine(Sliding());
		GetComponentInChildren<PullablePoint>(includeInactive: true).gameObject.SetActive(value: false);
	}

	private IEnumerator Sliding()
	{
		clldr.isTrigger = true;
		float timer = 0f;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 4f);
			t.position = Vector3.LerpUnclamped(aPos, bPos, curve.Evaluate(timer));
			yield return null;
		}
		QuickEffectsPool.Get("HardLanding", t.position, Quaternion.identity).Play();
		Game.player.sway.Sway(3f, 0f, 0f, 4f);
		CameraController.shake.Shake(1);
		clldr.isTrigger = false;
	}

	private void OnTriggerEnter(Collider c)
	{
		if (!clldrs.Contains(c))
		{
			clldrs.Add(c);
			if (c.gameObject.layer == 10)
			{
				Debug.Log("DJJD");
				DamageData damageData = new DamageData();
				damageData.knockdown = true;
				damageData.amount = 200f;
				damageData.newType = Game.style.basicMill;
				c.GetComponent<IDamageable<DamageData>>().Damage(damageData);
			}
		}
	}
}
