using System;
using UnityEngine;

[SelectionBase]
public class PooledWeapon : DashableObject, IInteractable, IDamageable<DamageData>
{
	public static Action<PooledWeapon> OnActivate = delegate
	{
	};

	public static Action<PooledWeapon> OnDeactivate = delegate
	{
	};

	public Weapon associatedScriptableObject;

	public bool keepActive;

	public DamageData damage = new DamageData();

	public Collider[] dynamicClldrs;

	public Collider kinematicClldr;

	public GameObject objOutline;

	[Header("Audio")]
	public AudioClip sfxGroundedKick;

	public AudioClip sfxFailedPick;

	private float gravity;

	private float timer;

	private AudioSource source;

	private Body connectedBody;

	private FixedJoint joint;

	private Rigidbody jointedRigidBody;

	private AnimationCurve gravityCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 2f, 0f));

	private Vector3 startPos;

	private Quaternion startRot;

	public float hotPickTimer { get; private set; }

	public float dissolveTimer { get; private set; }

	public virtual void Damage(DamageData damage)
	{
		if (!damage.newType.kick)
		{
			return;
		}
		gravity = 0f;
		timer = 0f;
		if (base.rb.isKinematic)
		{
			QuickEffectsPool.Get("Poof", base.t.position, base.t.rotation).Play();
			if ((bool)source)
			{
				source.PlayClip(sfxGroundedKick);
			}
		}
		else if (!damage.newType.slideKick && Physics.Raycast(base.rb.worldCenterOfMass, Vector3.down, 0.5f, 1))
		{
			base.rb.AddForceAndTorque(Vector3.up * 4f, base.t.forward * 10f);
			source.PlayClip(sfxGroundedKick);
			QuickEffectsPool.Get("Poof", base.t.position, base.t.rotation).Play();
		}
		else
		{
			Vector3 closestDirectionToNormal = CrowdControl.instance.GetClosestDirectionToNormal(base.t.position, damage.dir, 20f);
			Vector3 position = base.t.position;
			position.y += (damage.newType.slideKick ? 0.5f : 0f);
			(QuickPool.instance.Get(associatedScriptableObject.prefabThrowed, position, Quaternion.LookRotation(closestDirectionToNormal)) as ThrowedWeapon).dmg.amount = 200f;
			base.gameObject.SetActive(value: false);
		}
	}

	public void SetHotTimer(float value)
	{
		hotPickTimer = value;
	}

	protected virtual void OnEnable()
	{
		dissolveTimer = 0f;
		gravity = 0f;
		timer = 0f;
		if (!keepActive)
		{
			BreakConnections();
		}
		if (OnActivate != null)
		{
			OnActivate(this);
		}
		SetKinematic(keepActive);
	}

	public void BreakConnections()
	{
		if ((bool)joint)
		{
			UnityEngine.Object.Destroy(GetComponent<FixedJoint>());
		}
		if ((bool)connectedBody)
		{
			connectedBody.DestroyPinJoint();
			connectedBody = null;
		}
		if ((bool)jointedRigidBody)
		{
			jointedRigidBody = null;
		}
		base.rb.mass = 0.8f;
	}

	public void TossTowardsPlayer()
	{
		BreakConnections();
		if (base.rb.isKinematic)
		{
			SetKinematic(value: false);
		}
		base.rb.AddBallisticForce(Game.player.tHead.position + Game.player.tHead.forward.With(null, 0f).normalized, 1.2f, Physics.gravity.y, resetVelocity: true);
		base.rb.AddTorque(Vector3.up * 90f);
	}

	public void SetKinematic(bool value)
	{
		if (value)
		{
			base.rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
		base.rb.isKinematic = value;
		if (!value)
		{
			base.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		}
		for (int i = 0; i < dynamicClldrs.Length; i++)
		{
			dynamicClldrs[i].enabled = !value;
		}
		kinematicClldr.enabled = value;
	}

	protected override void Awake()
	{
		base.Awake();
		if (!WeaponsControl.allWeapons.Contains(base.t))
		{
			WeaponsControl.allWeapons.Add(base.t);
		}
		startPos = base.t.position;
		startRot = base.t.rotation;
		SetKinematic(keepActive);
		source = GetComponent<AudioSource>();
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Combine(BreakableB.OnBreak, new Action<GameObject>(CheckJoint));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
		Body.OnDeactivate = (Action<Body>)Delegate.Combine(Body.OnDeactivate, new Action<Body>(CheckBody));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (OnDeactivate != null)
		{
			OnDeactivate(this);
		}
	}

	protected virtual void OnDestroy()
	{
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Remove(BreakableB.OnBreak, new Action<GameObject>(CheckJoint));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
		Body.OnDeactivate = (Action<Body>)Delegate.Remove(Body.OnDeactivate, new Action<Body>(CheckBody));
	}

	private void FixedUpdate()
	{
		base.rb.drag = 1.5f - timer * 1.5f;
		base.rb.AddForce(Vector3.up * gravity);
	}

	private void LateUpdate()
	{
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
		}
		if (hotPickTimer != 0f)
		{
			hotPickTimer = Mathf.MoveTowards(hotPickTimer, 0f, Time.deltaTime);
		}
		gravity = Mathf.LerpUnclamped(0f, Physics.gravity.y, gravityCurve.Evaluate(timer));
	}

	private void Reset()
	{
		if ((bool)joint)
		{
			UnityEngine.Object.Destroy(GetComponent<FixedJoint>());
			connectedBody = null;
			jointedRigidBody = null;
			base.rb.mass = 0.8f;
		}
		if (keepActive)
		{
			SetKinematic(value: true);
			if (!base.gameObject.activeInHierarchy)
			{
				base.gameObject.SetActive(value: true);
			}
			base.t.SetPositionAndRotation(startPos, startRot);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void CheckJoint(GameObject obj)
	{
		if ((bool)jointedRigidBody && jointedRigidBody.gameObject == obj)
		{
			UnityEngine.Object.Destroy(GetComponent<FixedJoint>());
			jointedRigidBody = null;
			base.rb.AddForceAndTorque(Vector3.up * 10f, Vector3.one * 10f);
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		switch (c.gameObject.layer)
		{
		default:
			_ = 19;
			break;
		case 9:
			if (Game.player.slide.isSliding && Game.player.SlideHolded())
			{
				if (Game.player.weapons.currentWeapon == -1)
				{
					Game.player.sway.Sway(-5f, 0f, 0f, 4f);
					Interact();
					StyleRanking.instance.RegStylePoint(Game.style.WeaponsSlidePick);
				}
				else
				{
					base.rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
					base.rb.AddTorque(Vector3.one * 40f, ForceMode.Impulse);
				}
			}
			break;
		case 10:
		{
			if (c.attachedRigidbody.isKinematic && base.rb.velocity.sqrMagnitude < 0.25f && c.TryGetComponent<BaseEnemy>(out var component) && component.GetHealthPercentage() < 0.5f)
			{
				DamageData damageData = new DamageData();
				damageData.knockdown = true;
				damageData.amount = 0f;
				damageData.dir = c.transform.forward;
				damageData.newType = Game.player.weapons.basicBluntHit;
				damageData.stylePoint = Game.style.EnemyTripped;
				component.Damage(damageData);
				base.rb.AddForceAndTorque((Vector3.up - c.transform.forward).normalized * 5f, new Vector3(45f, 90f, 0f));
				if ((bool)source)
				{
					source.PlayClip(sfxGroundedKick);
				}
				QuickEffectsPool.Get("Poof", base.t.position, base.t.rotation).Play();
			}
			break;
		}
		}
	}

	private void CheckBody(Body body)
	{
		if ((bool)connectedBody && !(connectedBody != body))
		{
			BreakConnections();
		}
	}

	public virtual void PinTheBody(Body body)
	{
		connectedBody = body;
		joint = base.gameObject.AddComponent<FixedJoint>();
		joint.connectedBody = body.rb;
		joint.massScale = 0.05f;
		base.rb.mass = 0.1f;
	}

	public virtual void PinTheBodyToTheWall(Body body)
	{
		connectedBody = body;
		SetKinematic(value: true);
	}

	public virtual void StuckInObject(Collider c)
	{
		jointedRigidBody = c.attachedRigidbody;
		joint = base.gameObject.AddComponent<FixedJoint>();
		joint.connectedBody = jointedRigidBody;
		joint.massScale = 0.05f;
		base.rb.mass = 0.1f;
	}

	public virtual void Drop(Vector3 force, float torque = 0f)
	{
		base.rb.AddForce(force, ForceMode.Impulse);
		if (torque != 0f)
		{
			base.rb.AddTorque(-base.t.right * torque, ForceMode.Impulse);
		}
	}

	public void Interact()
	{
		if (Game.player.weapons.currentWeapon == -1)
		{
			Dash();
		}
	}

	public override void PreDash()
	{
		SetKinematic(value: true);
		Game.player.weapons.DropCurrentWeapon(Vector3.up);
	}

	public override void Dash()
	{
		if ((bool)connectedBody)
		{
			connectedBody.rbs[connectedBody.indexCoreBody].AddForce(-base.t.up * 30f, ForceMode.Impulse);
			connectedBody = null;
			QuickEffectsPool.Get("Damage", base.t.position, Quaternion.identity).Play();
		}
		if ((bool)jointedRigidBody)
		{
			jointedRigidBody.AddForce(Vector3.up * 10f, ForceMode.Impulse);
		}
		Game.player.weapons.PickWeapon(associatedScriptableObject.index);
		if (hotPickTimer > 0f)
		{
			StyleRanking.instance.RegStylePoint(Game.style.WeaponsQuickAirPick);
		}
		QuickEffectsPool.Get("Poof", base.t.position).Play();
		SetKinematic(value: false);
		base.gameObject.SetActive(value: false);
	}
}
