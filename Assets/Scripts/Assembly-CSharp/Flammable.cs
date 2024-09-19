using System;
using UnityEngine;

public class Flammable : MonoBehaviour
{
	public ParticleSystem particle;

	public BaseEnemy enemy;

	private int timerStep;

	private float lifetime;

	private IDamageable<DamageData> damageable;

	private DamageData damage = new DamageData();

	public bool onFire { get; private set; }

	public Transform tParticle { get; private set; }

	private void Awake()
	{
		tParticle = particle.transform;
		if (particle.isPlaying)
		{
			particle.Stop();
		}
		damageable = GetComponent<IDamageable<DamageData>>();
		damage.dir = Vector3.up;
		damage.amount = 10f;
		damage.newType = StyleData.instance.basicBurn;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		onFire = false;
		lifetime = 0f;
		particle.Stop();
	}

	private void OnEnable()
	{
		if (onFire)
		{
			particle.Play();
		}
		else
		{
			particle.Stop();
		}
	}

	private void LateUpdate()
	{
		if (onFire)
		{
			lifetime += Time.deltaTime;
			tParticle.rotation = Quaternion.identity;
			if (enemy.health < 10f)
			{
				damageable.Damage(damage);
				SetOnFire(value: false);
			}
			else
			{
				enemy.ChangeHealth(10f * Time.deltaTime);
			}
		}
	}

	public bool CheckFire(Flammable other)
	{
		if ((onFire && !other.onFire) || (other.onFire && !onFire))
		{
			SetOnFire();
			other.SetOnFire();
			return true;
		}
		return false;
	}

	public bool SetOnFire(bool value = true)
	{
		if (onFire == value)
		{
			return false;
		}
		onFire = value;
		if (onFire)
		{
			particle.Play();
		}
		else
		{
			particle.Stop();
		}
		return true;
	}
}
