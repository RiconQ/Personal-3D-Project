using UnityEngine;

public class GroundPound : PooledMonobehaviour
{
	public DamageData damage = new DamageData();

	public ParticleSystem particle;

	private BaseEnemy e;

	private float timer;

	private float delay = 0.3f;

	private bool triggered;

	protected override void Awake()
	{
		base.Awake();
	}

	public void Setup(BaseEnemy enemy)
	{
		if ((bool)enemy.shieldDamageType && !enemy.shieldDestroyed)
		{
			base.gameObject.SetActive(value: false);
		}
		e = enemy;
		base.t.localEulerAngles = new Vector3(0f, Quaternion.LookRotation(Game.player.t.position.DirTo(base.t.position.With(null, Game.player.t.position.y))).y, 0f);
		timer = 0f;
		triggered = false;
		particle.Play();
		enemy.Stagger(-enemy.GetTargetDirGrounded());
	}

	private void LateUpdate()
	{
		if (!e)
		{
			return;
		}
		base.t.position = e.GetActualPosition();
		timer = Mathf.MoveTowards(timer, delay, Time.deltaTime);
		if (e.dead || !e.isActiveAndEnabled || !e.agent.enabled)
		{
			e = null;
			triggered = true;
		}
		if (!triggered)
		{
			if (timer == delay)
			{
				triggered = true;
				damage.dir = (-Game.player.t.position.DirTo(e.GetActualPosition()) + Vector3.up).normalized;
				damage.knockdown = true;
				damage.amount = 20f;
				e.Damage(damage);
				CameraController.shake.Shake(2);
			}
		}
		else if (!particle.isPlaying)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
