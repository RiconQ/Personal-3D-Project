using System;
using UnityEngine;

public class BreakableB : BaseBreakable
{
	public static Action<GameObject> OnBreak = delegate
	{
	};

	public float maxHealth = 100f;

	public float minDamageVelocitySqr = 25f;

	public GameObject _prefabOnBreak;

	public BreakableEffect effect;

	public LayerMask damageMask;

	public DamageData damage = new DamageData();

	[HideInInspector]
	public ShelvesTest shelves;

	private bool isKinematicHashed;

	private float stunTimer;

	private float checkTimer;

	private float damageTimer;

	private Vector3 stunGravity = new Vector3(0f, -2f, 0f);

	private AudioSource source;

	protected EnemyMaterial mat;

	private Vector3 startPosition;

	private Quaternion startRotation;

	public bool lethal { get; private set; }

	public float health { get; private set; }

	public override void Kick(Vector3 dir)
	{
		if ((bool)shelves && !shelves.isBreaked)
		{
			shelves.Trigger();
			return;
		}
		damage.stylePoint = StyleData.instance.ObjectKicked;
		if (stunTimer != 0f)
		{
			Unstun();
		}
		if ((bool)base.rb && !base.rb.isKinematic)
		{
			base.t.position += Vector3.up / 2f;
			CrowdControl.instance.GetClosestEnemyToNormal(base.rb.worldCenterOfMass, dir, 20f, 20f, out var enemy);
			if ((bool)enemy)
			{
				Vector3 v = base.rb.worldCenterOfMass - enemy.GetActualPosition();
				float time = ((v.With(null, 0f).magnitude > 8f) ? (0.5f + Mathf.Clamp01((0f - v.y) / 20f)) : 0.3f);
				base.rb.AddTorque(Vector3.one * 90f, ForceMode.Impulse);
				base.rb.AddBallisticForce(enemy.GetActualPosition(), time, Physics.gravity.y, resetVelocity: true);
			}
			else
			{
				base.rb.velocity = Vector3.zero;
				base.rb.AddForce(dir * 45f, ForceMode.Impulse);
			}
		}
		PlaySound(effect.kickSound);
	}

	public override void Damage(DamageData damage)
	{
		if ((bool)shelves && !shelves.isBreaked)
		{
			shelves.Trigger();
			return;
		}
		if (damage.amount > 0f)
		{
			damageTimer = 0.5f;
		}
		if ((bool)damage.newType && damage.newType.kick)
		{
			if (damage.newType == Game.player.weapons.kickController.dmg_AirKick && damageTimer > 0f && Vector3.Angle(damage.dir, Vector3.down) < 30f)
			{
				health -= 80f;
			}
			health -= damage.amount;
			if (health > 0f)
			{
				Kick(damage.dir);
			}
			else
			{
				Break(damage.dir);
			}
			return;
		}
		if (damage.newType == Game.player.weapons.daggerController.dmg_Pull)
		{
			damage.stylePoint = Game.style.ObjectPulled;
		}
		health -= damage.amount;
		if ((bool)damage.newType && damage.newType.stun)
		{
			Stun();
		}
		else
		{
			Unstun();
		}
		if (health > 0f)
		{
			mat.Blink(damage.amount / 25f);
			if ((bool)base.rb && !base.rb.isKinematic && damage.newType != Game.style.basicBluntHit)
			{
				Vector3 dir = damage.dir;
				if (damage.newType != Game.player.weapons.kickController.dmg_AirKick)
				{
					dir *= Mathf.Clamp(damage.amount, 20f, 50f);
					base.rb.AddForceAndTorque(dir, dir);
					base.rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
				}
				else
				{
					base.rb.AddForce(CrowdControl.instance.GetClosestDirectionToNormal(base.rb.worldCenterOfMass, dir, 30f) * 45f, ForceMode.Impulse);
				}
			}
			PlaySound(effect.kickSound);
		}
		else
		{
			Break(damage.dir);
		}
	}

	public void Break(Vector3 dir)
	{
		if (base.gameObject.activeInHierarchy)
		{
			if ((bool)_prefabOnBreak)
			{
				QuickPool.instance.Get(_prefabOnBreak, base.rb.worldCenterOfMass, base.t.rotation);
			}
			else
			{
				QuickEffectsPool.Get(effect.effectName, base.clldr.bounds.center, Quaternion.LookRotation(dir)).Play();
			}
			if (base.rb.isKinematic)
			{
				CameraController.shake.Shake(2);
			}
			if (OnBreak != null)
			{
				OnBreak(base.gameObject);
			}
			base.gameObject.SetActive(value: false);
		}
	}

