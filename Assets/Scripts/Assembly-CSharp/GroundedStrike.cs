using UnityEngine;

public class GroundedStrike : PooledMonobehaviour
{
	public float speed = 10f;

	public float lifetime = 3f;

	public ParticleSystem particle;

	public BaseEnemy enemy;

	public DamageData dmg;

	private bool triggered;

	private int intDist;

	private float dist;

	private float timer;

	private Vector3 lastPos;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		lastPos = base.t.position;
		timer = 0f;
		dist = (intDist = 0);
		triggered = false;
		particle.Play();
		CrowdControl.instance.GetClosestEnemyToNormal(base.t.position, base.t.forward, 30f, 20f, out enemy);
	}

	private void Update()
	{
		if (!triggered)
		{
			dist += Time.deltaTime * speed;
			base.t.Translate(0f, 0f, Time.deltaTime * speed, Space.Self);
			if ((bool)enemy)
			{
				base.t.rotation = Quaternion.RotateTowards(base.t.rotation, Quaternion.LookRotation(base.t.position.DirTo(enemy.GetActualPosition().With(null, base.t.position.y))), Time.deltaTime * 45f);
			}
		}
		timer = Mathf.MoveTowards(timer, lifetime, Time.deltaTime);
		if (timer == lifetime && !particle.isStopped)
		{
			particle.Stop();
		}
		if (!particle.isPlaying)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void FixedUpdate()
	{
		if (triggered)
		{
			return;
		}
		if ((int)dist * 2 != intDist)
		{
			intDist = (int)dist * 2;
			if (Physics.Linecast(lastPos, base.t.position, 1))
			{
				triggered = true;
				particle.Stop();
				Debug.DrawLine(lastPos, base.t.position, Color.red, 0.25f);
			}
			else
			{
				Debug.DrawLine(lastPos, base.t.position, Color.cyan, 0.25f);
			}
			lastPos = base.t.position;
		}
		if (!particle.isStopped && !Physics.Raycast(base.t.position, Vector3.down, 2f, 1))
		{
			particle.Stop();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!triggered && other.gameObject.layer != 9)
		{
			triggered = true;
			dmg.dir = (Vector3.up - base.t.forward).normalized;
			CameraController.shake.Shake(1);
			other.GetComponent<IDamageable<DamageData>>().Damage(dmg);
			particle.Stop();
			QuickEffectsPool.Get("Spear Launch HIT", base.t.position, base.t.rotation).Play();
		}
	}
}
