using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Body : MonoBehaviour
{
	public static Action<GameObject> OnDie = delegate
	{
	};

	public static Action<DamageData, BaseEnemy> OnDamage = delegate
	{
	};

	public static Action<Body> OnDeactivate = delegate
	{
	};

	public bool preventKnocking;

	public bool preventFreeFall;

	private float freefallPreventTimer;

	private float knockoutPreventDelay;

	public Weapon weaponData;

	public Transform tTrailPivot;

	public Transform tWeaponRoot;

	private Vector3 force = new Vector3(0f, 7f, 0f);

	private Vector3 torque = new Vector3(90f, 90f, 90f);

	[Header("Components")]
	public ParticleSystem particle;

	public ParticleSystem bloodParticle;

	public GameObject buffedFX;

	public Projector shadowProjector;

	public AudioSource source;

	public BodySounds sounds;

	[Header("Materials")]
	public Material matDefault;

	public Material matDead;

	[Header("Physics")]
	public int indexCoreBody;

	public float recoverTime = 0.5f;

	public float maxStunTime = 5f;

	public int[] rippableJoints;

	public int ripableJoint;

	public int ripableJointB;

	public Collider clldr;

	public Rigidbody[] freeRigidbodies = new Rigidbody[0];

	protected float timer;

	protected float highestPoint;

	private bool stunned;

	private int pinState;

	private float pinTimer;

	private float stunTimer;

	private float shadowSize;

	public Vector3 recoverPos;

	private Vector3 temp;

	private Vector3 tempDir;

	private Vector3 spinNormal;

	private Vector3 pinPoint;

	private Vector3 pinNormal;

	private Quaternion deltaRotation;

	private Vector3 lastGroundCheckPos;

	private RaycastHit hit;

	protected DamageData lastDamage = new DamageData();

	private DamageData dmgInfo = new DamageData();

	private Vector3 zero = Vector3.zero;

	private List<RippableJoint> rippedJoints = new List<RippableJoint>(3);

	private RippableJoint tempHealableJoint = new RippableJoint();

	private BodyCollider bodyCollider;

	private MeshRenderer[] rends;

	private CanBeDrowned canBeDrowned;

	private Coroutine slamming;

	private WaitForSeconds waitForPointOneSec = new WaitForSeconds(0.1f);

	private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

	public float damageTimer { get; private set; }

	public BaseEnemy enemy { get; private set; }

	public Transform tRoot { get; private set; }

	public Transform t { get; private set; }

	public Rigidbody rb { get; private set; }

	public Rigidbody[] rbs { get; private set; }

	public Transform[] tRbs { get; private set; }

	public Flammable flammable { get; private set; }

	public EnemyMaterial mat { get; private set; }

	public float lifetime { get; private set; }

	public float fallDist { get; private set; }

	public int fallDistStep { get; private set; }

	public float deadTimer { get; private set; }

	public Vector3 oldVel { get; private set; }

	public Joint pinJoint { get; private set; }

	public void ResetFallDist()
	{
		highestPoint = t.position.y;
		int num2 = (fallDistStep = 0);
		fallDist = num2;
	}

	public virtual void Damage(DamageData damage, bool withKick = false)
	{
		if (slamming != null)
		{
			return;
		}
		BaseEnemy.lastDamaged = enemy;
		damage.CopyTo(lastDamage);
		enemy.lastDamageType = damage.newType;
		if (lifetime > 0f && damage.amount > 0f)
		{
			damageTimer = 0.5f;
		}
		if (preventFreeFall && lifetime > 0f)
		{
			freefallPreventTimer = 2f;
		}
		if (damage.amount > 0f)
		{
			mat.Blink();
		}
		if (damage.newType.fire)
		{
			flammable.SetOnFire();
		}
		if (damage.amount > 0f)
		{
			QuickEffectsPool.Get("Damage", rb.position, Quaternion.LookRotation(damage.dir)).Play();
		}
		if (enemy.dead)
		{
			return;
		}
		enemy.ChangeHealth(damage.amount * (((lifetime == 0f) ? 1f : 1.5f) + (flammable.onFire ? 0.25f : 0f)));
		if (stunned && (bool)enemy.weapon && enemy.GetHealthPercentage() < 0.5f && !WeaponsControl.allWeapons.Contains(tRbs[0]))
		{
			WeaponsControl.allWeapons.Add(tRbs[0]);
		}
		mat.SetFloatByName("_Power", enemy.health / enemy.maxHealth);
		if (!enemy.dead)
		{
			PlaySound(enemy.sounds.Hurt);
			if (OnDamage != null)
			{
				OnDamage(damage, enemy);
			}
			if ((bool)damage.newType)
			{
				if (damage.newType.pushBody)
				{
					PushBody(damage.dir + damage.newType.pushDirOffset, damage.newType.pushForce);
				}
				if (damage.newType.rotateBody && lifetime != 0f)
				{
					RotateBody(damage.newType.rotation.x, damage.newType.rotation.y, damage.newType.rotation.y);
				}
				if (damage.newType.stun)
				{
					Stun();
				}
				if (damage.newType.kick)
				{
					if (damage.newType.slideKick)
					{
						SlideKick(damage.dir);
					}
					else
					{
						Kick((damage.newType.overrideKickDir.sqrMagnitude > 0f) ? damage.newType.overrideKickDir : damage.dir);
					}
				}
				damage.Callback(enemy);
			}
			else
			{
				Debug.Log("NO DAMAGE TYPE 1");
			}
		}
		else
		{
			FatalDamage();
			Die();
		}
	}

	private void Die()
	{
		DropSomething();
		StopStun();
		buffedFX.gameObject.SetActive(value: false);
		particle.Stop();
		bloodParticle.Play();
		for (int i = 0; i < rends.Length; i++)
		{
			rends[i].sharedMaterial = matDead;
		}
		enemy.InvokeDeathAction(lastDamage.amount);
		if (flammable.onFire && flammable.SetOnFire(value: false))
		{
			QuickPool.instance.Get("Fire Poof", rb.position);
		}
		PlaySound(enemy.sounds.Die);
		mat.SetFloatByName("_Power", 0f);
		deadTimer = 0f;
		if (clldr.enabled)
		{
			clldr.enabled = false;
		}
		if (OnDie != null)
		{
			OnDie(base.gameObject);
		}
	}

	public virtual void DropSomething()
	{
		if ((bool)weaponData && lastDamage.newType != Game.style.basicMill)
		{
			Vector3 v = force + rb.position.DirTo(PlayerController.instance.t.position.With(null, rb.position.y)) * 2f;
			PooledWeapon pooledWeapon = QuickPool.instance.Get(weaponData.prefabPickable, rb.position + Vector3.up, v.Quaternionise()) as PooledWeapon;
			Physics.Raycast(rb.position, -lastDamage.dir, out hit, 4f, 1);
			if (hit.distance == 0f)
			{
				pooledWeapon.rb.AddForceAndTorque(v, torque);
			}
			else
			{
				pooledWeapon.rb.AddForceAndTorque(hit.normal * 7f, torque);
				Debug.DrawRay(hit.point, hit.normal * 3f, Color.magenta, 3f);
			}
			pooledWeapon.SetHotTimer(1f);
			if (tWeaponRoot.gameObject.activeInHierarchy)
			{
				tWeaponRoot.gameObject.SetActive(value: false);
			}
		}
	}

	private void FatalDamage()
	{
		if ((bool)lastDamage.newType)
		{
			lastDamage.Callback(enemy);
			switch (lastDamage.newType.finisher)
			{
			case DamageType.Finisher.Impile:
				FixedPin(lastDamage.dir);
				StyleRanking.instance.RegStylePoint(Game.style.EnemyImpiled);
				break;
			case DamageType.Finisher.Mill:
				Mill();
				break;
			case DamageType.Finisher.Sword:
			case DamageType.Finisher.Arrow:
			case DamageType.Finisher.Spear:
				if (TryToNail())
				{
					StyleRanking.instance.RegStylePoint(Game.style.EnemyNailed);
				}
				break;
			case DamageType.Finisher.Rip:
				if (Rip(ripableJoint))
				{
					StyleRanking.instance.RegStylePoint(Game.style.EnemyHalved);
				}
				break;
			default:
				PushBody(lastDamage.dir);
				break;
			}
		}
		else
		{
			Debug.Log("NO DAMAGE TYPE 2");
		}
	}

	private void Mill()
	{
		QuickEffectsPool.Get("Damage", rb.position + lastDamage.dir * 2f, rb.rotation).Play();
		QuickEffectsPool.Get("Milled Body", rb.position + lastDamage.dir * 2f).Play();
		DeactivateBody();
		if (OnDie != null)
		{
			OnDie(base.gameObject);
		}
	}

	public virtual bool Rip(int i)
	{
		if (lifetime != 0f)
		{
			if (rippableJoints.Length != 0)
			{
				i = rippableJoints[UnityEngine.Random.Range(0, rippableJoints.Length)];
			}
			tempHealableJoint.index = i;
			tempHealableJoint.joint = rbs[i].GetComponent<Joint>();
			tempHealableJoint.localPos = tRbs[i].localPosition;
			tempHealableJoint.connectedBody = tempHealableJoint.joint.connectedBody;
			tempHealableJoint.freeBody = freeRigidbodies[0];
			rippedJoints.Add(tempHealableJoint);
			freeRigidbodies[0].gameObject.SetActive(value: true);
			freeRigidbodies[0].position = tRbs[i].position;
			rbs[i].GetComponent<Joint>().connectedBody = freeRigidbodies[0];
			bloodParticle.transform.SetParent(tRbs[i], worldPositionStays: false);
			bloodParticle.transform.localPosition = Vector3.zero;
			rbs[i].velocity = (lastDamage.dir + Vector3.up).normalized * 15f;
			rbs[0].velocity = (-lastDamage.dir.normalized + Vector3.up).normalized * 15f;
			return true;
		}
		PushBody(lastDamage.dir);
		return false;
	}

	public void PushBody(Vector3 dir, float force = 12f)
	{
		if (force == 0f)
		{
			force = 12f;
		}
		dir.Normalize();
		for (int i = 0; i < rbs.Length; i++)
		{
			rbs[i].velocity = dir * ((i == indexCoreBody) ? force : (force / 2f));
		}
	}

	public void RotateBody(float x, float y, float z)
	{
		deltaRotation = Quaternion.Euler(x, y, z);
		rb.MoveRotation(rb.rotation * deltaRotation);
	}

	private bool CorneredKick(Vector3 dir, float maxDist = 1.5f)
	{
		Physics.Raycast(rbs[0].position, dir, out hit, 12f, 1);
		if (hit.distance != 0f && hit.distance <= maxDist)
		{
			enemy.ChangeHealth(-30f);
			mat.Blink();
			mat.SetFloatByName("_Power", enemy.health / enemy.maxHealth);
			QuickEffectsPool.Get("Damage", rb.position, Quaternion.LookRotation(hit.normal)).Play();
			tempDir = Vector3.Reflect(tempDir, hit.normal);
			PushBody(tempDir, 16f);
			if (enemy.dead)
			{
				Die();
			}
			Debug.DrawRay(hit.point, -dir, Color.magenta, 2f);
			Debug.DrawRay(hit.point, tempDir, Color.yellow, 2f);
			return true;
		}
		return false;
	}

	public virtual void Kick(Vector3 dir)
	{
		StopStun();
		KickController.lastKicked = enemy;
		bool flag = false;
		if (lifetime == 0f)
		{
			tempDir = Vector3.ProjectOnPlane(dir, Game.player.grounder.gNormal);
		}
		else
		{
			tempDir = dir;
		}
		Physics.SphereCast(rbs[0].position, 0.75f, tempDir, out hit, 12f, 148480);
		if (hit.distance != 0f)
		{
			flag = true;
			tempDir = rbs[0].position.DirTo(hit.collider.bounds.center);
			tempDir = Quaternion.AngleAxis(-10f, Game.player.tHead.right) * tempDir;
			Debug.DrawLine(t.position, hit.point, Color.magenta, 2f);
			Debug.DrawRay(t.position, tempDir, Color.yellow, 2f);
		}
		if (!flag && CorneredKick(dir))
		{
			return;
		}
		if (lifetime > 0.25f && !pinJoint && pinState == 0 && !flag)
		{
			CheckSlam(dir);
		}
		if (slamming == null)
		{
			if (flag)
			{
				PushBody(tempDir, 18f);
			}
			else
			{
				PushBody((dir + Vector3.up).normalized);
			}
			if (lifetime != 0f)
			{
				RotateBody(Mathf.Sin(Time.timeSinceLevelLoad) * 60f, 0f, 0f);
				StyleRanking.instance.RegStylePoint(Game.player.weapons.kickBounce);
			}
		}
		Game.sounds.PlayClipAtPosition(sounds.kick.GetRandom(), 1f, rb.position);
		if (!enemy.dead)
		{
			PlaySound(enemy.sounds.Hurt);
		}
	}

	private void SlideKick(Vector3 dir)
	{
		KickController.lastKicked = enemy;
		if (!CheckSlam(dir, 1.5f))
		{
			if (hit.distance != 0f && hit.distance <= 1.5f)
			{
				enemy.ChangeHealth(-30f);
				mat.Blink();
				mat.SetFloatByName("_Power", enemy.health / enemy.maxHealth);
				QuickEffectsPool.Get("Damage", rb.position, Quaternion.LookRotation(hit.normal)).Play();
				PushBody(Vector3.up, 20f);
				if (enemy.dead)
				{
					Die();
				}
			}
			else
			{
				PushBody(dir + Vector3.up / 2f, 20f);
			}
		}
		Game.sounds.PlayClipAtPosition(sounds.kick.GetRandom(), 1f, rb.position);
	}

	private bool CheckSlam(Vector3 dir, float maxDist = 2f, float maxAngle = 25f)
	{
		if (slamming != null)
		{
			return false;
		}
		Physics.Raycast(rbs[0].position, dir, out hit, 8f, 147457);
		if (hit.distance >= maxDist && hit.collider.gameObject.layer == 0 && Vector3.Angle(-hit.normal, dir) <= maxAngle)
		{
			Debug.DrawLine(hit.point + hit.normal, rbs[0].position, Color.magenta, 4f);
			slamming = StartCoroutine(Slamming(hit.point + hit.normal));
			return true;
		}
		return false;
	}

	private void StopSlam()
	{
		if (slamming != null)
		{
			StopCoroutine(slamming);
			slamming = null;
		}
	}

	private IEnumerator Slamming(Vector3 pos)
	{
		yield return waitForPointOneSec;
		for (int i = 0; i < tRbs.Length; i++)
		{
			rbs[i].isKinematic = true;
		}
		source.loop = true;
		source.PlayClip(sounds.dash);
		while (Vector3.Distance(tRbs[0].position, pos) > 0.05f)
		{
			tRbs[0].position = Vector3.MoveTowards(tRbs[0].position, pos, Time.deltaTime * 20f);
			tRbs[0].Rotate(-360f * Time.deltaTime * 4f, 0f, 0f, Space.Self);
			yield return null;
		}
		source.Stop();
		source.loop = false;
		Game.time.SlowMotion(0.1f, 0.1f, 0.1f);
		CameraController.shake.Shake(1);
		QuickPool.instance.Get("Slam FX", hit.point + hit.normal, Quaternion.LookRotation(hit.normal)).GetComponent<TrailScript>().Play();
		yield return waitForFixedUpdate;
		yield return waitForFixedUpdate;
		yield return waitForFixedUpdate;
		for (int j = 0; j < tRbs.Length; j++)
		{
			rbs[j].isKinematic = false;
		}
		QuickEffectsPool.Get("Damage", rb.position, Quaternion.LookRotation(hit.normal)).Play();
		StyleRanking.instance.RegStylePoint((hit.normal.y.Abs() < 0.5f) ? Game.style.slamWall : ((hit.normal.y > 0f) ? Game.style.slamFloor : Game.style.slamCeiling));
		lastDamage = new DamageData();
		lastDamage.dir = hit.normal;
		lastDamage.newType = Game.style.basicBluntHit;
		lastDamage.knockdown = true;
		enemy.ChangeHealth(-70f);
		if (enemy.dead)
		{
			Die();
		}
		else
		{
			mat.Blink();
			mat.SetFloatByName("_Power", enemy.health / enemy.maxHealth);
			PushBody(hit.normal + Vector3.up);
		}
		slamming = null;
	}

	private void Stun()
	{
		if ((bool)enemy.weapon && enemy.GetHealthPercentage() < 0.5f && !WeaponsControl.allWeapons.Contains(tRbs[0]))
		{
			WeaponsControl.allWeapons.Add(tRbs[0]);
		}
		if (!stunned)
		{
			stunned = true;
			stunTimer = maxStunTime;
			mat.Stun();
			for (int i = 0; i < rbs.Length; i++)
			{
				rbs[i].useGravity = false;
				rbs[i].drag = 0.5f;
			}
		}
	}

	private void StopStun()
	{
		if (stunned)
		{
			stunned = false;
			stunTimer = 0f;
			mat.Unstun();
			for (int i = 0; i < rbs.Length; i++)
			{
				rbs[i].useGravity = true;
				rbs[i].drag = 0f;
			}
			if ((bool)enemy.weapon && WeaponsControl.allWeapons.Contains(tRbs[0]))
			{
				WeaponsControl.allWeapons.Remove(tRbs[0]);
			}
		}
	}

	public virtual void OnEnable()
	{
		if ((bool)tWeaponRoot && !tWeaponRoot.gameObject.activeInHierarchy)
		{
			tWeaponRoot.gameObject.SetActive(value: true);
		}
		knockoutPreventDelay = UnityEngine.Random.Range(0.5f, 2.5f);
		lastGroundCheckPos.x = (lastGroundCheckPos.y = (lastGroundCheckPos.z = 0f));
		base.enabled = true;
		for (int i = 0; i < rippedJoints.Count; i++)
		{
			tRbs[rippedJoints[i].index].localPosition = rippedJoints[i].localPos;
			rippedJoints[i].joint.connectedBody = rippedJoints[i].connectedBody;
			rippedJoints[i].freeBody.gameObject.SetActive(value: false);
		}
		rippedJoints.Clear();
		spinNormal.x = UnityEngine.Random.Range(-1f, 1f);
		spinNormal.y = UnityEngine.Random.Range(-1f, 1f);
		spinNormal.z = UnityEngine.Random.Range(-1f, 1f);
		spinNormal.Normalize();
		for (int j = 0; j < rbs.Length; j++)
		{
			rbs[j].isKinematic = false;
			rbs[j].freezeRotation = false;
		}
		if (!clldr.enabled)
		{
			clldr.enabled = true;
		}
		highestPoint = rb.position.y;
		int num2 = (fallDistStep = 0);
		float num4 = (fallDist = num2);
		float num6 = (lifetime = num4);
		float num8 = (damageTimer = num6);
		timer = num8;
		if ((bool)mat && (bool)enemy)
		{
			mat.SetFloatByName("_Power", enemy.health / enemy.maxHealth);
		}
		shadowProjector.orthographicSize = shadowSize;
		particle.Play();
	}

	public void Reset()
	{
		StopStun();
		StopSlam();
		freefallPreventTimer = 0f;
		if ((bool)flammable)
		{
			flammable.SetOnFire(value: false);
		}
		for (int i = 0; i < rends.Length; i++)
		{
			rends[i].sharedMaterial = matDefault;
		}
	}

	public virtual void Awake()
	{
		rbs = GetComponentsInChildren<Rigidbody>();
		tRbs = new Transform[rbs.Length];
		for (int i = 0; i < tRbs.Length; i++)
		{
			tRbs[i] = rbs[i].transform;
		}
		rb = rbs[indexCoreBody];
		tRoot = base.transform;
		t = rb.transform;
		for (int j = 0; j < freeRigidbodies.Length; j++)
		{
			freeRigidbodies[j].gameObject.SetActive(value: false);
		}
		rends = GetComponentsInChildren<MeshRenderer>();
		bodyCollider = GetComponentInChildren<BodyCollider>();
		source = GetComponentInChildren<AudioSource>();
		flammable = GetComponentInChildren<Flammable>();
		canBeDrowned = GetComponentInChildren<CanBeDrowned>();
		CanBeDrowned obj = canBeDrowned;
		obj.OnDrowned = (Action)Delegate.Combine(obj.OnDrowned, new Action(OnDrowned));
		dmgInfo.amount = 1000f;
		dmgInfo.newType = Game.style.basicBluntHit;
		mat = GetComponentInChildren<EnemyMaterial>();
		mat.Setup();
		shadowSize = shadowProjector.orthographicSize;
	}

	public virtual void OnDrowned()
	{
		if (!enemy.dead)
		{
			enemy.InvokeDeathAction(0f);
		}
	}

	public virtual void Setup(BaseEnemy newEnemy)
	{
		if (enemy != newEnemy)
		{
			enemy = newEnemy;
		}
		flammable.enemy = enemy;
		DeactivateBody();
	}

	public virtual void ExtraUpdate()
	{
	}

	public void Update()
	{
		lifetime += Time.deltaTime;
		damageTimer = Mathf.MoveTowards(damageTimer, 0f, Time.deltaTime);
		if (stunned && !stunTimer.MoveTowards(0f, (rb.velocity.sqrMagnitude > 4f) ? 1 : 2))
		{
			StopStun();
		}
		if (t.position.y > highestPoint)
		{
			highestPoint = t.position.y;
		}
		fallDist = highestPoint - t.position.y;
		Debug.DrawLine(t.position.With(null, highestPoint), t.position, Color.yellow);
		if (enemy.dead)
		{
			deadTimer = Mathf.MoveTowards(deadTimer, 5f, Time.deltaTime);
			if (deadTimer == 5f)
			{
				DeactivateBody();
			}
			else
			{
				mat.Dissolve(deadTimer / 5f);
				shadowProjector.orthographicSize = shadowSize - deadTimer / 5f * (shadowSize - 0.1f);
			}
		}
		if (enemy.dead || rbs[0].isKinematic || stunTimer > 0f || slamming != null)
		{
			return;
		}
		ExtraUpdate();
		if (knockoutPreventDelay != 0f)
		{
			knockoutPreventDelay = Mathf.MoveTowards(knockoutPreventDelay, 0f, Time.deltaTime);
		}
		if ((preventKnocking || enemy.buffed) && !enemy.flammable.onFire && knockoutPreventDelay == 0f && lifetime > 0.5f && damageTimer == 0f)
		{
			Vector3 normalized = rb.velocity.With(null, 0f).normalized;
			knockoutPreventDelay = 0.2f;
			Physics.Raycast(rb.position, (Vector3.down + normalized).normalized, out hit, 12f, 1);
			if (hit.distance > 2f && hit.normal.y.Abs() > 0.5f && NavMesh.SamplePosition(hit.point, out var navMeshHit, 1f, -1))
			{
				ActivateEnemy(rb.position);
				DeactivateBody();
				enemy.targetPosition = navMeshHit.position;
				enemy.stateMachine.SwitchState(typeof(EnemyDashState));
			}
		}
		if (fallDistStep != Mathf.FloorToInt(fallDist / 6f))
		{
			fallDistStep = Mathf.FloorToInt(fallDist / 6f);
			if (fallDistStep == 1 && (bool)enemy.sounds.Fall && !preventFreeFall && !Physics.Raycast(rb.position, Vector3.down, 12f, 1))
			{
				PlaySound(enemy.sounds.Fall);
			}
			if (fallDistStep > 2 && !Physics.Raycast(rb.position, Vector3.down, 12f, 1))
			{
				dmgInfo.dir = rb.velocity.normalized;
				dmgInfo.newType = Game.style.basicBluntHit;
				dmgInfo.stylePoint = Game.style.bodyFreeFall;
				Damage(dmgInfo);
			}
		}
		else if (preventFreeFall && !freefallPreventTimer.MoveTowards(0f) && fallDist > 6f)
		{
			if (enemy.GetHealthPercentage() > 0.25f && recoverPos.sqrMagnitude > 0f && !Physics.Raycast(rb.position, Vector3.down, 18f, 1))
			{
				(QuickPool.instance.Get("EnemyTeleport", t.position) as EnemyTeleport).Setup(enemy, t.position.With(null, highestPoint));
				DeactivateBody();
			}
			freefallPreventTimer = 1f;
		}
		if (!(rb.velocity.sqrMagnitude < 4f))
		{
			return;
		}
		if (timer < recoverTime)
		{
			timer += Time.deltaTime;
		}
		else
		{
			if ((bool)pinJoint)
			{
				return;
			}
			timer = Mathf.Clamp(timer - 0.5f, 0.1f, float.PositiveInfinity);
			Physics.Raycast(rbs[0].position, Vector3.down, out hit, 1f, 1);
			if (hit.distance != 0f)
			{
				Vector3 target = default(Vector3);
				if (enemy.CheckNavMeshPos(ref target, hit.point, 1f))
				{
					ActivateEnemy(target);
				}
				else
				{
					timer = Mathf.Clamp(timer - 0.5f, 0.1f, float.PositiveInfinity);
				}
			}
			else
			{
				rb.AddForce(t.forward * 5f, ForceMode.Impulse);
				timer = Mathf.Clamp(timer - 0.5f, 0.1f, float.PositiveInfinity);
			}
		}
	}

	public void ActivateEnemy(Vector3 pos, bool wall = false)
	{
		if (!wall)
		{
			enemy.agent.Warp(pos);
			enemy.t.rotation = Quaternion.LookRotation(t.forward.With(null, 0f));
		}
		else
		{
			enemy.agent.enabled = false;
		}
		enemy.gameObject.SetActive(value: true);
		enemy.PlaySound(enemy.sounds.Spawn);
		enemy.flammable.SetOnFire(flammable.onFire);
		flammable.SetOnFire(value: false);
		DeactivateBody();
	}

	private void FixedUpdate()
	{
		if (pinState != 0)
		{
			PinUpdate();
		}
		if (stunned)
		{
			rb.AddForce(Vector3.down * 10f);
			rb.AddTorque(Vector3.one * (stunTimer * 10f), ForceMode.Force);
		}
		else
		{
			if (Vector3.Distance(t.position, lastGroundCheckPos) > 1.5f)
			{
				if (Physics.Raycast(t.position, Vector3.down, out var hitInfo, 6f, 1) && enemy.LastGroundedBodyPosition(hitInfo.point, ref recoverPos))
				{
					Debug.DrawRay(recoverPos, Vector3.up, Color.magenta, 2f);
				}
				lastGroundCheckPos = t.position;
			}
			if (lifetime < 1f)
			{
				rb.AddTorque(spinNormal * (45f * (1f - lifetime)), ForceMode.Force);
			}
		}
		oldVel = rb.velocity;
	}

	public bool TryToNail()
	{
		PushBody(lastDamage.dir);
		Physics.Raycast(rb.position, lastDamage.dir, out hit, 12f, 16385);
		if (hit.distance == 0f || (bool)pinJoint || hit.collider.gameObject.layer != 0 || Vector3.Angle(lastDamage.dir, -hit.normal) > 60f)
		{
			if (lastDamage.newType.finisher == DamageType.Finisher.Sword)
			{
				((PooledWeapon)QuickPool.instance.Get("Sword", rb.position, rb.rotation)).PinTheBody(this);
			}
			if (lastDamage.newType.finisher == DamageType.Finisher.Spear)
			{
				((PooledWeapon)QuickPool.instance.Get("Spear", rb.position, rb.rotation)).PinTheBody(this);
			}
			PushBody(lastDamage.dir, 24f);
			return false;
		}
		pinPoint = hit.point;
		pinNormal = hit.normal;
		pinTimer = 0f;
		pinState = 3;
		return true;
	}

	public void PinUpdate()
	{
		switch (pinState)
		{
		case 3:
			if (pinTimer < 0.04f)
			{
				pinTimer += Time.fixedDeltaTime;
			}
			else
			{
				pinState--;
			}
			break;
		case 2:
		{
			int num = 0;
			pinJoint = rbs[num].gameObject.AddComponent<SpringJoint>();
			rbs[num].freezeRotation = true;
			((SpringJoint)pinJoint).damper = 1f;
			((SpringJoint)pinJoint).spring = 100f;
			pinJoint.massScale = 4f;
			pinJoint.autoConfigureConnectedAnchor = false;
			pinJoint.anchor = zero;
			switch (lastDamage.newType.finisher)
			{
			case DamageType.Finisher.Sword:
			{
				PooledWeapon pooledWeapon = (PooledWeapon)QuickPool.instance.Get("Sword", pinPoint + pinNormal * 0.5f, Quaternion.FromToRotation(Vector3.up, -pinNormal));
				pooledWeapon.PinTheBodyToTheWall(this);
				pinJoint.connectedAnchor = zero;
				pinJoint.connectedBody = pooledWeapon.rb;
				break;
			}
			case DamageType.Finisher.Spear:
			{
				PooledWeapon pooledWeapon = (PooledWeapon)QuickPool.instance.Get("Spear", pinPoint + pinNormal * 0.5f, Quaternion.FromToRotation(Vector3.up, -pinNormal));
				pooledWeapon.PinTheBodyToTheWall(this);
				pinJoint.connectedAnchor = zero;
				pinJoint.connectedBody = pooledWeapon.rb;
				break;
			}
			case DamageType.Finisher.Arrow:
				QuickPool.instance.Get("Arrow", pinPoint, Quaternion.LookRotation(-pinNormal));
				pinJoint.connectedAnchor = pinPoint;
				break;
			}
			pinTimer = 0f;
			pinState = 0;
			CameraController.shake.Shake(1);
			QuickEffectsPool.Get("Arrow Hit", pinPoint, Quaternion.LookRotation(pinNormal)).Play();
			break;
		}
		case 1:
			if (pinTimer != 1f)
			{
				pinTimer = Mathf.MoveTowards(pinTimer, 1f, Time.fixedDeltaTime);
				((SpringJoint)pinJoint).spring = Mathf.Lerp(((SpringJoint)pinJoint).spring, 10f, pinTimer);
				pinJoint.massScale = Mathf.Lerp(pinJoint.massScale, 10f, pinTimer);
			}
			else
			{
				pinState--;
			}
			break;
		}
	}

	public virtual void FixedPin(Vector3 pos)
	{
		if (!pinJoint)
		{
			rb.position = pos;
			pinJoint = rb.gameObject.AddComponent<FixedJoint>();
			timer = 0f;
			Game.sounds.PlayClipAtPosition(sounds.implied, 0.75f, rb.position);
			CameraController.shake.Shake(2);
		}
	}

	public void DestroyPinJoint()
	{
		pinState = 0;
		if ((bool)pinJoint)
		{
			UnityEngine.Object.Destroy(pinJoint);
		}
	}

	public void DeactivateBody()
	{
		base.enabled = false;
		if (OnDeactivate != null)
		{
			OnDeactivate(this);
		}
		DestroyPinJoint();
		temp.x = (temp.z = 0f);
		temp.y = 1000f;
		tRoot.position = temp;
		StopSlam();
		for (int i = 0; i < rbs.Length; i++)
		{
			rbs[i].isKinematic = true;
			rbs[i].angularVelocity = Vector3.zero;
			rbs[i].velocity = Vector3.zero;
			if (i == 0)
			{
				temp.y = 1.2f;
				tRbs[i].localPosition = temp;
				tRbs[i].localRotation = Quaternion.Euler(-90f, 0f, 0f);
				tRbs[i].Rotate(Vector3.up * UnityEngine.Random.Range(-45, 45));
			}
			else
			{
				tRbs[i].localRotation = Quaternion.identity;
			}
		}
		particle.Stop();
		bloodParticle.Stop();
	}

	public void PlaySound(AudioClip clip, float volume = 1f)
	{
		if ((bool)source)
		{
			if (source.volume != volume)
			{
				source.volume = volume;
			}
			if (source.isPlaying)
			{
				source.Stop();
			}
			if (source.clip != clip)
			{
				source.clip = clip;
			}
			source.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
			source.Play();
		}
	}
}