	public override void Awake()
	{
		base.Awake();
		mat = GetComponent<EnemyMaterial>();
		mat.Setup();
		source = GetComponentInChildren<AudioSource>();
		isKinematicHashed = base.rb.isKinematic;
		startPosition = base.t.position;
		startRotation = base.t.rotation;
		damage.newType = StyleData.instance.basicBluntHit;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	public void Stun()
	{
		stunTimer = 2f;
		base.rb.drag = 2f;
		base.rb.useGravity = false;
		mat.Stun();
	}

	public void Unstun()
	{
		stunTimer = 0f;
		base.rb.drag = 0f;
		base.rb.useGravity = true;
		mat.Unstun();
	}

	private void Update()
	{
		if (stunTimer != 0f)
		{
			stunTimer = Mathf.MoveTowards(stunTimer, 0f, Time.deltaTime);
			if (stunTimer == 0f)
			{
				Unstun();
			}
		}
		checkTimer.MoveTowards(0f);
		damageTimer.MoveTowards(0f);
		if (lethal != base.rb.velocity.sqrMagnitude > minDamageVelocitySqr)
		{
			lethal = !lethal;
			if (!lethal)
			{
				damage.stylePoint = null;
			}
		}
	}

	private void FixedUpdate()
	{
		if (stunTimer > 0f)
		{
			float num = 180f * Time.deltaTime;
			base.rb.AddTorque(num, num, num);
			base.rb.AddForce(stunGravity);
		}
	}

	private void OnEnable()
	{
		health = maxHealth;
	}

	private void Reset()
	{
		health = maxHealth;
		mat.ResetBlink();
		base.t.SetPositionAndRotation(startPosition, startRotation);
		base.rb.velocity = Vector3.zero;
		base.rb.angularVelocity = Vector3.zero;
		base.rb.isKinematic = isKinematicHashed;
		source.Stop();
		damage.stylePoint = null;
		if (stunTimer != 0f)
		{
			Unstun();
		}
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private void OnCollisionEnter(Collision c)
	{
		float sqrMagnitude = c.relativeVelocity.sqrMagnitude;
		int layer = c.gameObject.layer;
		if ((bool)_prefabOnBreak && sqrMagnitude > 9f && Physics.CheckSphere(base.t.position, 5f, 1024))
		{
			Break(c.contacts[0].normal);
			CameraController.shake.Shake(1);
		}
		else if (minDamageVelocitySqr != 0f && lethal && (int)damageMask == ((int)damageMask | (1 << layer)))
		{
			if (layer == 10)
			{
				health -= c.relativeVelocity.magnitude * 2f;
				if (health <= 0f)
				{
					Break(c.contacts[0].normal);
				}
				else
				{
					mat.Blink();
				}
				CameraController.shake.Shake(1);
			}
			if (c.gameObject.activeInHierarchy && checkTimer == 0f)
			{
				checkTimer = 0.1f;
				damage.dir = (-c.contacts[0].normal + Vector3.up) / 2f;
				damage.amount = 60f;
				damage.knockdown = true;
				damage.newType = Game.style.basicBluntHit;
				c.transform.root.GetComponentInChildren<IDamageable<DamageData>>().Damage(damage);
			}
			if (!_prefabOnBreak)
			{
				if (layer != 10)
				{
					health -= c.relativeVelocity.magnitude;
					if (health <= 0f)
					{
						Break(c.contacts[0].normal);
					}
					else if (layer == 0)
					{
						base.rb.velocity = c.contacts[0].normal * 10f;
					}
				}
			}
			else
			{
				Break(c.contacts[0].normal);
			}
		}
		else if (sqrMagnitude > 16f)
		{
			source.PlayClip(effect.damage[0], sqrMagnitude / 16f * 0.2f, Mathf.Clamp(sqrMagnitude / 16f * 0.1f, 0f, 1.25f));
		}
	}

	private void PlaySound(AudioClip clip)
	{
		if (base.isActiveAndEnabled)
		{
			if (source.isPlaying)
			{
				source.Stop();
			}
			if (source.clip != clip)
			{
				source.clip = clip;
			}
			source.Play();
		}
	}
}
