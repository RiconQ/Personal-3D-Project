using System;
using UnityEngine;

public class HeavyGroundAttack : PooledMonobehaviour
{
	public float lifetime = 2f;

	private float timer;

	public float rotSpeed = 45f;

	public float speed = 20f;

	public ParticleSystem stopParticle;

	public ParticleSystem particle;

	public Transform tMesh;

	public MeshRenderer rend;

	public bool isStopped;

	public Color aColor;

	public Color bColor;

	private MaterialPropertyBlock block;

	private Vector3 angles = new Vector3(0f, -90f, -90f);

	protected override void Awake()
	{
		base.Awake();
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		timer = (lifetime = 0f);
		particle.Play();
		angles.x = 75f;
		isStopped = false;
		block.SetColor("_TintColor", aColor);
		rend.SetPropertyBlock(block);
		if ((bool)tMesh)
		{
			tMesh.localEulerAngles = angles;
		}
	}

	private void Update()
	{
		angles.x = Mathf.Lerp(angles.x, 125f, Time.deltaTime);
		if ((bool)tMesh)
		{
			tMesh.localEulerAngles = angles;
		}
		base.t.Translate(0f, 0f, Time.deltaTime * speed, Space.Self);
		base.t.rotation = Quaternion.RotateTowards(base.t.rotation, Quaternion.LookRotation(base.t.position.DirTo(Game.player.t.position.With(null, base.t.position.y))), Time.deltaTime * rotSpeed);
		if (isStopped)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 4f);
			block.SetColor("_TintColor", Color.Lerp(aColor, bColor, timer));
			rend.SetPropertyBlock(block);
			if (timer == 1f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			lifetime += Time.deltaTime;
			if (lifetime > 3f)
			{
				isStopped = true;
				particle.Stop();
			}
		}
	}

	private void FixedUpdate()
	{
		if (!isStopped)
		{
			if (Physics.CheckSphere(tMesh.position, 0.5f, 1))
			{
				QuickEffectsPool.Get("Block", tMesh.position, Quaternion.LookRotation(tMesh.forward)).Play();
				isStopped = true;
				particle.Stop();
			}
			else if (!Physics.Raycast(base.t.position + Vector3.up, Vector3.down, 2f, 1))
			{
				isStopped = true;
				particle.Stop();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isStopped)
		{
			switch (other.gameObject.layer)
			{
			case 9:
			{
				Vector4 dir = (base.t.forward + Vector3.up).normalized;
				dir.w = 1f;
				Game.player.Damage(dir);
				isStopped = true;
				particle.Stop();
				break;
			}
			case 14:
			{
				DamageData damageData = new DamageData();
				damageData.dir = Vector3.up;
				damageData.newType = Game.style.basicBluntHit;
				damageData.amount = 40f;
				other.GetComponent<IDamageable<DamageData>>().Damage(damageData);
				QuickEffectsPool.Get("Block", tMesh.position, Quaternion.LookRotation(tMesh.forward)).Play();
				other.attachedRigidbody.velocity = (base.t.forward + Vector3.up).normalized * 15f;
				other.attachedRigidbody.AddTorque(Vector3.one * 10f);
				isStopped = true;
				particle.Stop();
				break;
			}
			}
		}
	}
}
