using System;
using UnityEngine;

public class KickController : MonoBehaviour
{
	public static Action OnKick = delegate
	{
	};

	public static BaseEnemy lastKicked;

	public DamageData damageKick;

	public DamageData damageAirkick;

	public DamageData damageSlidekick;

	public LayerMask mask;

	[Header("DamageTypes")]
	public DamageType dmg_Kick;

	public DamageType dmg_SlideKick;

	public DamageType dmg_AirKick;

	[Header("Audio")]
	public AudioSource source;

	public AudioClip chargeSound;

	public AudioClip kickSound;

	public AudioClip hitSound;

	public AudioClip sfxMidAirHit;

	public AudioClip sfxWallKickJump;

	public AudioClip sfxSlidekick;

	private Animator animator;

	private PlayerController player;

	private PlayerWeapons manager;

	private int layermask = 58368;

	private int kickType;

	private float minKickJumpDot;

	private float minDistance;

	private float distance;

	private RaycastHit hit;

	private Vector3 closestTargetPos;

	private Collider targetCollider;

	private Collider[] colliders = new Collider[3];

	private Vector3 kickBoxPosition = new Vector3(0f, 0f, 1.4f);

	private Vector3 kickHalfBox = new Vector3(0.7f, 1.25f, 1.4f);

	private Vector3 airKickEndPoint = new Vector3(0f, 0f, 3.75f);

	private TrailScript trail;

	private ParticleSystem trailParticle;

	public bool isCharging { get; private set; }

	public float timer { get; private set; }

	private void Awake()
	{
		animator = GetComponent<Animator>();
		player = GetComponentInParent<PlayerController>();
		manager = GetComponentInParent<PlayerWeapons>();
		trail = GetComponentInChildren<TrailScript>();
		trail.transform.SetParent(null);
		trailParticle = trail.GetComponent<ParticleSystem>();
		OnValidate();
		WeaponController.OnCharge = (Action)Delegate.Combine(WeaponController.OnCharge, new Action(OnAttack));
	}

	private void Start()
	{
		damageKick.knockdown = true;
		damageKick.amount = 0f;
		damageKick.newType = dmg_Kick;
	}

	private void OnDestroy()
	{
		WeaponController.OnCharge = (Action)Delegate.Remove(WeaponController.OnCharge, new Action(OnAttack));
	}

