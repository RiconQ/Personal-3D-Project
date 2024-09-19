using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakPillar : MonoBehaviour, ITriggerable
{
	public Transform t;

	public float height = 2f;

	public float angle = -90f;

	public Vector3 rotationAxis = new Vector3(1f, 0f, 0f);

	public float speed = 1f;

	public bool disableOnHit = true;

	public Vector3 startPos;

	public Vector3 targetPos;

	public Quaternion startRot;

	public Quaternion targetRot;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private bool isFalling;

	private List<Collider> clldrs = new List<Collider>(10);

	private MeshRenderer rend;

	public void Trigger()
	{
		StartCoroutine(Falling());
	}

	private void Awake()
	{
		rend = GetComponent<MeshRenderer>();
		startPos = t.localPosition;
		startRot = t.localRotation;
		if (angle != 0f)
		{
			targetRot = Quaternion.AngleAxis(angle, rotationAxis) * startRot;
		}
		else
		{
			targetRot = startRot;
		}
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		t.localPosition = startPos;
		t.localRotation = startRot;
		StopAllCoroutines();
		base.gameObject.SetActive(value: true);
	}

	private IEnumerator Falling()
	{
		float timer = 0f;
		CameraController.shake.Shake(1);
		QuickEffectsPool.Get("Wooden Debris B", t.position).Play(25f);
		clldrs.Clear();
		isFalling = true;
		while (t.localPosition != targetPos)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
			Vector3 localPosition = Vector3.LerpUnclamped(startPos, targetPos, curve.Evaluate(timer));
			localPosition.z += Mathf.Sin(timer * (float)Math.PI) * height;
			Quaternion localRotation = Quaternion.SlerpUnclamped(startRot, targetRot, curve.Evaluate(timer));
			t.localPosition = localPosition;
			t.localRotation = localRotation;
			yield return null;
		}
		isFalling = false;
		yield return new WaitForFixedUpdate();
		QuickEffectsPool.Get("Rocks", rend.bounds.center, Quaternion.identity).Play();
		CameraController.shake.Shake();
		if (disableOnHit)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void HandleCollider(Collider c)
	{
		if (c.gameObject.layer != 10 || !isFalling)
		{
			return;
		}
		switch (c.gameObject.layer)
		{
		case 10:
			if (!clldrs.Contains(c))
			{
				DamageData damageData = new DamageData();
				damageData.dir = t.right;
				damageData.newType = Game.style.basicBluntHit;
				damageData.knockdown = true;
				damageData.amount = 200f;
				c.GetComponent<IDamageable<DamageData>>().Damage(damageData);
				StyleRanking.instance.AddStylePoint(StylePointTypes.CrushedStones);
				clldrs.Add(c);
			}
			break;
		case 14:
			if (!clldrs.Contains(c))
			{
				c.GetComponent<IKickable<Vector3>>().Kick(t.root.right);
				clldrs.Add(c);
			}
			break;
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		HandleCollider(c);
	}

	private void OnTriggerStay(Collider c)
	{
		HandleCollider(c);
	}
}
