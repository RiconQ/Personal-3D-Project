using System;
using UnityEngine;

public class ThrowedDagger : MonoBehaviour
{
	public static Action<BaseEnemy> OnTargetDetected = delegate
	{
	};

	public static Action<BaseEnemy> OnEnemyHitted = delegate
	{
	};

	public static Action<Rigidbody> OnBreakableHitted = delegate
	{
	};

	public static Action<GameObject> OnHit = delegate
	{
	};

	public Transform tChainPivot;

	public Transform tChainRoot;

	public Transform[] segments;

	public GameObject prefab;

	public float speed = 25f;

	private bool weaponHit;

	private string hookOnEffect = "Hoocked";

	private string hitEffect = "Arrow Hit";

	public DamageData damage = new DamageData();

	[Header("Audio")]
	public AudioClip pinSound;

	[HideInInspector]
	public StylePointTypes stylePoint;

	private RaycastHit hit;

	private ParticleSystem particle;

	private AudioSource source;

	public AudioSource clickingSource;

	private Collider clldr;

	private Collider[] clldrs = new Collider[1];

	private Animation anim;

	public Transform target;

	public BaseEnemy targetEnemy;

	public BaseBreakable targetBreakable;

	private float dist;

	private int intDist;

	private int chainDist;

	private int chainClickDist;

	private int chainLastClickDist;

	private int targetType;

	private Vector3 lastPos;

	private SpringJoint joint;

	public Collider hoockedClldr { get; private set; }

	public Vector3 targetPos { get; private set; }

	public Transform t { get; private set; }

	public Transform tMesh { get; private set; }

