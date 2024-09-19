using System;
using UnityEngine;

[SelectionBase]
public class Zipline : SetupableMonobehavior, IPlatformable
{
	public CapsuleCollider clldr;

	public ChainRenderer chainMesh;

	public ChainSkinnedRenderer rend;

	public LineRenderer lineRend;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Transform pointA;

	public Transform pointB;

	public Transform t;

	public Transform tMesh;

	public Vector3 target;

	public Vector3 posA;

	public Vector3 posB;

	public Vector3[] poses;

	private int count;

	private int sign;

	private float dist;

	private float timer;

	private float speed;

	public AnimationCurve mgtCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float mgt;

	public float mgtTimer;

	public AudioClip sfxGrab;

	public AudioClip sfxDrop;

	private Vector3 off;

	private Vector3 offset;

	private Vector3 pos;

	private Vector3 startPos;

	private Vector3 temp;

	private PooledEffect effect;

	private void Awake()
	{
		t = base.transform;
		dist = Vector3.Distance(posA, posB);
		count = Mathf.RoundToInt(dist);
		poses = new Vector3[count];
		for (int i = 0; i < count; i++)
		{
			poses[i] = new Vector3(0f, 0f, i);
		}
		rend.GenerateChainMesh(poses);
		rend.transform.SetPositionAndRotation(posA, Quaternion.LookRotation(posB - posA, Vector3.up));
		clldr = GetComponentInChildren<CapsuleCollider>();
		clldr.center = new Vector3(0f, -0.5f, dist / 2f);
		clldr.height = dist - 2f;
		clldr.radius = 1f;
	}

	public void Setup()
	{
		t = base.transform;
		lineRend = GetComponentInChildren<LineRenderer>();
		clldr = GetComponentInChildren<CapsuleCollider>();
		clldr.isTrigger = true;
		curve = new AnimationCurve();
		curve.AddKey(new Keyframe(0f, 0f, 0f, 12f));
		curve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
		base.gameObject.layer = 12;
	}

	public override void SetTargetPosition(Vector3 worldPos)
	{
		target = worldPos;
		UpdateLine();
	}

	public override Vector3 GetWorldTargetPosition()
	{
		return target;
	}

	public void UpdateLine()
	{
		float num = Vector3.Distance(posA, posB);
		clldr.center = new Vector3(0f, (0f - num) / 2f, 0f);
		clldr.height = num - 4f;
		clldr.radius = 0.8f;
		if (!QuickmapScene.instance)
		{
			chainMesh.transform.SetPositionAndRotation(posA, Quaternion.FromToRotation(Vector3.up, posA - posB));
			chainMesh.GenerateChainMesh(posA, posB);
			lineRend.enabled = false;
		}
		else
		{
			lineRend.enabled = true;
			lineRend.SetPosition(0, posA);
			lineRend.SetPosition(1, posB);
		}
	}

	private void OnTriggerStay()
	{
		Grab();
	}

	private void OnTriggerEnter()
	{
		Grab();
	}

	public void Grab()
	{
		if ((Game.player.JumpHolded() || PlayerController.gamepad) && Game.player.Grab(this))
		{
			speed = Mathf.Clamp(Game.player.oldVel.With(null, 0f).magnitude, 15f, 25f);
			startPos = Game.player.t.position;
			pos = Game.player.t.position.ClosestPointOnLine(posA, posB);
			offset = -(pos - startPos);
			sign = Vector3.Dot(Game.player.tHead.forward, posB - posA).Sign();
			mgtTimer = 0f;
			timer = 0f;
			effect = QuickEffectsPool.Get("Zipline FX");
			effect.gameObject.SetActive(value: true);
			effect.Play();
			CameraController.shake.Shake(1);
			Game.player.sway.Sway(-2.5f, 0f, 5f, 2f);
			Game.sounds.PlayClip(sfxGrab);
		}
	}

	private void Update()
	{
		for (int i = 0; i < poses.Length; i++)
		{
			float num = (float)i / dist;
			poses[i].z = i;
			poses[i].y = YSway(num);
			poses[i].x = XSway(num);
		}
	}

	public void LateUpdate()
	{
		for (int i = 0; i < rend.tBones.Length; i++)
		{
			float z;
			if (i != rend.tBones.Length - 1)
			{
				rend.tBones[i].LookAt(rend.tBones[i + 1], rend.transform.up);
				z = Vector3.Distance(poses[i], poses[i + 1]);
			}
			else
			{
				rend.tBones[i].LookAt(posB, rend.transform.up);
				z = 1f;
			}
			rend.tBones[i].localScale = new Vector3(1f, 1f, z);
			rend.tBones[i].localPosition = poses[i];
		}
	}

	public static float GetPercentageAlong(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 vector = b - a;
		return Vector3.Dot(c - a, vector.normalized) / vector.magnitude;
	}

	public float XSway(float t)
	{
		return Mathf.Sin(Time.time * 6f + t * 10f) * mgt * 0.5f * Mathf.Sin(t * (float)Math.PI);
	}

	public float YSway(float t)
	{
		return Mathf.Sin(t * (float)Math.PI) * (0f - (0.5f + mgt));
	}

	public void Tick()
	{
		speed += Time.deltaTime;
		pos = Vector3.MoveTowards(pos, (sign == 1) ? posB : posA, Time.deltaTime * speed);
		float num = (rend.transform.InverseTransformPoint(pos).z / dist).Abs();
		if (Game.player.JumpReleased() || (num > 0.9f && sign > 0) || (num < 0.1f && sign < 0))
		{
			Drop();
			Game.player.sway.Sway(5f, 0f, 0f, 4f);
			Game.player.rb.AddForce((Vector3.up + posA.DirTo(posB) * sign * 0.5f).normalized * 25f, ForceMode.Impulse);
			Game.player.rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
			return;
		}
		mgt = mgtCurve.Evaluate(mgtTimer);
		mgtTimer += Time.deltaTime * 0.5f;
		temp.x = XSway(num);
		temp.y = YSway(num);
		temp.z = 0f;
		temp = rend.transform.TransformDirection(temp);
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 2f);
			off = Vector3.LerpUnclamped(offset, -Vector3.up * 1.5f, curve.Evaluate(timer));
		}
		Game.player.t.position = pos + temp + off;
		Game.player.camController.Angle(XSway(num) * 10f);
		effect.t.position = pos + temp;
		effect.source.pitch = Mathf.Clamp(0.5f + speed / 7.5f, 0f, 1.5f);
		effect.source.volume = speed / 15f;
	}

	public void Drop()
	{
		mgt = (mgtTimer = 0f);
		effect.gameObject.SetActive(value: false);
		Game.player.Drop();
		Game.player.sway.Sway(2.5f, 0f, -5f, 2f);
		Game.sounds.PlayClipAtPosition(Game.player.sfxChainOff, 1f, Game.player.t.position);
	}

	private void OnDrawGizmos()
	{
		if (poses.Length != 0)
		{
			for (int i = 0; i < poses.Length - 1; i++)
			{
				Gizmos.DrawLine(poses[i], poses[i + 1]);
			}
		}
		else
		{
			Gizmos.DrawLine(posA, posB);
		}
		Gizmos.DrawSphere(base.transform.position, 0.25f);
		Gizmos.color = Color.green / 2f;
		Gizmos.DrawSphere(target, 0.25f);
	}
}
