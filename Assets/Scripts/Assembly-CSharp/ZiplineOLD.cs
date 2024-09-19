using System;
using UnityEngine;

[SelectionBase]
public class ZiplineOLD : SetupableMonobehavior, IPlatformable
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

	private Vector3 off;

	private Vector3 offset;

	private Vector3 pivotOffset;

	private Vector3 pos;

	private Vector3 targetPos;

	private PooledEffect effect;

	private void Awake()
	{
		t = base.transform;
		dist = Vector3.Distance(posA, posB);
		count = Mathf.RoundToInt(dist);
		poses = new Vector3[count];
		for (int i = 0; i < count; i++)
		{
			poses[i] = Vector3.Lerp(posA, posB, (float)i / (float)count);
			poses[i].y -= Mathf.Sin((float)i / dist * (float)Math.PI) * 2f;
		}
		rend.transform.SetPositionAndRotation(posA, Quaternion.FromToRotation(Vector3.up, posA - posB));
		rend.GenerateChainMesh(poses);
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
		speed = Mathf.Clamp(Game.player.rb.velocity.magnitude, 15f, 30f);
		if ((Game.player.JumpHolded() || PlayerController.gamepad) && Game.player.Grab(this))
		{
			pos = Game.player.t.position;
			pivotOffset = pos - Game.player.t.position.ClosestPointOnLine(posA, posB);
			offset = -pivotOffset + new Vector3(0f, -1.5f, 0f);
			sign = Vector3.Dot(Game.player.tHead.forward, posB - posA).Sign();
			targetPos = ((sign == 1) ? posB : posA) + pivotOffset + (posA - posB).normalized * (sign * 2);
			if (!(Vector3.Distance(targetPos, pos) < 3f))
			{
				effect = QuickEffectsPool.Get("Zipline FX");
				effect.t.SetPositionAndRotation(pos, Quaternion.LookRotation(t.forward * sign));
				effect.gameObject.SetActive(value: true);
				effect.Play();
				timer = 0f;
				CameraController.shake.Shake(1);
				Game.sounds.PlayClipAtPosition(Game.player.sfxChainGrab, 1f, Game.player.t.position);
			}
		}
	}

	public void LateUpdate()
	{
	}

	public void Tick()
	{
		if (Game.player.JumpReleased())
		{
			Drop();
			Game.player.sway.Sway(5f, 0f, 0f, 4f);
			Game.player.rb.AddForce(Vector3.up * 25f, ForceMode.Impulse);
		}
		if (((posB - posA).normalized * sign).y > 0.25f)
		{
			speed = Mathf.MoveTowards(speed, 0f, Time.deltaTime);
			if (speed == 0f)
			{
				sign *= -1;
				targetPos = ((sign == 1) ? posB : posA) + pivotOffset + (posA - posB).normalized * (sign * 2);
			}
		}
		else
		{
			speed += Time.deltaTime * 5f;
		}
		Debug.DrawRay(targetPos, Vector3.up, Color.green);
		pos = Vector3.MoveTowards(pos, targetPos, Time.deltaTime * speed);
		effect.t.position = pos - pivotOffset;
		effect.source.pitch = Mathf.Clamp(0.5f + speed / 7.5f, 0f, 1.5f);
		effect.source.volume = speed / 15f;
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 2f);
			off = Vector3.LerpUnclamped(Vector3.zero, offset, curve.Evaluate(timer));
		}
		if (pos == targetPos)
		{
			Drop();
			Game.player.rb.velocity = (posB - posA).normalized * ((float)sign * speed);
		}
		Game.player.t.position = pos + off;
		Game.player.camController.Angle(Mathf.Sin(Time.time * 3f) * speed);
	}

	public void Drop()
	{
		effect.gameObject.SetActive(value: false);
		Game.player.Drop();
		Game.sounds.PlayClipAtPosition(Game.player.sfxChainOff, 1f, Game.player.t.position);
	}

	private void Update()
	{
		for (int i = 0; i < count; i++)
		{
			poses[i] = Vector3.Lerp(posA, posB, (float)i / (float)count);
			poses[i].y -= Mathf.Sin((float)i / dist * (float)Math.PI) * (1f + Mathf.Sin(Time.time) / 2f);
			poses[i] += tMesh.forward * Mathf.Sin(Time.time * 4f + (float)i / dist * 4f) * 0.5f * Mathf.Sin((float)i / dist * (float)Math.PI);
		}
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
		Gizmos.DrawSphere(base.transform.position, 0.25f);
		Gizmos.color = Color.green / 2f;
		Gizmos.DrawSphere(target, 0.25f);
	}
}