	private void Awake()
	{
		t = base.transform;
		tMesh = t.Find("Mesh").transform;
		particle = GetComponentInChildren<ParticleSystem>();
		source = GetComponent<AudioSource>();
		clldr = GetComponentInChildren<Collider>();
		anim = GetComponentInChildren<Animation>();
		CreateChainMesh();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnEnable()
	{
		weaponHit = false;
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void CreateChainMesh()
	{
		segments = new Transform[100];
		for (int i = 0; i < 100; i++)
		{
			segments[i] = UnityEngine.Object.Instantiate(prefab).transform;
			segments[i].SetParent(tChainRoot);
			segments[i].localPosition = new Vector3(0f, 0f, (float)i * 0.3f);
			segments[i].localEulerAngles = new Vector3(0f, 0f, (i % 2 != 1) ? 90 : 0);
		}
		tChainRoot.gameObject.SetActive(value: false);
		tChainRoot.SetParent(null);
	}

	public void AlignChainMesh(Transform tPlayerPivot)
	{
		float num = Vector3.Distance(tPlayerPivot.position, tChainPivot.position);
		chainDist = Mathf.RoundToInt(num / 0.3f);
		chainClickDist = Mathf.RoundToInt(num / 2f);
		for (int i = 0; i < 100; i++)
		{
			if (segments[i].gameObject.activeInHierarchy != i < chainDist)
			{
				segments[i].gameObject.SetActive(i < chainDist);
			}
		}
		if (!tChainRoot.gameObject.activeInHierarchy)
		{
			tChainRoot.gameObject.SetActive(value: true);
		}
		if (chainClickDist != chainLastClickDist)
		{
			if (clickingSource.isPlaying)
			{
				clickingSource.Stop();
			}
			clickingSource.pitch = UnityEngine.Random.Range(0.8f, 1.1f);
			clickingSource.Play();
			chainLastClickDist = chainClickDist;
		}
	}

	private void Update()
	{
		if (!CheckState())
		{
			Reset();
		}
		else if (!hoockedClldr)
		{
			tMesh.Rotate(0f, -1440f * Time.deltaTime, 0f, Space.Self);
			t.Translate(Vector3.forward * (Time.deltaTime * speed));
			if (targetType > 0)
			{
				UpdateTargetPos();
				t.rotation = Quaternion.RotateTowards(t.rotation, Quaternion.LookRotation(t.position.DirTo(targetPos)), Time.deltaTime * 90f);
			}
			dist += Time.deltaTime * speed;
		}
		else
		{
			UpdateTargetPos();
		}
	}

	public void Activate(Transform pivot)
	{
		target = null;
		targetType = 0;
		t.SetPositionAndRotation(pivot.position, pivot.rotation);
		lastPos = t.position;
		base.gameObject.SetActive(value: true);
		hoockedClldr = null;
		targetBreakable = null;
		targetEnemy = null;
		clldr.enabled = true;
		dist = (intDist = -1);
		speed = 45f;
		anim.Play(PlayMode.StopAll);
		PullableControl.GetCurrent(out target);
		if ((bool)target)
		{
			targetType = 3;
		}
		else
		{
			CrowdControl.instance.GetClosestEnemyToNormal(Game.player.tHead.position, Game.player.tHead.forward, 20f, 18f, out targetEnemy);
			BreakablesControl.instance.GetClosest(18f, 20f, out targetBreakable);
			if ((bool)targetEnemy && !targetBreakable)
			{
				targetType = 1;
			}
			else if ((bool)targetBreakable && !targetEnemy)
			{
				targetType = 2;
			}
			else if ((bool)targetEnemy && (bool)targetBreakable)
			{
				float num = Vector3.Angle(Game.player.tHead.forward, Game.player.tHead.position.DirTo(targetEnemy.GetActualPosition()));
				float num2 = Vector3.Angle(Game.player.tHead.forward, Game.player.tHead.position.DirTo(targetBreakable.clldr.bounds.center));
				if (num < num2)
				{
					targetType = 1;
				}
				else
				{
					targetType = 2;
				}
				Debug.Log(targetType);
			}
		}
		if (targetType == 1)
		{
			if (OnTargetDetected != null)
			{
				OnTargetDetected(targetEnemy);
			}
			if (!targetEnemy)
			{
				targetType = 0;
			}
		}
		if ((bool)targetBreakable || (bool)targetEnemy || (bool)target)
		{
			UpdateTargetPos();
			t.rotation = Quaternion.LookRotation(t.position.DirTo(targetPos));
			Debug.DrawLine(t.position, targetPos, Color.green, 4f);
		}
	}

	private void HitStop(Vector3 pos, Vector3 normal)
	{
		Game.sounds.PlayClipAtPosition(pinSound, 1f, t.position);
		QuickEffectsPool.Get(hitEffect, pos, Quaternion.LookRotation(normal)).Play();
		Reset();
	}

	private void Stop()
	{
		QuickEffectsPool.Get(hitEffect, t.position, Quaternion.LookRotation(t.forward)).Play();
		Reset();
	}

	public void Reset()
	{
		target = null;
		targetBreakable = null;
		targetEnemy = null;
		hoockedClldr = null;
		tChainRoot.gameObject.SetActive(value: false);
		if ((bool)joint)
		{
			UnityEngine.Object.Destroy(joint);
		}
		base.gameObject.SetActive(value: false);
		Game.player.weapons.SwitchFromDagger();
	}

	public void Pull()
	{
		tChainRoot.gameObject.SetActive(value: false);
		if ((bool)joint)
		{
			UnityEngine.Object.Destroy(joint);
		}
		switch (hoockedClldr.gameObject.layer)
		{
		case 10:
		{
			if (!Game.player.grounder.grounded)
			{
				damage.amount = 10f;
				damage.knockdown = false;
				damage.newType = Game.player.weapons.daggerController.dmg_Pull;
				damage.dir = Vector3.up;
				Game.player.PullTo(targetEnemy.GetActualPosition(), withPound: true);
				StyleRanking.instance.RegStylePoint(Game.style.DaggerBoost);
				break;
			}
			Vector2 vector = Game.player.camController.worldCam.WorldToScreenPoint(targetEnemy.GetActualPosition());
			vector.x /= Screen.width;
			vector.y /= Screen.height;
			vector -= new Vector2(0.5f, 0.5f);
			Vector3 vector2 = t.position.DirTo(Game.player.t.position);
			damage.amount = 5f;
			damage.knockdown = true;
			if (targetEnemy.isActiveAndEnabled || !Physics.Raycast(targetEnemy.GetActualPosition(), Vector3.down, 8f, 1))
			{
				damage.newType = Game.player.weapons.daggerController.dmg_Pull;
				damage.dir = (vector2 + Vector3.up + Vector3.Cross(vector2, Vector3.up).normalized * (0f - Mathf.Clamp(vector.x, -0.6f, 0.6f))).normalized;
			}
			else
			{
				Game.player.sway.Sway(7.5f, 0f, 2.5f, 3f);
				damage.newType = Game.player.weapons.daggerController.dmg_Slam;
				if (vector.y.Abs() > vector.x.Abs() || vector.x.Abs() < 0.2f)
				{
					if (Game.player.weapons.daggerController.holding > 0.1f)
					{
						Game.time.SlowMotion();
						damage.dir = Vector3.down;
					}
					else
					{
						damage.dir = vector2.normalized;
					}
				}
				else
				{
					damage.dir = (vector2 + Vector3.Cross(vector2, Vector3.up).normalized * (0f - Mathf.Clamp(vector.x, -0.6f, 0.6f))).normalized;
				}
			}
			targetEnemy.Damage(damage);
			break;
		}
		case 14:
		case 19:
			if (hoockedClldr.attachedRigidbody.isKinematic || hoockedClldr.CompareTag("Target"))
			{
				if (hoockedClldr.CompareTag("Pullable") && OnBreakableHitted != null)
				{
					OnBreakableHitted(hoockedClldr.attachedRigidbody);
				}
				damage.newType = Game.player.weapons.daggerController.dmg_Pull;
				damage.dir = -t.forward.Snap();
				hoockedClldr.GetComponent<IDamageable<DamageData>>().Damage(damage);
				break;
			}
			t.position.DirTo(Game.player.tHead.position);
			Vector3.Distance(Game.player.tHead.position, hoockedClldr.bounds.center);
			if (hoockedClldr.attachedRigidbody.drag != 0f && !Game.player.grounder.grounded)
			{
				Game.player.PullTo(hoockedClldr.attachedRigidbody.position);
			}
			hoockedClldr.attachedRigidbody.AddBallisticForce(Game.player.tHead.position, 1f, Physics.gravity.y, resetVelocity: true);
			hoockedClldr.attachedRigidbody.AddTorque(Vector3.one * 180f, ForceMode.Impulse);
			if (hoockedClldr.gameObject.activeInHierarchy)
			{
				hoockedClldr.GetComponent<BreakableB>().damage.stylePoint = Game.style.ObjectPulled;
				hoockedClldr.GetComponent<BreakableB>().Unstun();
			}
			if (OnBreakableHitted != null)
			{
				OnBreakableHitted(hoockedClldr.attachedRigidbody);
			}
			break;
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.layer == 9)
		{
			return;
		}
		if (c.gameObject.layer == 14 && c.attachedRigidbody.isKinematic && !c.CompareTag("Pullable"))
		{
			Stop();
			return;
		}
		clldr.enabled = false;
		hoockedClldr = c;
		Game.player.sway.Sway(-2f, 0f, 4f, 3f);
		switch (c.gameObject.layer)
		{
		case 10:
			targetBreakable = null;
			if (c.attachedRigidbody.isKinematic)
			{
				targetEnemy = c.GetComponent<BaseEnemy>();
				if (!targetEnemy.linkedSouls || !targetEnemy.linkedSouls.isActiveAndEnabled)
				{
					targetEnemy.Stagger(t.forward.With(null, 0f).normalized);
					QuickEffectsPool.Get(hookOnEffect, targetEnemy.GetActualPosition(), Quaternion.LookRotation(-t.forward)).Play();
				}
				else
				{
					Stop();
					CameraController.shake.Shake(2);
				}
			}
			else
			{
				targetEnemy = c.GetComponent<BodyCollider>().body.enemy;
				QuickEffectsPool.Get(hookOnEffect, targetEnemy.GetActualPosition(), Quaternion.LookRotation(-t.forward)).Play();
			}
			break;
		case 14:
			targetEnemy = null;
			targetBreakable = c.GetComponent<BaseBreakable>();
			if (!targetBreakable.rb.isKinematic)
			{
				targetBreakable.rb.velocity = Vector3.up * 5f;
				targetBreakable.rb.AddTorque(Vector3.one * 45f);
				QuickEffectsPool.Get(hookOnEffect, targetBreakable.rb.worldCenterOfMass, Quaternion.LookRotation(-t.forward)).Play();
			}
			else
			{
				Debug.Log("DJ");
				QuickEffectsPool.Get(hookOnEffect, hoockedClldr.bounds.center, Quaternion.LookRotation(-t.forward)).Play();
			}
			break;
		case 19:
			if (OnHit != null)
			{
				OnHit(hoockedClldr.gameObject);
			}
			QuickEffectsPool.Get(hookOnEffect, hoockedClldr.bounds.center, Quaternion.LookRotation(-t.forward)).Play();
			break;
		}
	}

	private void LateUpdate()
	{
		if ((bool)hoockedClldr)
		{
			t.position = targetPos;
			t.rotation = Quaternion.LookRotation(-hoockedClldr.transform.forward);
			tChainRoot.rotation = Quaternion.LookRotation(tChainPivot.position.DirTo(Game.player.weapons.daggerController.tPivot.position));
			tChainRoot.position = tChainPivot.position;
		}
	}

	public bool CheckState()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		if ((bool)hoockedClldr)
		{
			if ((bool)targetEnemy)
			{
				return !targetEnemy.dead;
			}
			if ((bool)targetBreakable)
			{
				return targetBreakable.gameObject.activeInHierarchy;
			}
			return hoockedClldr.gameObject.activeInHierarchy;
		}
		return true;
	}

	public void UpdateTargetPos()
	{
		if ((bool)hoockedClldr)
		{
			switch (hoockedClldr.gameObject.layer)
			{
			case 14:
			case 19:
				targetPos = (hoockedClldr.attachedRigidbody.isKinematic ? hoockedClldr.bounds.center : hoockedClldr.attachedRigidbody.worldCenterOfMass);
				break;
			case 10:
				targetPos = targetEnemy.GetActualPosition();
				break;
			}
			return;
		}
		switch (targetType)
		{
		case 0:
			targetPos = hoockedClldr.bounds.center;
			break;
		case 1:
			targetPos = targetEnemy.GetActualPosition();
			break;
		case 2:
			targetPos = (targetBreakable.rb.isKinematic ? targetBreakable.clldr.bounds.center : targetBreakable.rb.worldCenterOfMass);
			break;
		case 3:
			targetPos = target.position;
			break;
		}
	}

	private void FixedUpdate()
	{
		if ((bool)hoockedClldr)
		{
			return;
		}
		if ((int)dist * 2 != intDist)
		{
			intDist = (int)dist * 2;
			if (!weaponHit)
			{
				Physics.OverlapCapsuleNonAlloc(lastPos, t.position, 0.5f, clldrs, 8192);
				if (clldrs[0] != null)
				{
					if (clldrs[0].TryGetComponent<PooledWeapon>(out var component))
					{
						component.rb.velocity = (-t.forward + Vector3.up * 2f).normalized * 10f;
						component.rb.AddTorque(90f, -45f, 60f, ForceMode.Impulse);
						weaponHit = true;
						QuickEffectsPool.Get(hitEffect, t.position, Quaternion.LookRotation(t.position.DirTo(clldrs[0].bounds.center))).Play();
					}
					clldrs[0] = null;
					return;
				}
			}
			Physics.Linecast(lastPos, t.position, out hit, 1);
			Debug.DrawLine(lastPos, t.position, (hit.distance != 0f) ? Color.red : Color.cyan, 0.5f);
			if (hit.distance != 0f)
			{
				HitStop(hit.point, hit.normal);
				return;
			}
			lastPos = t.position;
		}
		if (dist >= (float)((targetType == 0) ? 12 : 18))
		{
			Stop();
		}
	}
}
