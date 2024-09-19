using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseEnemy : MonoBehaviour, IDamageable<DamageData>, IKickable<Vector3>
{
	public static Action OnAnyEnemyDie = delegate
	{
	};

	public static Action<BaseEnemy> OnDamage = delegate
	{
	};

	public static Action<BaseEnemy> OnEnenyDie = delegate
	{
	};

	public static Action<BaseEnemy> OnEnemyActivated = delegate
	{
	};

	public static Action<BaseEnemy> OnEnemyDeactivated = delegate
	{
	};

	public static BaseEnemy lastDamaged;

	public Transform tTrailPivot;

	public Transform tHead;

	public Transform tHeadMesh;

	public GameObject bodyPrefab;

	public Weapon weapon;

	public DamageType lastDamageType;

	public DamageType shieldDamageType;

	public List<DamageType> blessedWeakness = new List<DamageType>(3);

	public LinkedSouls linkedSouls;

	public bool onNavMeshLink;

	public bool canBlock;

	public bool knockable = true;

	public bool friendlyFire;

	public bool ManualReset;

	public bool isTeleporting;

	public bool shieldDestroyed;

	public int targetIsTooFar;

	public float maxHealth = 100f;

	public float kickMinDamage = 60f;

	public float minStaggerDamage = 50f;

	public float attackDamage = 100f;

	public float attackCorrectionRadius = 60f;

	[Header("Jump")]
	public float landRecover = 0.1f;

	public bool staggered;

	public bool lockJumpRotation;

	public bool directJumpCheck;

	public bool hasLandAttack;

	public float landAttackRecover = 0.25f;

	public float maxDetectionHeight = 1f;

	public DamageData bluntHitDamage;

	public DamageData friendlyDamageInfo;

	[Header("SHIELD")]
	public EnemyShield shield;

	[Header("BUFFED")]
	public bool buffed;

	public ParticleSystem buffedFX;

	public ParticleSystem particleDash;

	protected float repositionTimer;

	[Header("AUDIO")]
	public AudioSource source;

	public EnemySoundsData sounds;

	public float actionTime;

	protected Vector3 warpPos;

	[HideInInspector]
	public Vector3 targetPosition;

	[HideInInspector]
	public Vector3 targetNormal;

	public Transform tTarget;

	protected RaycastHit hit;

	protected NavMeshHit navHit;

	[HideInInspector]
	public OffMeshLink link;

	private Vector3 tHeadAngles = new Vector3(0f, 0f, 0f);

	private Vector3 initialMeshAngles = new Vector3(-90f, 0f, 0f);

	private Quaternion _tempRotation;

	public float minReposDist = 6f;

	public float maxReposDist = 18f;

	public float verticalReposDist = 5f;

	public int dashSign = 1;

	public float dashAngle = 90f;

	public float dashDist = 6f;

	protected Vector3 targetPos;

	protected Vector3 targetDir;

	protected Vector3 targetDirGrounded;

	private Collider[] colliders = new Collider[2];

	private Vector4 attackInfo;

	private IDamageable<Vector4> attackTarget;

	public bool dead { get; private set; }

	public int randomSeed { get; private set; }

	public float health { get; private set; }

	public float isStrafing { get; protected set; }

	public float damageCooldown { get; protected set; }

	public Vector3 startPosition { get; protected set; }

	public Quaternion startRotation { get; protected set; }

	public Transform t { get; private set; }

	public Transform tMesh { get; private set; }

	public Transform tBody { get; private set; }

	public Rigidbody rb { get; private set; }

	public CapsuleCollider clldr { get; private set; }

	public Flammable flammable { get; private set; }

	public Animator animator { get; private set; }

	public StateMachine stateMachine { get; private set; }

	public NavMeshAgent agent { get; private set; }

	public Body body { get; private set; }

	public EnemyMaterial mat { get; private set; }

	public float dist { get; private set; }

	public float distGrounded { get; private set; }

	public float distVertical { get; private set; }

	public void Warning()
	{
		QuickEffectsPool.Get("Warning", tTrailPivot.position, Quaternion.LookRotation(t.forward)).Play();
	}

	public bool GetFireStatus()
	{
		if (!base.isActiveAndEnabled)
		{
			return body.flammable.onFire;
		}
		return flammable.onFire;
	}

	public bool GetKnockedStatus()
	{
		if (!base.isActiveAndEnabled)
		{
			return body.lifetime != 0f;
		}
		return false;
	}

	public Vector3 GetActualPosition()
	{
		if (!base.isActiveAndEnabled)
		{
			return body.rb.worldCenterOfMass;
		}
		return rb.worldCenterOfMass;
	}

	public Vector3 GetActualTrailPivotPosition()
	{
		if (!base.isActiveAndEnabled)
		{
			return body.tTrailPivot.position;
		}
		return tTrailPivot.position;
	}

	public Transform GetActualTrailPivotTransform()
	{
		if (!base.isActiveAndEnabled)
		{
			return body.tTrailPivot;
		}
		return tTrailPivot;
	}

	public void ResetPositionAndRotation()
	{
		t.SetPositionAndRotation(startPosition, startRotation);
	}

	public void DeactivateEnemy()
	{
		health = maxHealth;
		dead = false;
		if (body.gameObject.activeInHierarchy)
		{
			body.Reset();
			body.DeactivateBody();
		}
		if (OnEnemyDeactivated != null)
		{
			OnEnemyDeactivated(this);
		}
		base.gameObject.SetActive(value: false);
	}

	public void ActivateEnemy()
	{
		base.gameObject.SetActive(value: true);
		SetTarget(Game.player.t);
		if (!shieldDamageType)
		{
			shield.gameObject.SetActive(value: false);
		}
		else
		{
			shield.gameObject.SetActive(value: true);
			shieldDestroyed = false;
		}
		mat.SetFloatByName("_GlowPower", buffed ? 1 : 0);
		body.mat.SetFloatByName("_GlowPower", buffed ? 1 : 0);
		buffedFX.gameObject.SetActive(buffed);
		body.buffedFX.SetActive(buffed);
		if (OnEnemyActivated != null)
		{
			OnEnemyActivated(this);
		}
		PlaySound(sounds.Spawn);
	}

	public void Teleport(Vector3 pos)
	{
		agent.Warp(pos);
		t.LookAt(tTarget.position.With(null, t.position.y));
		actionTime = 1f;
		stateMachine.SwitchState(typeof(EnemyActionState));
		isTeleporting = false;
	}

	protected virtual void Awake()
	{
		if (!CrowdControl.allEnemies.Contains(this))
		{
			CrowdControl.allEnemies.Add(this);
		}
		health = maxHealth;
		t = base.transform;
		if ((bool)particleDash)
		{
			particleDash.transform.SetParent(null);
		}
		RefreshStartPositionAndRotation();
		tMesh = t.Find("Mesh").transform;
		rb = GetComponent<Rigidbody>();
		clldr = GetComponent<CapsuleCollider>();
		rb.centerOfMass = clldr.center + Vector3.up * 0.25f;
		animator = GetComponentInChildren<Animator>();
		stateMachine = GetComponent<StateMachine>();
		agent = GetComponent<NavMeshAgent>();
		flammable = GetComponentInChildren<Flammable>();
		flammable.enemy = this;
		body = UnityEngine.Object.Instantiate(bodyPrefab).GetComponent<Body>();
		body.Setup(this);
		tBody = body.transform;
		mat = GetComponentInChildren<EnemyMaterial>();
		mat.Setup();
		mat.SetFloatByName("_Power", health / maxHealth);
		mat.SetFloatByName("_GlowPower", buffed ? 1 : 0);
		body.mat.SetFloatByName("_GlowPower", buffed ? 1 : 0);
		buffedFX.gameObject.SetActive(buffed);
		body.buffedFX.SetActive(buffed);
		shield = GetComponentInChildren<EnemyShield>(includeInactive: true);
		shield.Setup();
		if (!shieldDamageType)
		{
			shieldDestroyed = true;
			shield.gameObject.SetActive(value: false);
		}
		else
		{
			shield.gameObject.SetActive(value: true);
			shieldDestroyed = false;
		}
		PlayerController.OnPlayerDie = (Action)Delegate.Combine(PlayerController.OnPlayerDie, new Action(OnPlayerDie));
		PlayerController.OnNewGroundedPoint = (Action)Delegate.Combine(PlayerController.OnNewGroundedPoint, new Action(CheckTarget));
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Combine(BreakableB.OnBreak, new Action<GameObject>(CheckGround));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
		QuickmapScene.OnPlayMode = (Action)Delegate.Combine(QuickmapScene.OnPlayMode, new Action(RefreshStartPositionAndRotation));
		ThrowedDagger.OnTargetDetected = (Action<BaseEnemy>)Delegate.Combine(ThrowedDagger.OnTargetDetected, new Action<BaseEnemy>(OnDaggerThrow));
	}

	protected virtual void OnDestroy()
	{
		if (CrowdControl.allEnemies.Contains(this))
		{
			CrowdControl.allEnemies.Remove(this);
		}
		PlayerController.OnPlayerDie = (Action)Delegate.Remove(PlayerController.OnPlayerDie, new Action(OnPlayerDie));
		PlayerController.OnNewGroundedPoint = (Action)Delegate.Remove(PlayerController.OnNewGroundedPoint, new Action(CheckTarget));
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Remove(BreakableB.OnBreak, new Action<GameObject>(CheckGround));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
		QuickmapScene.OnPlayMode = (Action)Delegate.Remove(QuickmapScene.OnPlayMode, new Action(RefreshStartPositionAndRotation));
		ThrowedDagger.OnTargetDetected = (Action<BaseEnemy>)Delegate.Remove(ThrowedDagger.OnTargetDetected, new Action<BaseEnemy>(OnDaggerThrow));
	}

	protected virtual void OnDaggerThrow(BaseEnemy e)
	{
		if (this == e)
		{
			ReactToDagger();
		}
	}

	protected virtual void ReactToDagger()
	{
	}

	private void RefreshStartPositionAndRotation()
	{
		startPosition = t.position;
		startRotation = t.rotation;
	}

	private void CheckGround(GameObject obj)
	{
		Physics.OverlapSphereNonAlloc(t.position, agent.radius, colliders);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] != null)
			{
				if (colliders[i].gameObject == obj)
				{
					Kick(Vector3.up);
				}
				colliders[i] = null;
			}
		}
	}

	public virtual void Buff(bool value)
	{
		if (buffed != value)
		{
			buffed = value;
			mat.SetFloatByName("_GlowPower", buffed ? 1 : 0);
			body.mat.SetFloatByName("_GlowPower", buffed ? 1 : 0);
		}
	}

	protected virtual void OnEnable()
	{
		repositionTimer = UnityEngine.Random.Range(1f, 2f);
		randomSeed = UnityEngine.Random.Range(0, 100);
		tMesh.localEulerAngles = initialMeshAngles;
		isStrafing = 0f;
		animator.Update(0f);
		if (dead)
		{
			health = maxHealth;
			dead = false;
		}
		else
		{
			mat.ResetBlink();
		}
		mat.SetFloatByName("_Power", health / maxHealth);
	}

	protected virtual void OnDisable()
	{
		stateMachine.SwitchState(typeof(EnemyIdleState));
		if (!agent.enabled)
		{
			agent.enabled = true;
		}
	}

	public virtual void OnLanded()
	{
	}

	public virtual void OnDashed()
	{
		if (lastDamageType != null)
		{
			lastDamageType = null;
		}
	}

	public virtual void Reset()
	{
		lastDamageType = null;
		if (!ManualReset)
		{
			base.gameObject.SetActive(value: false);
			base.gameObject.SetActive(value: true);
			SetTarget(null);
			stateMachine.SwitchState(typeof(EnemyIdleState));
			dead = false;
			isTeleporting = false;
			health = maxHealth;
			mat.SetFloatByName("_Power", health / maxHealth);
			base.gameObject.SetActive(value: true);
			tHeadAngles.x = (tHeadAngles.y = (tHeadAngles.z = 0f));
			tHeadMesh.localEulerAngles = tHeadAngles;
			if (animator.GetBool("Running"))
			{
				animator.SetBool("Running", value: false);
			}
			if (body.gameObject.activeInHierarchy)
			{
				body.DeactivateBody();
			}
			CheckLink();
			body.Reset();
			if (!agent.enabled)
			{
				agent.enabled = true;
			}
			agent.Warp(startPosition);
			if (agent.isOnNavMesh)
			{
				agent.ResetPath();
			}
			if ((bool)flammable)
			{
				flammable.SetOnFire(value: false);
			}
			if ((bool)shieldDamageType)
			{
				shieldDestroyed = false;
				shield.gameObject.SetActive(value: true);
			}
			t.rotation = startRotation;
			tTarget = null;
		}
	}

	private bool TryToTeleport()
	{
		Vector3 normalized = tTarget.position.DirTo(t.position).With(null, 0f).normalized;
		if (NavMesh.SamplePosition(tTarget.position + normalized * 6f, out navHit, 1f, -1))
		{
			isTeleporting = true;
			(QuickPool.instance.Get("EnemySpawnPoint", navHit.position) as EnemySpawnPoint).enemyToActivate = this;
			base.gameObject.SetActive(value: false);
			return true;
		}
		return false;
	}

	public void UpdateReposition()
	{
		if (repositionTimer <= 0f)
		{
			if (agent.enabled && !isTeleporting && !flammable.onFire && !stateMachine.CurrentIs(typeof(EnemyActionState)) && !(distVertical.Abs() > verticalReposDist) && distGrounded > minReposDist && distGrounded < maxReposDist && Game.player.ViewAnglePlane(t.position) <= 90f)
			{
				Vector3 vector = GetTargetDirGrounded();
				vector = Quaternion.Euler(0f, UnityEngine.Random.Range((0f - dashAngle) / 2f, dashAngle / 2f), 0f) * vector;
				if (NavMesh.SamplePosition(t.position + vector * ((float)dashSign * dashDist), out navHit, 1f, -1))
				{
					if (!Physics.Linecast(navHit.position + Vector3.up, t.position + Vector3.up, 1))
					{
						targetPosition = navHit.position;
						stateMachine.SwitchState(typeof(EnemyDashState));
						Debug.DrawLine(navHit.position, t.position, Color.green, 2f);
					}
					else
					{
						Debug.DrawLine(navHit.position, t.position, Color.red, 2f);
					}
				}
				else
				{
					Debug.DrawLine(navHit.position, t.position, Color.magenta, 2f);
				}
			}
			repositionTimer = UnityEngine.Random.Range(2f, 4f);
		}
		else
		{
			repositionTimer -= Time.deltaTime;
		}
	}

	public bool CheckForNavMesh()
	{
		if (agent.SamplePathPosition(-1, 1f, out var navMeshHit))
		{
			t.position = navMeshHit.position;
			return true;
		}
		return false;
	}

	protected virtual void UpdateTargetPosition()
	{
		targetPosition = tTarget.position;
	}

	public virtual void RunningUpdate()
	{
		float sqrMagnitude = agent.velocity.sqrMagnitude;
		if (sqrMagnitude > 1f)
		{
			animator.speed = 0.1f + sqrMagnitude / (agent.speed * agent.speed);
			t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(agent.velocity), Time.deltaTime * agent.speed);
		}
		else
		{
			animator.speed = Mathf.Lerp(animator.speed, 1f, Time.deltaTime);
		}
	}

	public virtual void FollowBreak()
	{
	}

	public virtual void SetTarget(Transform newTarget)
	{
		if (tTarget != newTarget)
		{
			tTarget = newTarget;
		}
	}

	public Vector3 GetTargetDirGrounded()
	{
		return t.position.DirTo(tTarget.position.With(null, t.position.y));
	}

	protected void CheckTarget()
	{
		if (!base.isActiveAndEnabled || !Game.player || Game.player.isDead)
		{
			return;
		}
		float num = (rb.worldCenterOfMass.y - Game.player.t.position.y).Abs();
		if (!tTarget)
		{
			if (num < maxDetectionHeight)
			{
				CheckTargetWithRay();
			}
		}
		else
		{
			if (!agent.enabled || stateMachine.CurrentIs(typeof(EnemyActionState)))
			{
				return;
			}
			if (num > 20f)
			{
				targetIsTooFar++;
				if (targetIsTooFar > 20)
				{
					stateMachine.SwitchState(typeof(EnemyIdleState));
					SetTarget(null);
					if (OnEnemyActivated != null)
					{
						OnEnemyActivated(this);
					}
				}
			}
			else if (targetIsTooFar > 0)
			{
				targetIsTooFar = 0;
			}
		}
	}

	public bool CheckTargetWithRay(float dist = 24f)
	{
		if (!base.isActiveAndEnabled)
		{
			return false;
		}
		Physics.Raycast(rb.worldCenterOfMass, rb.worldCenterOfMass.DirTo(Game.player.t.position), out hit, dist, 49665);
		if ((bool)hit.collider && hit.collider.gameObject.layer == 9)
		{
			Debug.DrawLine(hit.point, t.position, Color.cyan, 2f);
			if (tTarget != Game.player.t)
			{
				SetTarget(Game.player.t);
				if (OnEnemyActivated != null)
				{
					OnEnemyActivated(this);
				}
			}
			return true;
		}
		return false;
	}

	public virtual void OnPlayerDie()
	{
		SetTarget(null);
		stateMachine.SwitchState(typeof(EnemyIdleState));
	}

	public virtual void AnimationEvent(int index)
	{
	}

	public virtual void Stun()
	{
	}

	public bool LastGroundedBodyPosition(Vector3 pos, ref Vector3 recoverPos)
	{
		if (NavMesh.SamplePosition(pos, out navHit, 0.25f, -1))
		{
			recoverPos = navHit.position;
			return true;
		}
		return false;
	}

	public bool GetPreventFallingPosition(Vector3 from)
	{
		if (NavMesh.FindClosestEdge(from, out navHit, -1))
		{
			Debug.DrawLine(from, navHit.position, Color.yellow, 2f);
			if (NavMesh.SamplePosition(navHit.position + from.DirTo(navHit.position).With(null, 0f).normalized * 3f, out navHit, 1f, -1))
			{
				Debug.DrawLine(from, navHit.position, Color.green, 2f);
				targetPosition = navHit.position;
				return true;
			}
		}
		return false;
	}

	public Vector3 SetClosestEdgeAsTargetPosition()
	{
		agent.FindClosestEdge(out navHit);
		targetPosition = navHit.position + t.position.DirTo(navHit.position).With(null, 0f).normalized * 3f;
		return navHit.position;
	}

	public virtual void SpawnBody()
	{
		CheckLink();
		tBody.SetPositionAndRotation(t.position, t.rotation);
		body.OnEnable();
		body.flammable.SetOnFire(flammable.onFire);
	}

	public void InvokeDeathAction(float damage)
	{
		CameraController.shake.Shake(1);
		Vector3 forward = default(Vector3).Random();
		if (damage < 999f)
		{
			QuickPool.instance.Get("Blood", GetActualPosition(), Quaternion.LookRotation(forward));
		}
		SetTarget(null);
		if (OnEnenyDie != null)
		{
			OnEnenyDie(this);
		}
		if (OnAnyEnemyDie != null)
		{
			OnAnyEnemyDie();
		}
	}

	public float GetHealthPercentage()
	{
		return health / maxHealth;
	}

	public virtual void ChangeHealth(float value)
	{
		health -= value.Abs();
		mat.SetFloatByName("_Power", health / maxHealth);
		if (health <= 0f)
		{
			dead = true;
		}
	}

	public virtual void Kick(Vector3 dir)
	{
	}

	public virtual void ActualDamage(DamageData damage)
	{
		if (damage.amount < minStaggerDamage || buffed || damageCooldown > 0f)
		{
			return;
		}
		damageCooldown = 1.25f;
		if (ActionStateWithAnim("Damage", 0.3f))
		{
			Vector3 normalized = Vector3.ProjectOnPlane(damage.dir, Vector3.up).normalized;
			agent.Warp(t.position + normalized);
			if (normalized.sqrMagnitude != 0f)
			{
				t.rotation = Quaternion.LookRotation(-normalized);
			}
		}
	}

	public virtual void Stagger(Vector3 dir)
	{
		if (!agent.enabled && knockable)
		{
			base.gameObject.SetActive(value: false);
			SpawnBody();
			body.PushBody(dir);
		}
		else if (ActionStateWithAnim("Stagger", 0.5f))
		{
			Vector3 normalized = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
			agent.Warp(t.position + normalized);
			if (normalized.sqrMagnitude != 0f)
			{
				t.rotation = Quaternion.LookRotation(-normalized);
			}
		}
	}

	public virtual void Block(Vector3 dir)
	{
		if (ActionStateWithAnim("Block", 1.75f))
		{
			QuickEffectsPool.Get("Block", clldr.bounds.center, t.rotation).Play();
			CameraController.shake.Shake(1);
		}
	}

	public virtual void Die(DamageData damage)
	{
		health = 0f;
		base.gameObject.SetActive(value: false);
		SpawnBody();
		body.Damage(damage);
	}

	public virtual void Damage(DamageData damage)
	{
		if (dead)
		{
			return;
		}
		lastDamaged = this;
		if (OnDamage != null)
		{
			OnDamage(this);
		}
		if ((bool)linkedSouls && linkedSouls.isActiveAndEnabled)
		{
			QuickEffectsPool.Get("Block", rb.worldCenterOfMass, t.rotation).Play();
			if (damage.amount > 0f)
			{
				linkedSouls.SetDamage(this, damage.newType);
			}
			return;
		}
		if (!base.isActiveAndEnabled)
		{
			body.Damage(damage);
			return;
		}
		if (damage.newType.stugger)
		{
			ActionStateWithAnim("Block");
		}
		float num = Vector3.Angle(-damage.dir, t.forward);
		bool flag = num > 100f && damage.dir.y.Abs() < 0.5f;
		if ((buffed && agent.enabled && damage.amount < 999f && (!lastDamageType || damage.newType == lastDamageType) && !flag && !blessedWeakness.Contains(damage.newType)) || (canBlock && agent.enabled && num < 60f && !stateMachine.CurrentIs(typeof(EnemyActionState))))
		{
			lastDamageType = damage.newType;
			if ((bool)tTarget)
			{
				t.rotation = Quaternion.LookRotation(t.position.DirTo(tTarget.position).With(null, t.position.y), Vector3.up);
			}
			else
			{
				t.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-damage.dir, t.up), Vector3.up);
			}
			ActionStateWithAnim("Block", 0.75f);
			QuickEffectsPool.Get("Block", rb.worldCenterOfMass, t.rotation).Play();
			CameraController.shake.Shake(1);
			return;
		}
		if (buffed && flag)
		{
			StyleRanking.instance.RegStylePoint(Game.style.backStab);
		}
		lastDamageType = damage.newType;
		if (damage.newType.fire)
		{
			flammable.SetOnFire();
		}
		if (!shieldDestroyed && ((bool)shieldDamageType || damage.amount == 999f))
		{
			if (!(damage.newType == shieldDamageType))
			{
				shield.DamageReaction();
				CameraController.shake.Shake(1);
				return;
			}
			shieldDestroyed = true;
			shield.gameObject.SetActive(value: false);
			Stagger(damage.dir);
			CameraController.shake.Shake(1);
			QuickEffectsPool.Get("Orb Explosion", rb.worldCenterOfMass).Play();
		}
		if (health - damage.amount > 0f)
		{
			if (knockable && (damage.amount > kickMinDamage || damage.knockdown || !agent.enabled))
			{
				if (stateMachine.CurrentIs(typeof(EnemyJumpState)))
				{
					StyleRanking.instance.AddStylePoint(StylePointTypes.KnockedJump);
				}
				base.gameObject.SetActive(value: false);
				SpawnBody();
				body.Damage(damage, withKick: true);
			}
			else if (damage.amount != 0f)
			{
				ActualDamage(damage);
				if ((bool)damage.newType)
				{
					damage.newType.Callback(this);
				}
				health -= damage.amount;
				mat.Blink(damage.amount / 50f);
				mat.SetFloatByName("_Power", health / maxHealth);
				PlaySound(sounds.Hurt);
				QuickEffectsPool.Get("Damage", t.position + Vector3.up * 1.5f, Quaternion.LookRotation(damage.dir)).Play();
			}
		}
		else
		{
			Die(damage);
		}
	}

	protected bool ActionStateWithAnim(string animStateName, float timer = 1f)
	{
		if (agent.enabled)
		{
			animator.Play(animStateName, -1, 0f);
			actionTime = timer;
			stateMachine.SwitchState(typeof(EnemyActionState));
			return true;
		}
		return false;
	}

	protected bool ActionStateWithTrigger(string triggerName, float timer = 1f)
	{
		if (agent.enabled)
		{
			animator.SetTrigger(triggerName);
			actionTime = timer;
			stateMachine.SwitchState(typeof(EnemyActionState));
			return true;
		}
		return false;
	}

	protected void UpdateTargetDirections()
	{
		if ((bool)tTarget)
		{
			targetPos = tTarget.position;
			targetPos.y -= 1f;
			targetDir = t.position.DirTo(targetPos);
			targetDirGrounded = targetDir.With(null, 0f).normalized;
		}
	}

	protected void UpdateTargetDistances()
	{
		if ((bool)tTarget)
		{
			targetPos = tTarget.position;
			targetPos.y -= 1f;
			dist = Vector3.Distance(t.position, targetPos);
			distGrounded = Vector3.Distance(t.position, targetPos.With(null, t.position.y));
			distVertical = (targetPos.y - t.position.y).Abs();
		}
	}

	private void LateUpdate()
	{
		onNavMeshLink = agent.isOnOffMeshLink;
		if ((bool)tHeadMesh && (bool)Game.player && (bool)tTarget)
		{
			_tempRotation = Quaternion.LookRotation(t.InverseTransformDirection(tHead.position.DirTo(Game.player.tHead.position)));
			float num = _tempRotation.eulerAngles.y.WrapAngle();
			tHeadAngles.x = (tHeadAngles.y = 0f);
			tHeadAngles.z = Mathf.LerpAngle(tHeadMesh.localEulerAngles.z, (num.Abs() > 75f) ? 0f : num, Time.deltaTime * 8f);
			tHeadMesh.localEulerAngles = tHeadAngles;
		}
		if (shield.isActiveAndEnabled)
		{
			shield.Tick();
		}
		if (isStrafing != 0f)
		{
			isStrafing = Mathf.MoveTowards(isStrafing, 0f, Time.deltaTime * 4f);
		}
		if (damageCooldown != 0f)
		{
			damageCooldown = Mathf.MoveTowards(damageCooldown, 0f, Time.deltaTime);
		}
	}

	protected void AttackWithTrailBounds(TrailScript trail)
	{
		clldr.enabled = false;
		UpdateTargetDirections();
		if (Vector3.Angle(t.forward, targetDir) < attackCorrectionRadius)
		{
			t.rotation = Quaternion.LookRotation(targetDirGrounded, Vector3.up);
		}
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] != null)
			{
				colliders[i] = null;
			}
		}
		Physics.OverlapBoxNonAlloc(trail.t.position, trail.filter.sharedMesh.bounds.extents * 1.1f, colliders, trail.t.rotation, 17920);
		for (int j = 0; j < colliders.Length; j++)
		{
			if (colliders[j] == null)
			{
				continue;
			}
			if (colliders[j].gameObject.layer == 9)
			{
				attackTarget = colliders[j].GetComponentInChildren<IDamageable<Vector4>>();
				if (attackTarget != null)
				{
					CrowdControl.lastAttacked = this;
					attackInfo = targetDir;
					attackInfo.y = 0f;
					attackInfo.w = attackDamage;
					attackTarget.Damage(attackInfo);
					CrowdControl.lastAttacked = null;
					break;
				}
			}
			else if (friendlyFire || GetHealthPercentage() < 0.25f)
			{
				friendlyDamageInfo.dir = (t.forward * ((t.InverseTransformPoint(colliders[j].transform.position).z > 0f) ? 1 : (-1)) + t.up) / 2f;
				friendlyDamageInfo.amount = 10f;
				friendlyDamageInfo.knockdown = true;
				friendlyDamageInfo.newType = Game.style.basicBluntHit;
				colliders[j].GetComponent<IDamageable<DamageData>>().Damage(friendlyDamageInfo);
				StyleRanking.instance.AddStylePoint(StylePointTypes.FriendlyFire);
			}
			colliders[j] = null;
		}
		clldr.enabled = true;
	}

	public bool CheckNavMeshPos(ref Vector3 target, Vector3 pos, float radius = 0.5f)
	{
		if (NavMesh.SamplePosition(pos, out navHit, radius, -1))
		{
			target = navHit.position;
			return true;
		}
		return false;
	}

	public bool CheckJumpPosAtPosition(ref Vector3 target, Vector3 pos, float atHeight = 2f, float depth = 4f, float maxDist = 16f)
	{
		Vector3 worldCenterOfMass = rb.worldCenterOfMass;
		worldCenterOfMass.y += atHeight;
		Vector3 vector = pos;
		vector.y = worldCenterOfMass.y;
		if (Physics.Linecast(worldCenterOfMass, vector, out hit, 1))
		{
			if (!(hit.distance > 2f) || !(hit.distance < maxDist))
			{
				Debug.DrawLine(worldCenterOfMass, hit.point, Color.red, 2f);
				return false;
			}
			vector = hit.point + hit.normal;
		}
		Debug.DrawLine(worldCenterOfMass, vector, Color.cyan, 2f);
		Physics.Raycast(vector, Vector3.down, out hit, depth, 1);
		if (hit.distance != 0f)
		{
			Debug.DrawLine(vector, hit.point, Color.cyan, 2f);
		}
		if (hit.distance == 0f)
		{
			return false;
		}
		return CheckNavMeshPos(ref target, hit.point, 0.25f);
	}

	public bool CheckJumpPosInDirection(ref Vector3 target, Vector3 dir, float dist = 6f, float atHeight = 2f, float depth = 4f)
	{
		Vector3 worldCenterOfMass = rb.worldCenterOfMass;
		worldCenterOfMass.y += atHeight;
		Vector3 vector = worldCenterOfMass + dir.normalized * dist;
		if (Physics.Linecast(worldCenterOfMass, vector, out hit, 1))
		{
			if (!(hit.distance > 2f))
			{
				Debug.DrawLine(worldCenterOfMass, hit.point, Color.red, 2f);
				return false;
			}
			vector = hit.point + hit.normal;
		}
		Debug.DrawLine(worldCenterOfMass, vector, Color.cyan, 2f);
		Physics.Raycast(vector, Vector3.down, out hit, depth, 1);
		if (hit.distance != 0f)
		{
			Debug.DrawLine(vector, hit.point, Color.cyan, 2f);
		}
		if (hit.distance == 0f)
		{
			return false;
		}
		return CheckNavMeshPos(ref target, hit.point, 0.25f);
	}

	public bool CheckJumpPosNearPlayer(ref Vector3 target, float maxHeight = 18f, float atDist = 2f)
	{
		Physics.Linecast(rb.worldCenterOfMass, Game.player.t.position, out hit, 1);
		if (hit.distance != 0f)
		{
			return false;
		}
		Debug.DrawLine(rb.worldCenterOfMass, Game.player.t.position, Color.red, 2f);
		Physics.Raycast(Game.player.t.position, -Vector3.up, out hit, maxHeight, 1);
		if (hit.distance == 0f)
		{
			return false;
		}
		return CheckNavMeshPos(ref target, hit.point + hit.point.DirToXZ(t.position) * atDist);
	}

	public virtual bool CheckViewPoints()
	{
		if (!ViewPoints.instance)
		{
			return false;
		}
		if (ViewPoints.instance.GetClosest(t.position, out var result))
		{
			targetPosition = result;
			stateMachine.SwitchState(typeof(EnemyJumpState));
			return true;
		}
		return false;
	}

	protected bool CheckWarpPos(Vector3 pos)
	{
		if (NavMesh.SamplePosition(pos, out navHit, 0.5f, -1))
		{
			warpPos = navHit.position;
			return true;
		}
		warpPos = Vector3.zero;
		return false;
	}

	protected virtual void Warp()
	{
		agent.Warp(warpPos);
	}

	protected bool CheckForOffMeshLinks()
	{
		if (agent.enabled && agent.isOnOffMeshLink)
		{
			if (!directJumpCheck || !TryDirectJump())
			{
				link = agent.currentOffMeshLinkData.offMeshLink;
				if ((bool)link)
				{
					link.activated = false;
					targetPosition = agent.currentOffMeshLinkData.endPos;
					stateMachine.SwitchState(typeof(EnemyJumpState));
					return true;
				}
				Debug.Log("No Link");
				return false;
			}
			return false;
		}
		return false;
	}

	public void CheckLink()
	{
		if ((bool)link)
		{
			link.activated = true;
			link = null;
		}
	}

	protected bool TryJump(Vector3 pos)
	{
		if (NavMesh.SamplePosition(hit.point, out navHit, agent.radius, -1))
		{
			targetPosition = navHit.position;
			stateMachine.SwitchState(typeof(EnemyJumpState));
			return true;
		}
		return false;
	}

	public bool TryJumpOn()
	{
		UpdateTargetDirections();
		Physics.Raycast(t.position + (targetDirGrounded * 3f).With(null, 7f), Vector3.down, out hit, 6f, 1);
		if (hit.distance == 0f)
		{
			return false;
		}
		return TryJump(hit.point);
	}

	public bool TryJumpOff()
	{
		UpdateTargetDirections();
		Physics.Raycast(t.position + t.up + targetDirGrounded * 4f, Vector3.down, out hit, 16f, 1);
		if (hit.distance == 0f)
		{
			return false;
		}
		return TryJump(hit.point);
	}

	public bool TryDirectJump()
	{
		if (Game.player.ViewAnglePlane(t.position) > 90f)
		{
			return false;
		}
		UpdateTargetDirections();
		Physics.Raycast(t.position + t.up * 1.5f, targetDir, out hit, 14f, 1 | (1 << tTarget.gameObject.layer));
		if (hit.transform == tTarget)
		{
			Physics.Raycast(hit.point - targetDirGrounded, Vector3.down, out hit, 3f, 1);
			if (hit.distance == 0f)
			{
				return false;
			}
			return TryJump(hit.point);
		}
		if (hit.distance != 0f)
		{
			CheckViewPoints();
		}
		Debug.DrawRay(t.position + t.up * 1.5f, targetDir.normalized * 18f, Color.red, 2f);
		return false;
	}

	public void PlaySound(AudioClip clip)
	{
		source.PlayClip(clip);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!clldr.isTrigger)
		{
			return;
		}
		switch (other.gameObject.layer)
		{
		case 9:
			if (!Game.player.slide.isSliding)
			{
				Game.player.Damage(t.forward);
				if (knockable)
				{
					SpawnBody();
					body.PushBody(-t.forward);
					base.gameObject.SetActive(value: false);
				}
			}
			break;
		case 14:
			if (!other.attachedRigidbody.isKinematic && other.attachedRigidbody.velocity.sqrMagnitude > 8f)
			{
				bluntHitDamage.dir = (stateMachine.CurrentIs(typeof(EnemyDashState)) ? (-t.forward) : t.forward);
				bluntHitDamage.amount = 40f;
				bluntHitDamage.knockdown = true;
				bluntHitDamage.newType = Game.style.basicBluntHit;
				Damage(bluntHitDamage);
				other.attachedRigidbody.velocity = GetActualPosition().DirTo(other.bounds.center) * 10f;
			}
			break;
		}
	}

	private void OnDrawGizmos()
	{
		if (buffed)
		{
			Gizmos.DrawIcon(base.transform.position + Vector3.up * 4f, "BuffedEnemy");
		}
	}

	private void OnDrawGizmosSelected()
	{
		if ((bool)tTarget)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(t.position, targetPosition);
			Gizmos.DrawSphere(targetPosition, 0.5f);
		}
	}
}
