using System;
using UnityEngine;

[SelectionBase]
public class ClimbingChain : SetupableMonobehavior, IPlatformable
{
	private Transform t;

	private CapsuleCollider clldr;

	private Vector3[] poses;

	public AnimationCurve grabCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public AnimationCurve mgtCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float mgt;

	public float atHeight;

	public Transform tPivot;

	public Transform tCoil;

	public ChainSkinnedRenderer rend;

	public AudioSource coilSource;

	private Vector3 dir;

	private float grabTimer;

	private float timer;

	private float speed;

	private float cnainLength;

	private Vector3 pos;

	private Vector3 pPos;

	private void Awake()
	{
		t = base.transform;
		int num = Mathf.RoundToInt(localTarget.y).Abs();
		poses = new Vector3[num];
		for (int i = 0; i < poses.Length; i++)
		{
			poses[i].y = -i;
		}
		cnainLength = num;
		clldr = GetComponent<CapsuleCollider>();
		clldr.center = new Vector3(0f, (0f - cnainLength) / 2f, 0f);
		clldr.height = cnainLength;
		clldr.radius = 0.75f;
		rend.GenerateChainMesh(poses);
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(base.transform.position, base.transform.position + localTarget);
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
			if (Game.player.oldVel.y < -2f)
			{
				speed = -7.5f;
			}
			else
			{
				speed = Mathf.Clamp(Game.player.oldVel.magnitude, 7.5f, 20f);
			}
			if (speed == 20f)
			{
				Game.player.ParkourMove();
			}
			timer = (grabTimer = 0f);
			mgt = 0f;
			atHeight = t.InverseTransformPoint(Game.player.t.position).y;
			pPos = (pos = Game.player.t.position);
			dir = Game.player.t.position.DirTo(t.position.With(null, Game.player.t.position.y));
			dir = Quaternion.Euler(0f, UnityEngine.Random.Range(60, 120), 0f) * dir;
			tPivot.rotation = Quaternion.LookRotation(dir, Vector3.up);
			coilSource.Play();
			CameraController.shake.Shake(1);
			Game.player.sway.Sway(-2.5f, 0f, 5f, 2f);
			Game.sounds.PlayClipAtPosition(Game.player.sfxChainGrab, 1f, Game.player.t.position);
		}
	}

	public float ChainSway(float y)
	{
		return Mathf.Cos(timer * 8f + (y + atHeight)) * mgt * 0.5f * (1f - Mathf.Clamp01(Mathf.Abs(y + atHeight) / 4f)) * Mathf.Clamp01(y / 4f);
	}

	public void LateUpdate()
	{
		for (int i = 0; i < rend.tBones.Length; i++)
		{
			rend.tBones[i].localPosition = poses[i];
			if (i < poses.Length - 1)
			{
				float num = Vector3.Angle(Vector3.up, poses[i + 1].DirTo(poses[i]));
				float y = Vector3.Distance(poses[i], poses[i + 1]);
				rend.tBones[i].localEulerAngles = new Vector3(num * (float)Vector3.Dot(t.forward, poses[i + 1].DirTo(poses[i])).Sign(), 0f, 0f);
				rend.tBones[i].localScale = new Vector3(1f, y, 1f);
			}
		}
	}

	public void Tick()
	{
		if (Game.player.JumpReleased() || pos.y > t.position.y - 4f)
		{
			Drop();
			Game.player.sway.Sway(5f, 0f, 2f, 2f);
			Game.player.rb.velocity = (Game.player.tHead.forward.With(null, 0f).normalized + Vector3.up).normalized * 20f;
			Game.player.airControlBlock = 0.2f;
			return;
		}
		speed = Mathf.Lerp(speed, 10f, Time.deltaTime * 2f);
		atHeight += Time.deltaTime * speed;
		pos.y += Time.deltaTime * speed;
		float num = ChainSway(0f - t.InverseTransformPoint(pos).y);
		pos.x = pPos.x + num * 0.75f * dir.x;
		pos.z = pPos.z + num * 0.75f * dir.z;
		if (grabTimer != 1f)
		{
			grabTimer = Mathf.MoveTowards(grabTimer, 1f, Time.deltaTime * 2f);
			Game.player.t.position = Vector3.LerpUnclamped(pPos, pos, grabCurve.Evaluate(grabTimer));
		}
		else
		{
			Game.player.t.position = pos;
		}
		Game.player.camController.Angle(num * 5f);
		for (int i = 0; i < poses.Length; i++)
		{
			float z = ChainSway(0f - poses[i].y);
			poses[i].z = z;
			poses[i].y += Time.deltaTime * speed;
		}
		timer += Time.deltaTime;
		mgt = mgtCurve.Evaluate(timer * 0.5f);
		tCoil.Rotate(0f, 0f, 360f / (float)Math.PI * speed * Time.deltaTime, Space.Self);
	}

	public void Drop()
	{
		Game.player.Drop();
		Game.sounds.PlayClipAtPosition(Game.player.sfxChainOff, 1f, Game.player.t.position);
		coilSource.Stop();
		for (int i = 0; i < poses.Length; i++)
		{
			poses[i].y = -i;
			poses[i].x = (poses[i].z = 0f);
			rend.tBones[i].localPosition = poses[i];
			rend.tBones[i].localEulerAngles = Vector3.zero;
			rend.tBones[i].localScale = Vector3.one;
		}
	}
}
