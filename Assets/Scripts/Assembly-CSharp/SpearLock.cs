using System;
using UnityEngine;

public class SpearLock : MonoBehaviour
{
	public float damageIgnoring = 0.5f;

	public float radius = 9f;

	public Vector2 delayMinMax = new Vector2(0.2f, 0.4f);

	public Transform t;

	public LineRenderer line;

	public PooledWeapon weapon;

	public BaseEnemy enemy;

	public Transform tParticleA;

	public Transform tParticleB;

	public ParticleSystem aParticle;

	public ParticleSystem bParticle;

	private float lifetime;

	private float timer;

	private Vector3 pos;

	private void Awake()
	{
		line.positionCount = 8;
		BaseEnemy.OnDamage = (Action<BaseEnemy>)Delegate.Combine(BaseEnemy.OnDamage, new Action<BaseEnemy>(Check2));
	}

	private void OnDestroy()
	{
		BaseEnemy.OnDamage = (Action<BaseEnemy>)Delegate.Remove(BaseEnemy.OnDamage, new Action<BaseEnemy>(Check2));
	}

	public void Check()
	{
		lifetime = 0f;
		enemy = CrowdControl.instance.GetClosestEnemy(weapon.t.position, radius);
		if ((bool)enemy && (!enemy.isActiveAndEnabled || !enemy.agent.enabled))
		{
			enemy = null;
		}
		if ((bool)enemy)
		{
			Vector3 vector = t.position.DirTo(enemy.GetActualPosition());
			tParticleA.SetPositionAndRotation(t.position, Quaternion.LookRotation(vector));
			tParticleB.SetPositionAndRotation(enemy.GetActualPosition(), Quaternion.LookRotation(-vector));
			CameraController.shake.Shake(1);
		}
		line.enabled = enemy;
	}

	public void Check2(BaseEnemy e)
	{
		if (lifetime > damageIgnoring && e == enemy)
		{
			Reset();
		}
	}

	public void Reset()
	{
		line.enabled = false;
		enemy = null;
	}

	public void Update()
	{
		if (!enemy)
		{
			return;
		}
		lifetime += Time.deltaTime;
		if (enemy.dead || !enemy.isActiveAndEnabled)
		{
			Reset();
			return;
		}
		timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
		if (timer == 0f)
		{
			enemy.Stagger(Vector3.up);
			timer = UnityEngine.Random.Range(delayMinMax.x, delayMinMax.y);
			aParticle.Play();
			bParticle.Play();
			float num = 0f;
			float num2 = 0.5f;
			float num3 = 0f;
			for (int i = 0; i < 8; i++)
			{
				num = (float)i / 7f;
				pos = Vector3.Lerp(t.position, enemy.GetActualPosition(), num);
				num3 = Mathf.Sin(num * (float)Math.PI);
				pos.x += UnityEngine.Random.Range(0f - num2, num2) * num3;
				pos.y += UnityEngine.Random.Range(0f - num2, num2) * num3;
				pos.z += UnityEngine.Random.Range(0f - num2, num2) * num3;
				line.SetPosition(i, pos);
			}
		}
		else
		{
			line.widthMultiplier = timer * 2f;
		}
	}

	public void OnDisable()
	{
		enemy = null;
		line.enabled = false;
	}
}
