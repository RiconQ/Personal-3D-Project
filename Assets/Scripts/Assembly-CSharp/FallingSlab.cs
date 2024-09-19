using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSlab : MonoBehaviour
{
	public bool vert;

	public float speed = 1f;

	public GameObject breakable;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AudioClip sfxCrashed;

	private Transform t;

	private Collider clldr;

	private float xAngle;

	private float timer;

	private RaycastHit hit;

	private bool isFalling;

	private List<Collider> clldrs = new List<Collider>(10);

	private float yPos;

	private void Awake()
	{
		t = base.transform;
		clldr = GetComponentInChildren<Collider>();
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Combine(BreakableB.OnBreak, new Action<GameObject>(OnBreak));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		if (vert)
		{
			Physics.Raycast(t.position, Vector3.down, out var hitInfo, 10f, 1);
			yPos = hitInfo.point.y;
		}
	}

	private void OnDestroy()
	{
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Remove(BreakableB.OnBreak, new Action<GameObject>(OnBreak));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnBreak(GameObject obj)
	{
		if (obj == breakable)
		{
			StartCoroutine(Falling());
		}
	}

	private void Reset()
	{
		clldrs.Clear();
		base.gameObject.SetActive(value: true);
		t.localEulerAngles = new Vector3(-90f, 90f, -90f);
		if (vert)
		{
			t.localPosition = t.localPosition.With(null, 4f);
		}
		StopAllCoroutines();
		isFalling = false;
	}

	private IEnumerator Falling()
	{
		xAngle = -90f;
		timer = 0f;
		isFalling = true;
		float yTarget = 0f;
		Physics.Raycast(t.parent.position, Vector3.down, out hit, 62f, 1);
		if (hit.distance != 0f)
		{
			yTarget = t.parent.InverseTransformPoint(hit.point).y;
			Debug.DrawLine(t.position, hit.point, Color.red, 2f);
		}
		float speed = -5f;
		yPos = 4f;
		while (timer != 1f && yPos != yTarget)
		{
			if (!vert)
			{
				timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
				xAngle = Mathf.LerpUnclamped(-90f, 0f, curve.Evaluate(timer));
				t.localEulerAngles = new Vector3(xAngle, 90f, -90f);
			}
			else
			{
				speed = Mathf.Lerp(speed, 40f, Time.deltaTime * 2f);
				yPos = Mathf.MoveTowards(yPos, yTarget, speed * Time.deltaTime);
				t.localPosition = t.localPosition.With(null, yPos);
			}
			yield return null;
		}
		isFalling = false;
		yield return new WaitForFixedUpdate();
		new DamageData
		{
			dir = t.forward,
			amount = 200f,
			newType = Game.style.basicMill
		};
		CameraController.shake.Shake(2);
	}

	private void OnCollisionEnter(Collision c)
	{
		HandleCollision(c);
	}

	private void OnCollisionStay(Collision c)
	{
		HandleCollision(c);
	}

	private void HandleCollision(Collision c)
	{
		if (c.gameObject.layer == 10 && isFalling && !clldrs.Contains(c.collider))
		{
			DamageData damageData = new DamageData();
			damageData.dir = t.root.right;
			damageData.newType = Game.style.basicMill;
			damageData.knockdown = true;
			damageData.amount = 200f;
			clldrs.Add(c.collider);
			c.collider.GetComponent<IDamageable<DamageData>>().Damage(damageData);
			StyleRanking.instance.AddStylePoint(StylePointTypes.CrushedStones);
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
				damageData.dir = t.root.right;
				damageData.knockdown = true;
				damageData.amount = 200f;
				damageData.newType = Game.style.basicMill;
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
