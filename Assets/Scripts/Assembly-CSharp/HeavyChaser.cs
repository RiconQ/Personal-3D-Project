using System;
using UnityEngine;

public class HeavyChaser : PooledMonobehaviour
{
	public Transform[] checkPoses;

	public ParticleSystem particle;

	public MeshRenderer rend;

	public bool isStopped;

	public Color aColor;

	public Color bColor;

	private float timer;

	private MaterialPropertyBlock block;

	private void Update()
	{
		base.t.Translate(0f, 0f, Time.deltaTime * 20f, Space.Self);
		base.t.rotation = Quaternion.RotateTowards(base.t.rotation, Quaternion.LookRotation(base.t.position.DirTo(Game.player.t.position.With(null, base.t.position.y))), Time.deltaTime * 45f);
	}

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
		particle.Play();
		timer = 0f;
		isStopped = false;
		block.SetColor("_TintColor", aColor);
		rend.SetPropertyBlock(block);
	}

	private void FixedUpdate()
	{
		if (isStopped)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 4f);
			block.SetColor("_TintColor", Color.Lerp(aColor, bColor, timer));
			rend.SetPropertyBlock(block);
			if (timer == 1f)
			{
				base.gameObject.SetActive(value: false);
			}
			return;
		}
		Transform[] array = checkPoses;
		foreach (Transform transform in array)
		{
			if (!Physics.Raycast(transform.position, Vector3.down, 1f, 1))
			{
				isStopped = true;
				particle.Stop();
				break;
			}
			if (Physics.Raycast(transform.position, transform.forward, 2f, 1))
			{
				isStopped = true;
				particle.Stop();
				break;
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
				particle.Stop();
				isStopped = true;
				break;
			}
			case 14:
			{
				DamageData damageData = new DamageData();
				damageData.dir = base.t.forward;
				damageData.newType = Game.style.basicBluntHit;
				damageData.amount = 40f;
				other.GetComponent<IDamageable<DamageData>>().Damage(damageData);
				QuickEffectsPool.Get("Block", base.t.position, Quaternion.LookRotation(base.t.forward)).Play();
				particle.Stop();
				isStopped = true;
				break;
			}
			}
		}
	}
}
