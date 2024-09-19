using UnityEngine;

public class PhantomSlash : PooledMonobehaviour
{
	private float timer;

	private float delay = 0.25f;

	private BaseEnemy e;

	private TrailScript trail;

	private ParticleSystem particle;

	private DamageData dmg = new DamageData();

	protected override void Awake()
	{
		base.Awake();
		trail = GetComponentInChildren<TrailScript>();
		particle = GetComponentInChildren<ParticleSystem>();
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		trail.Reset();
		particle.Play();
	}

	public void Setup(BaseEnemy enemy)
	{
		e = enemy;
		timer = 0f;
	}

	public void LateUpdate()
	{
		if ((bool)e)
		{
			base.t.position = e.GetActualPosition();
			timer = Mathf.MoveTowards(timer, delay, Time.deltaTime);
			if (e.dead)
			{
				e = null;
				base.gameObject.SetActive(value: false);
			}
			if (timer == delay)
			{
				dmg.dir = (Vector3.up - Game.player.tHead.forward).normalized;
				dmg.amount = 35f;
				dmg.knockdown = false;
				e.Damage(dmg);
				e = null;
				trail.Play();
				Game.time.SlowMotion(0.05f, 0.1f, 0.1f);
				CameraController.shake.Shake(2);
			}
		}
	}
}
