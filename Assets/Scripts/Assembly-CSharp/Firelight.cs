using System;
using UnityEngine;

public class Firelight : MonoBehaviour
{
	private Transform t;

	private ParticleSystem particle;

	private DamageData damage = new DamageData();

	private IDamageable<DamageData> host;

	private BaseEnemy enemy;

	private int cooldownStep;

	public bool onFire { get; private set; }

	public float cooldown { get; private set; }

	private void Awake()
	{
		damage.amount = 25f;
		damage.dir = Vector3.down;
		t = base.transform;
		particle = GetComponent<ParticleSystem>();
		if (particle.isPlaying)
		{
			particle.Stop();
		}
		onFire = false;
		cooldown = 0f;
		host = GetComponentInParent<IDamageable<DamageData>>();
		enemy = GetComponentInParent<BaseEnemy>();
		if (!enemy)
		{
			enemy = GetComponentInParent<Body>().enemy;
		}
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		Stop();
	}

	public bool Play(float value = 5f)
	{
		if (!onFire)
		{
			cooldown = value;
			cooldownStep = Mathf.FloorToInt(cooldown);
			particle.Play();
			onFire = true;
			QuickEffectsPool.Get("Fire Poof", t.position).Play();
			return true;
		}
		return false;
	}

	public void Stop()
	{
		if (onFire)
		{
			cooldown = 0f;
			particle.Stop();
			onFire = false;
		}
	}

	private void LateUpdate()
	{
		if (onFire)
		{
			t.rotation = Quaternion.identity;
			cooldown = Mathf.MoveTowards(cooldown, 0f, Time.deltaTime);
			if (cooldown == 0f)
			{
				Stop();
			}
			else
			{
				enemy.ChangeHealth(-5f * Time.deltaTime);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		damage.dir = t.position.DirTo(other.transform.position);
		other.GetComponent<IDamageable<DamageData>>().Damage(damage);
	}
}