	private void OnAttack()
	{
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			animator.Play("Cancel", -1, 0f);
		}
	}

	private void OnValidate()
	{
		minKickJumpDot = Mathf.Cos((float)Math.PI / 12f);
	}

	private void OnDisable()
	{
		if (isCharging)
		{
			isCharging = false;
		}
	}

	public void Sway()
	{
		player.sway.Sway(-5f, 0f, 0f, 2f);
	}

	private void OnDrawGizmos()
	{
		if ((bool)player)
		{
			Gizmos.color = Color.black / 2f;
			Gizmos.matrix = player.tHead.localToWorldMatrix;
			Gizmos.DrawWireCube(kickBoxPosition, kickHalfBox * 2f);
		}
	}

	public void GetTargetCollider()
	{
		targetCollider = null;
		minDistance = float.PositiveInfinity;
		if (player.grounder.grounded && !player.slide.isSliding)
		{
			Physics.OverlapBoxNonAlloc(player.tHead.TransformPoint(kickBoxPosition), kickHalfBox, colliders, player.tHead.rotation, layermask);
		}
		else
		{
			Physics.OverlapCapsuleNonAlloc(player.tHead.position, player.tHead.TransformPoint(airKickEndPoint), 1.2f, colliders, layermask);
		}
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] != null)
			{
				distance = Vector3.Distance(player.tHead.position, colliders[i].ClosestPoint(player.tHead.position));
				if (distance < minDistance)
				{
					minDistance = distance;
					targetCollider = colliders[i];
				}
				colliders[i] = null;
			}
		}
	}

	private void Kick()
	{
		damageKick.dir = player.tHead.forward;
		damageKick.newType = dmg_Kick;
		targetCollider.GetComponent<IDamageable<DamageData>>().Damage(damageKick);
		if (OnKick != null)
		{
			OnKick();
		}
		CameraController.shake.Shake(1);
		player.rb.AddForce(Vector3.ProjectOnPlane(-player.tHead.forward, player.grounder.gNormal) * 10f, ForceMode.Impulse);
	}

	private void Slidekick()
	{
		damageSlidekick.dir = player.tHead.forward;
		damageSlidekick.newType = dmg_SlideKick;
		targetCollider.GetComponent<IDamageable<DamageData>>().Damage(damageSlidekick);
		if (OnKick != null)
		{
			OnKick();
		}
		player.rb.position = Vector3.Lerp(player.rb.position, closestTargetPos, 0.75f);
		player.grounder.Ungrounded();
		player.rb.velocity = (-player.tHead.forward.With(null, 0f) + Vector3.up).normalized * 10f;
		player.airControlBlock = 0.1f;
		source.PlayClip(sfxSlidekick);
	}

	private void Airkick()
	{
		if (player.tHead.forward.y > 0f - minKickJumpDot)
		{
			player.rb.position = Vector3.Lerp(player.rb.position, closestTargetPos, 0.75f);
			player.rb.velocity = Vector3.zero;
		}
		player.rb.AddForce((Vector3.up - player.tHead.forward.With(null, 0f).normalized / 4f).normalized * 20f, ForceMode.Impulse);
		player.airControlBlock = 0.25f;
		if (targetCollider.gameObject.layer == 10 && targetCollider.attachedRigidbody.isKinematic)
		{
			damageAirkick.dir = player.tHead.forward.With(null, player.tHead.forward.y.Abs() / 2f);
		}
		else
		{
			damageAirkick.dir = player.tHead.forward;
		}
		damageAirkick.newType = dmg_AirKick;
		targetCollider.GetComponent<IDamageable<DamageData>>().Damage(damageAirkick);
		if (OnKick != null)
		{
			OnKick();
		}
		if (targetCollider.gameObject.layer == 13)
		{
			timer = 0f;
		}
		player.grounder.Ungrounded();
		player.ParkourMove();
		source.PlayClip(sfxMidAirHit);
	}

	private void Wallkick()
	{
		player.rb.velocity = Vector3.zero;
		player.rb.AddForce((Vector3.up - player.tHead.forward.With(null, (0f - hit.normal.y) * 2f).normalized).normalized * (23.5f * (1f + hit.normal.y.Abs())), ForceMode.Impulse);
		player.sway.Sway(0f, 0f, 10f, 5f);
		QuickEffectsPool.Get("WallKick", hit.point, Quaternion.LookRotation(hit.normal)).Play();
	}

	private void TangentWallkick(Vector3 normal)
	{
		Vector3 vector = hit.normal * 8f;
		vector.y += 18f;
		vector += normal * 20f;
		player.rb.velocity = Vector3.zero;
		player.rb.AddForce(vector, ForceMode.Impulse);
		player.sway.Sway(5f, 0f, 10f * Mathf.Sign(player.tHead.TransformDirection(normal).x), 3f);
		QuickEffectsPool.Get("WallKick 1", hit.point, Quaternion.LookRotation(vector)).Play();
	}

	public void Strike()
	{
		GetTargetCollider();
		if ((bool)targetCollider && !Physics.Linecast(player.tHead.position, targetCollider.ClosestPoint(player.tHead.position), 1))
		{
			closestTargetPos = targetCollider.ClosestPoint(player.tHead.position);
			if (!player.grounder.grounded)
			{
				Airkick();
			}
			else if (player.slide.isSliding)
			{
				Slidekick();
			}
			else
			{
				Kick();
			}
			trail.transform.SetPositionAndRotation(closestTargetPos, Quaternion.LookRotation(-player.tHead.forward));
			trail.Play();
			trailParticle.Emit(10);
			player.sway.Sway(10f, 0f, 5f, 4.5f);
			CameraController.shake.Shake(2);
		}
		else
		{
			if (!Physics.Raycast(player.tHead.position, player.tHead.forward, out hit, 2.75f, 1))
			{
				return;
			}
			player.jumpHolded = false;
			player.airControlBlock = 0.25f;
			if (hit.normal.y.Abs() < 0.5f && !player.grounder.grounded)
			{
				Vector3 normalized = Vector3.ProjectOnPlane(player.rb.velocity.With(null, 0f), hit.normal).normalized;
				if (player.rb.velocity.With(null, 0f).sqrMagnitude > 2f && Vector3.Angle(normalized, player.tHead.forward) < 45f)
				{
					TangentWallkick(normalized);
				}
				else
				{
					Wallkick();
				}
				player.ParkourMove();
				source.PlayClip(sfxWallKickJump);
				Debug.DrawRay(hit.point, normalized, Color.magenta, 1f);
				Debug.DrawRay(hit.point, player.tHead.forward, Color.green, 1f);
			}
			else if (player.slide.isSliding && hit.normal.y > 0.5f)
			{
				CameraController.shake.Shake(1);
				source.PlayClip(sfxWallKickJump);
				QuickEffectsPool.Get("Poof", player.t.position).Play();
			}
			else
			{
				player.rb.AddForce(-player.tHead.forward * Mathf.Lerp(8f, 4f, player.tHead.forward.y.Abs()), ForceMode.Impulse);
				player.sway.Sway(5f, 0f, 0f, 5f);
				QuickEffectsPool.Get("Poof", player.tHead.position + player.tHead.forward).Play();
			}
			timer = 0.3f;
		}
	}

	public bool IsChargingOrInProcess()
	{
		if (!isCharging)
		{
			return timer > 0f;
		}
		return true;
	}

	public void QuickKick()
	{
		kickType = ((!(player.grounder.grounded & !player.slide.isSliding)) ? 1 : 0);
		animator.Play((kickType == 0) ? "Kick" : "Slidekick", -1, 0f);
		Game.time.SlowMotion(0.01f);
	}

	public void Charge()
	{
		isCharging = true;
		kickType = ((!(player.grounder.grounded & !player.slide.isSliding)) ? 1 : 0);
		animator.Play((kickType == 0) ? "Charge Kick" : "Charge Slidekick", -1, 0f);
		manager.OnKickHolded();
		if (player.slide.isSliding)
		{
			player.slide.Extend();
			player.slide.DeactivateTrigger();
		}
		if (kickType == 1)
		{
			Game.time.SlowMotion(0.3f, 0.5f, 0.1f);
		}
		source.PlayClip(chargeSound);
	}

	public void Release()
	{
		isCharging = false;
		kickType = ((!(player.grounder.grounded & !player.slide.isSliding)) ? 1 : 0);
		animator.Play((kickType == 0) ? "Kick" : "Slidekick", -1, 0f);
		manager.OnKickReleased();
		player.sway.Sway(5f, 0f, 0f, 3f);
		timer = 1f;
		Game.time.StopSlowmo();
		source.PlayClip(kickSound);
	}

	public void Tick()
	{
		if (manager.IsAttacking() || timer != 0f)
		{
			if (isCharging)
			{
				animator.Play("Cancel", -1, 0f);
				isCharging = false;
			}
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime * 2f);
		}
		else if (!isCharging)
		{
			if (!manager.IsAttacking() && Game.player.KickPressed())
			{
				Charge();
			}
		}
		else if (!Game.player.KickHolded())
		{
			Release();
		}
		else if (kickType != ((!(player.grounder.grounded & !player.slide.isSliding)) ? 1 : 0))
		{
			kickType = ((!(player.grounder.grounded & !player.slide.isSliding)) ? 1 : 0);
			animator.Play((kickType == 0) ? "Charge Kick" : "Charge Slidekick", -1, 0f);
			source.PlayClip(chargeSound);
		}
	}
}
