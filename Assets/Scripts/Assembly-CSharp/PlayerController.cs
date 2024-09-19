using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable<Vector4>, IKickable<Vector3>
{
	public static PlayerController instance;

	public static readonly string[] states = new string[4] { "Default", "Slide", "Jump", "Parkour" };

	public static bool gamepad = false;

	public static Action OnInputTypeChange = delegate
	{
	};

	public static Action OnNewGroundedPoint = delegate
	{
	};

	public static Action<Vector3> OnDamage = delegate
	{
	};

	public static Action OnPlayerDie = delegate
	{
	};

	public static Action OnParkourMove = delegate
	{
	};

	public static Action<ContactPoint> OnWallHit = delegate
	{
	};

	public static Action<BaseEnemy> OnEnemyJump = delegate
	{
	};

	public KeyboardInputs inputs;

	public IPlatformable onPlatformable;

	public ParticleSystem parkourActionFX;

	public bool platformableCounted;

	public bool jumpHolded;

	public bool dashHolded;

	public bool speedControl;

	public bool dashPossible;

	public GameObject _prefabPlayerUI;

	public float dotN;

	private float hTemp;

	private float vTemp;

	private float dot;

	private float maxSpeed = 10.5f;

	private float blockTimer;

	private float lastTimeGrabbed;

	private float damageTimer;

	public float gravity = -40f;

	public float airControlBlock;

	public float airControl = 1f;

	public float defaultSpeed = 1f;

	public float gTimer;

	public float jumpBuffer;

	public float ringTimer;

	private Vector3 lastPos;

	private Vector3 inputDir;

	private Vector3 jumpForce = new Vector3(0f, 15f, 0f);

	public AudioClip damageSound;

	public AudioClip jumpSound;

	public AudioClip sfxSlideJump;

	public AudioClip landSound;

	public AudioClip climbSound;

	public AudioClip sfxSlideSlopeJump;

	public AudioClip sfxChainGrab;

	public AudioClip sfxChainOff;

	public AudioClip sfxBodyWallHit;

	public AudioClip sfxSpawnSwoosh;

	public AudioClip sfxFocusSwoosh;

	public AudioClip sfxFocusSwooshLight;

	public AudioClip sfxWeaponJump;

	private Vector3 gDirCross;

	private Vector3 gDirCrossProject;

	private RaycastHit hit;

	private DamageData dmg = new DamageData();

	private Vector3[] lastGroundedOffsets = new Vector3[4]
	{
		new Vector3(-0.5f, 0f, -0.5f),
		new Vector3(-0.5f, 0f, 0.5f),
		new Vector3(0.5f, 0f, -0.5f),
		new Vector3(0.5f, 0f, 0.5f)
	};

	private Collider[] clldrs = new Collider[1];

	public Transform t { get; private set; }

	public Transform tHead { get; private set; }

	public Rigidbody rb { get; private set; }

	public CapsuleCollider clldr { get; private set; }

	public Grounder grounder { get; private set; }

	public PlayerSlide slide { get; private set; }

	public PlayerClimb climb { get; private set; }

	public PlayerDash dash { get; private set; }

	public PlayerHead head { get; private set; }

	public PlayerLand land { get; private set; }

	public CameraSway sway { get; private set; }

	public PlayerWeapons weapons { get; private set; }

	public PlayerFootsteps footsteps { get; private set; }

	public MouseLook mouseLook { get; private set; }

	public CameraController camController { get; private set; }

	public CameraBob bob { get; private set; }

	public HeadPosition headPosition { get; private set; }

	public CameraFOV fov { get; private set; }

	public int parkourActionsCount { get; private set; }

	public bool isInvinsible { get; private set; }

	public bool inputActive { get; private set; }

	public float lifetime { get; private set; }

	public float h { get; private set; }

	public float v { get; private set; }

	public Vector3 vel { get; private set; }

	public bool chainLanding { get; private set; }

	public bool isDead { get; private set; }

	public bool isDamaged => damageTimer > 0f;

	public float speed { get; private set; }

	public Vector3 oldVel { get; private set; }

	public Vector3 gDir { get; private set; }

	public Vector3 gVel { get; private set; }

	public Vector3 lastGroundedPosition { get; private set; }

	public Vector3 lastGroundedDirection { get; private set; }

	public KeyCode upKey { get; private set; }

	public KeyCode leftKey { get; private set; }

	public KeyCode downKey { get; private set; }

	public KeyCode rightKey { get; private set; }

	public KeyCode slideKey { get; private set; }

	public KeyCode jumpKey { get; private set; }

	public KeyCode restartKey { get; private set; }

	public KeyCode kickKey { get; private set; }

	public KeyCode attackKey { get; private set; }

	public KeyCode altKey { get; private set; }

	public KeyCode dashKey { get; private set; }

	public KeyCode zenKey { get; private set; }

	private void Awake()
	{
		_prefabPlayerUI = UnityEngine.Object.Instantiate(_prefabPlayerUI);
		_prefabPlayerUI.transform.SetAsFirstSibling();
		instance = this;
		t = base.transform;
		tHead = t.Find("Head Pivot").transform;
		rb = GetComponent<Rigidbody>();
		rb.sleepThreshold = 0f;
		speed = defaultSpeed;
		clldr = GetComponent<CapsuleCollider>();
		grounder = GetComponent<Grounder>();
		Grounder obj = grounder;
		obj.OnGrounded = (Action)Delegate.Combine(obj.OnGrounded, new Action(Grounded));
		Grounder obj2 = grounder;
		obj2.OnFakeGrounded = (Action)Delegate.Combine(obj2.OnFakeGrounded, new Action(FakeGrounded));
		Grounder obj3 = grounder;
		obj3.OnUngrounded = (Action)Delegate.Combine(obj3.OnUngrounded, new Action(Ungrounded));
		head = GetComponentInChildren<PlayerHead>(includeInactive: true);
		slide = GetComponent<PlayerSlide>();
		climb = GetComponent<PlayerClimb>();
		land = GetComponent<PlayerLand>();
		dash = GetComponent<PlayerDash>();
		footsteps = GetComponent<PlayerFootsteps>();
		sway = GetComponentInChildren<CameraSway>();
		camController = GetComponentInChildren<CameraController>();
		bob = tHead.GetComponentInChildren<CameraBob>();
		headPosition = tHead.GetComponentInChildren<HeadPosition>();
		mouseLook = tHead.GetComponentInChildren<MouseLook>();
		weapons = tHead.GetComponentInChildren<PlayerWeapons>();
		fov = tHead.GetComponentInChildren<CameraFOV>();
		UpdateInputs();
		QuickRebindMenu.OnRebinded = (Action)Delegate.Combine(QuickRebindMenu.OnRebinded, new Action(UpdateInputs));
	}

	private void OnDestroy()
	{
		Grounder obj = grounder;
		obj.OnGrounded = (Action)Delegate.Remove(obj.OnGrounded, new Action(Grounded));
		Grounder obj2 = grounder;
		obj2.OnFakeGrounded = (Action)Delegate.Remove(obj2.OnFakeGrounded, new Action(FakeGrounded));
		Grounder obj3 = grounder;
		obj3.OnUngrounded = (Action)Delegate.Remove(obj3.OnUngrounded, new Action(Ungrounded));
		RageOFF();
		QuickRebindMenu.OnRebinded = (Action)Delegate.Remove(QuickRebindMenu.OnRebinded, new Action(UpdateInputs));
	}

	private void OnEnable()
	{
		NewGroundedPoint();
		damageTimer = 0f;
		if ((bool)DamageUI.instance)
		{
			DamageUI.instance.cg.alpha = 0f;
		}
		if ((bool)_prefabPlayerUI)
		{
			_prefabPlayerUI.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if ((bool)_prefabPlayerUI)
		{
			_prefabPlayerUI.SetActive(value: false);
		}
	}

	private void UpdateInputs()
	{
		upKey = inputs.playerKeys[0].key;
		leftKey = inputs.playerKeys[1].key;
		downKey = inputs.playerKeys[2].key;
		rightKey = inputs.playerKeys[3].key;
		kickKey = inputs.playerKeys[4].key;
		slideKey = inputs.playerKeys[5].key;
		jumpKey = inputs.playerKeys[6].key;
		restartKey = inputs.playerKeys[7].key;
		attackKey = inputs.playerKeys[8].key;
		altKey = inputs.playerKeys[9].key;
		dashKey = inputs.playerKeys[10].key;
		zenKey = inputs.playerKeys[11].key;
	}

	public void MakeInvinsible(bool value)
	{
		if (isInvinsible != value)
		{
			isInvinsible = value;
		}
	}

	public void PullTo(Vector3 targetPos, bool withPound = false)
	{
		float num = tHead.position.y - targetPos.y;
		weapons.daggerController.source.PlayClip(weapons.daggerController.sfxLaunch);
		if (withPound && num > 2f && Game.player.tHead.forward.y.InDegrees() > 30f && parkourActionsCount > 0 && Physics.Raycast(t.position, Vector3.down, 16f, 1))
		{
			airControlBlock = 0.2f;
			rb.velocity = (t.position.DirTo(targetPos) + Vector3.down).normalized * 30f;
			chainLanding = true;
			return;
		}
		if (grounder.grounded)
		{
			grounder.Ungrounded();
		}
		ParkourMove();
		rb.velocity = Vector3.zero;
		rb.AddForce((t.position.DirTo(targetPos) + Vector3.up).normalized * Mathf.Lerp(36f, 12f, Mathf.Clamp01(num / 6f)), ForceMode.Impulse);
		airControlBlock = 0.2f;
		CameraController.shake.Shake(1);
	}

	public void PullInDir(Vector3 targetPos)
	{
		_ = tHead.position;
		weapons.daggerController.source.PlayClip(weapons.daggerController.sfxLaunch);
		Vector3 vector = t.position.DirTo(targetPos);
		if (grounder.grounded)
		{
			grounder.Ungrounded();
		}
		ParkourMove();
		rb.velocity = Vector3.zero;
		rb.AddForce(vector * 32f, ForceMode.Impulse);
		airControlBlock = 0.1f;
		CameraController.shake.Shake(1);
	}

	public bool Grab(IPlatformable platformable, bool kinematic = true)
	{
		if (Time.timeSinceLevelLoad - lastTimeGrabbed < 0.3f || onPlatformable != null || grounder.grounded || rb.isKinematic)
		{
			return false;
		}
		onPlatformable = platformable;
		grounder.enabled = false;
		weapons.DropLifted();
		weapons.gameObject.SetActive(value: false);
		if (!platformableCounted && (platformable.GetType() == typeof(SwingRope) || platformable.GetType() == typeof(Zipline)))
		{
			ParkourMove();
			platformableCounted = true;
		}
		if (kinematic)
		{
			SetKinematic(value: true);
			fov.kinematicFOV = 0f;
		}
		return true;
	}

	public void Drop()
	{
		onPlatformable = null;
		jumpHolded = false;
		lastTimeGrabbed = Time.timeSinceLevelLoad;
		grounder.enabled = true;
		weapons.gameObject.SetActive(value: true);
		if (rb.isKinematic)
		{
			SetKinematic(value: false);
		}
	}

	public void SetKinematic(bool value)
	{
		if (value)
		{
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			rb.isKinematic = true;
			rb.velocity = Vector3.zero;
		}
		else
		{
			rb.isKinematic = false;
			rb.collisionDetectionMode = ((!grounder.grounded) ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete);
		}
	}

	private void OnCollisionEnter(Collision c)
	{
		if (onPlatformable != null && onPlatformable.GetType() == typeof(SwingRope) && c.relativeVelocity.sqrMagnitude > 16f)
		{
			if (Vector3.Dot(c.contacts[0].normal, oldVel.normalized).Abs() > 0.5f)
			{
				onPlatformable.Drop();
				rb.velocity = c.contacts[0].normal * 5f;
				airControlBlock = 0.2f;
				CameraController.shake.Shake(2);
				Game.sounds.PlayClipAtPosition(sfxBodyWallHit, 1f, t.position);
			}
			else
			{
				rb.AddForce(c.contacts[0].normal * 5f, ForceMode.Impulse);
				CameraController.shake.Shake(1);
				Game.sounds.PlayClipAtPosition(sfxBodyWallHit, 1f, t.position);
			}
		}
		if (c.contacts[0].normal.y.Abs() < 0.5f && OnWallHit != null)
		{
			OnWallHit(c.contacts[0]);
		}
		if (vel.y > -1f && weapons.ReadyToLift() && c.collider.gameObject.layer == 14 && !c.collider.attachedRigidbody.isKinematic)
		{
			weapons.Lift(c.collider);
		}
		if (c.collider.CompareTag("Danger"))
		{
			Damage(c.contacts[0].normal);
		}
	}

	public void Kick(Vector3 dir)
	{
		if (grounder.grounded)
		{
			grounder.Ungrounded();
			gTimer = 0f;
		}
		rb.velocity = rb.velocity.With(null, 0f);
		rb.AddForce(dir * 20f, ForceMode.Impulse);
	}

	public void Deactivate()
	{
		MouseLook obj = mouseLook;
		bool flag2 = (weapons.enabled = false);
		bool flag4 = (obj.enabled = flag2);
		inputActive = flag4;
		inputDir = Vector3.zero;
		if (slide.isSliding)
		{
			slide.Interrupt();
		}
	}

	public void Activate()
	{
		MouseLook obj = mouseLook;
		bool flag2 = (weapons.enabled = true);
		bool flag4 = (obj.enabled = flag2);
		inputActive = flag4;
		if (!weapons.gameObject.activeInHierarchy)
		{
			weapons.gameObject.SetActive(value: true);
		}
		grounder.Reset();
	}

	public bool IsActive()
	{
		if (inputActive)
		{
			return !rb.isKinematic;
		}
		return false;
	}

	private void Ungrounded()
	{
		if (rb.isKinematic)
		{
			return;
		}
		gTimer = 0.2f;
		if (slide.isSliding)
		{
			slide.Interrupt();
			if (rb.velocity.y > 0f && rb.velocity.With(null, 0f).sqrMagnitude > 4f && (1f - rb.velocity.normalized.y).InDegrees() > 10f)
			{
				SlopeJump((rb.velocity.normalized + Vector3.up).normalized);
			}
		}
	}

	private void FakeGrounded()
	{
		if (jumpBuffer > 0f)
		{
			CameraController.sway.Sway(3f, 0f, 0f, 3f);
			Jump();
			jumpBuffer = 0f;
			return;
		}
		if (grounder.jumpHeight != 0f)
		{
			headPosition.Bounce((0f - grounder.jumpHeight) / 12f);
		}
		footsteps.PlayLandingSound();
	}

	private void Grounded()
	{
		if (jumpHolded)
		{
			jumpHolded = false;
		}
		if (grounder.maxFallVelocity < -70f)
		{
			Die(Vector3.up);
			return;
		}
		if ((grounder.maxFallVelocity < -30f && !weapons.IsAttacking()) || chainLanding)
		{
			land.Land(chainLanding);
			chainLanding = false;
		}
		if (!rb.isKinematic)
		{
			rb.velocity = Vector3.ProjectOnPlane(vel, grounder.gNormal);
		}
		lastGroundedPosition = t.position;
		lastGroundedDirection = tHead.forward;
		gTimer = 0f;
		NewGroundedPoint();
	}

	public void Damage(Vector4 dir)
	{
		if (isDead || isInvinsible || damageTimer > 2.75f)
		{
			return;
		}
		if (dash.state != 0)
		{
			StyleRanking.instance.RegStylePoint(Game.style.savingDash);
			return;
		}
		if (Vector3.Angle(tHead.forward, -dir) < 90f && (weapons.ReactToDamage() || (bool)weapons.rbLifted))
		{
			if ((bool)weapons.rbLifted)
			{
				StyleRanking.instance.RegStylePoint(Game.style.objectShield);
				weapons.DropLifted(andBreak: true);
				if (grounder.grounded)
				{
					grounder.Ungrounded(forced: true);
					rb.AddForce(((Vector3)dir + Vector3.up) * 10f, ForceMode.Impulse);
				}
			}
			else if (grounder.grounded)
			{
				rb.AddForce(dir * (slide.isSliding ? 2 : 20), ForceMode.Impulse);
			}
			sway.Sway(-15f, 0f, 5f, 4f);
			QuickEffectsPool.Get("Block", tHead.position + tHead.forward, tHead.rotation).Play();
			return;
		}
		Game.time.SlowMotion(0.5f, 2f);
		if (OnDamage != null)
		{
			OnDamage(dir);
		}
		chainLanding = false;
		if (damageTimer <= 0f && dir.w < 100f)
		{
			if (!grounder.grounded)
			{
				airControlBlock = 0.2f;
				rb.velocity = (Vector3.up + (Vector3)dir).normalized * 20f;
			}
			else
			{
				rb.AddForce(Vector3.ProjectOnPlane(dir, grounder.gNormal) * 10f, ForceMode.Impulse);
			}
			sway.Sway(5f, 0f, 30f, 3f);
			damageTimer = 3f;
			CameraController.shake.Shake();
			QuickEffectsPool.Get("Damage", tHead.position, Quaternion.LookRotation(tHead.forward)).Play();
		}
		else
		{
			Die(dir);
		}
	}

	public void Die(Vector4 dir)
	{
		QuickEffectsPool.Get("Damage", tHead.position, Quaternion.LookRotation(dir)).Play();
		RageOFF();
		if (!Game.godmode)
		{
			if (OnPlayerDie != null)
			{
				OnPlayerDie();
			}
			Game.time.SlowMotion(0.1f, 0.75f, 0.1f);
			DamageUI.instance.cg.alpha = 1f;
			SetKinematic(value: true);
			camController.EnableCameraAndListener(value: false);
			weapons.gameObject.SetActive(value: false);
			head.Chop(camController.worldCam.transform, dir, dir.w >= 1000f);
			isDead = true;
			lifetime = 0f;
		}
	}

	public void Reset()
	{
		grounder.Reset();
		climb.Reset();
		slide.Interrupt();
		headPosition.Reset();
		MakeInvinsible(value: false);
		RageOFF();
		weapons.gameObject.SetActive(value: false);
		weapons.gameObject.SetActive(value: true);
		weapons.SetWeapon(-1);
		if (dash.isDashing)
		{
			dash.Reset();
		}
		float num2 = (v = 0f);
		h = num2;
		hTemp = (vTemp = 0f);
		inputDir.x = 0f;
		inputDir.y = 0f;
		inputDir.z = 0f;
		damageTimer = 0f;
		isDead = false;
		isDead = false;
		rb.velocity = Vector3.zero;
		if (rb.isKinematic)
		{
			SetKinematic(value: false);
		}
		DamageUI.instance.cg.alpha = 0f;
		Game.message.Hide();
	}

	public void BackToLastGrounded()
	{
		NewGroundedPoint();
		t.position = lastGroundedPosition;
		Debug.DrawRay(lastGroundedPosition, Vector3.up, Color.yellow, 2f);
		Vector3 vector = default(Vector3);
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (Physics.Raycast(lastGroundedPosition + lastGroundedOffsets[i], Vector3.down, out hit, 2f, 1))
			{
				vector += hit.point;
				num++;
			}
		}
		if (num > 0)
		{
			vector /= (float)num;
			vector.y += 1f;
			t.position = vector;
			Debug.DrawRay(vector, Vector3.up, Color.green, 2f);
		}
		else
		{
			t.position = lastGroundedPosition;
		}
		mouseLook.LookInDir(lastGroundedDirection);
	}

	public void ParkourMove()
	{
		if (!grounder.grounded)
		{
			dashPossible = true;
			parkourActionsCount++;
			parkourActionFX.Play();
			if (OnParkourMove != null)
			{
				OnParkourMove();
			}
		}
	}

	public void ForceInDir(Vector3 dir)
	{
		gTimer = 0f;
		rb.velocity = Vector3.zero;
		rb.AddForce(dir, ForceMode.Impulse);
	}

	public void ObjectFlip()
	{
		if ((bool)grounder.gCollider && grounder.gCollider.gameObject.layer == 14)
		{
			Rigidbody attachedRigidbody = grounder.gCollider.attachedRigidbody;
			if ((bool)attachedRigidbody)
			{
				attachedRigidbody.AddForce(Vector3.up * (7f * attachedRigidbody.mass), ForceMode.Impulse);
				attachedRigidbody.AddTorque(tHead.forward * 90f, ForceMode.Impulse);
				QuickEffectsPool.Get("Poof", grounder.gPoint).Play();
			}
		}
	}

	public void Jump()
	{
		jumpHolded = false;
		if (slide.isSliding)
		{
			slide.Interrupt();
			if (slide.TryBash())
			{
				return;
			}
			Physics.Raycast(t.position, Vector3.down, out hit, 1.3f, 1);
			if (hit.distance != 0f)
			{
				if (hit.normal.y.InDegrees() > 15f)
				{
					Vector3 normalized = oldVel.normalized;
					if (Physics.Raycast(tHead.position, normalized, out hit, 8f, 1) && hit.normal.y.Abs().InDegrees() > 45f)
					{
						Debug.DrawLine(tHead.position, hit.point, Color.yellow, 2f);
						Debug.DrawRay(hit.point, hit.normal, Color.yellow, 2f);
						SlopeJump((Vector3.up + tHead.forward.With(null, 0f).normalized / 6f).normalized, vertical: true);
					}
					else
					{
						SlopeJump((normalized + Vector3.up).normalized);
					}
				}
				else
				{
					SlideJump();
				}
			}
			else
			{
				BasicJump();
			}
		}
		else
		{
			BasicJump();
		}
	}

	public void BasicJump(float multiplier = 1f)
	{
		if (grounder.grounded)
		{
			ObjectFlip();
		}
		grounder.Ungrounded(forced: true);
		gTimer = 0f;
		rb.velocity = rb.velocity.With(null, 0f);
		rb.AddForce(jumpForce * multiplier, ForceMode.Impulse);
		Game.sounds.PlayClipAtPosition(jumpSound, 0.5f, t.position);
	}

	public void SlideJump()
	{
		grounder.Ungrounded(forced: true);
		gTimer = 0f;
		rb.velocity = (rb.velocity * 1.1f).With(null, 0f);
		rb.AddForce(new Vector3(0f, 16.5f, 0f), ForceMode.Impulse);
		Game.sounds.PlayClipAtPosition(sfxSlideJump, 0.5f, t.position);
	}

	public void SlopeJump(Vector3 dir, bool vertical = false)
	{
		grounder.Ungrounded(forced: true);
		gTimer = 0f;
		rb.velocity = Vector3.zero;
		rb.AddForce(dir * (vertical ? 34 : 32), ForceMode.Impulse);
		airControlBlock = 0.2f;
		ParkourMove();
		sway.Sway(2f, 0f, 4f, 3f);
		CameraController.shake.Shake(2);
		Game.sounds.PlayClipAtPosition(sfxSlideSlopeJump, 1f, t.position);
	}

	private void JumpOrClimb()
	{
		if (!rb.isKinematic)
		{
			Debug.DrawRay(t.position, Vector3.up, Color.grey, 2f);
			if (grounder.grounded || (gTimer > 0f && airControlBlock <= 0f))
			{
				Jump();
			}
			else if (!JumpBoost() && !climb.TryToClimb())
			{
				jumpBuffer = 0.1f;
			}
		}
	}

	private bool JumpBoost()
	{
		Physics.OverlapCapsuleNonAlloc(t.position, t.position + Vector3.down * 2f, 1f, clldrs, 58368);
		if (clldrs[0] != null)
		{
			switch (clldrs[0].gameObject.layer)
			{
			case 10:
				if (clldrs[0].attachedRigidbody.isKinematic)
				{
					dmg.amount = 20f;
					dmg.knockdown = ((parkourActionsCount >= 1) ? true : false);
					dmg.dir = (-rb.velocity.With(null, 0f) + Vector3.up).normalized;
					dmg.newType = weapons.unarmedEnemyJump;
					clldrs[0].GetComponent<IDamageable<DamageData>>().Damage(dmg);
					if (OnEnemyJump != null)
					{
						OnEnemyJump(clldrs[0].GetComponent<BaseEnemy>());
					}
				}
				else
				{
					float y = clldrs[0].attachedRigidbody.position.y;
					Debug.DrawLine(clldrs[0].attachedRigidbody.worldCenterOfMass, t.position, Color.yellow, 2f);
					if (y > t.position.y)
					{
						t.position += Vector3.up * (y - t.position.y + 0.5f).Abs();
					}
					dmg.amount = 0f;
					dmg.knockdown = true;
					dmg.dir = Vector3.down;
					dmg.newType = weapons.unarmedEnemyKnockedJump;
					clldrs[0].GetComponent<IDamageable<DamageData>>().Damage(dmg);
				}
				BasicJump(1.6f);
				sway.Sway(Mathf.Clamp(vel.magnitude, 5f, 10f), 0f, 5f, 3f);
				CameraController.shake.Shake(2);
				QuickPool.instance.Get("Slam FX", t.position, Quaternion.LookRotation(Vector3.up)).GetComponent<TrailScript>().Play();
				ParkourMove();
				break;
			case 14:
				dmg.amount = 10f;
				dmg.knockdown = false;
				dmg.dir = t.forward;
				dmg.newType = weapons.unarmedEnemyJump;
				clldrs[0].GetComponent<IDamageable<DamageData>>().Damage(dmg);
				sway.Sway(Mathf.Clamp(vel.magnitude, 5f, 10f), 0f, 3f, 3f);
				BasicJump((rb.velocity.y > 1f) ? 1.75f : 1.5f);
				CameraController.shake.Shake(2);
				QuickPool.instance.Get("Slam FX", t.position, Quaternion.LookRotation(Vector3.up)).GetComponent<TrailScript>().Play();
				ParkourMove();
				break;
			case 13:
				clldrs[0].GetComponent<IInteractable>().Interact();
				sway.Sway(Mathf.Clamp(vel.magnitude, 5f, 10f), 0f, 5f, 3f);
				BasicJump(2f);
				airControlBlock = 0.1f;
				CameraController.shake.Shake(2);
				QuickEffectsPool.Get("Enemy Jump", t.position, Quaternion.LookRotation(Vector3.up)).Play();
				Game.sounds.PlayClipAtPosition(sfxWeaponJump, 1f, t.position);
				StyleRanking.instance.RegStylePoint(Game.style.WeaponsJumpPick);
				ParkourMove();
				break;
			case 15:
				sway.Sway(Mathf.Clamp(vel.magnitude, 5f, 10f), 0f, 5f, 3f);
				clldrs[0].GetComponent<Jumpable>().OnJump();
				CameraController.shake.Shake(1);
				ParkourMove();
				break;
			}
			clldrs[0] = null;
			jumpHolded = false;
			return true;
		}
		return false;
	}

	public float HAxis()
	{
		return 0f + (float)(Input.GetKey(leftKey) ? (-1) : 0) + (float)(Input.GetKey(rightKey) ? 1 : 0);
	}

	public float VAxis()
	{
		return 0f + (float)(Input.GetKey(downKey) ? (-1) : 0) + (float)(Input.GetKey(upKey) ? 1 : 0);
	}

	public bool AttackPressed()
	{
		if (!Input.GetKeyDown(attackKey))
		{
			return InputsManager.rTriggerPressed;
		}
		return true;
	}

	public bool AttackHolded()
	{
		if (!Input.GetKey(attackKey))
		{
			return InputsManager.rTriggerHolded;
		}
		return true;
	}

	public bool AttackReleased()
	{
		return Input.GetKeyUp(attackKey);
	}

	public bool AltPressed()
	{
		if (!Input.GetKeyDown(altKey))
		{
			return InputsManager.lTriggerPressed;
		}
		return true;
	}

	public bool AltHolded()
	{
		if (!Input.GetKey(altKey))
		{
			return InputsManager.lTriggerHolded;
		}
		return true;
	}

	public bool AltReleased()
	{
		return Input.GetKeyUp(altKey);
	}

	public bool SlidePressed()
	{
		if (!Input.GetKeyDown(slideKey))
		{
			return Input.GetButtonDown("Slide");
		}
		return true;
	}

	public bool SlideHolded()
	{
		if (!Input.GetKey(slideKey))
		{
			return Input.GetButton("Slide");
		}
		return true;
	}

	public bool JumpPressed()
	{
		if (!Input.GetKeyDown(jumpKey))
		{
			return Input.GetButtonDown("Jump");
		}
		return true;
	}

	public bool JumpHolded()
	{
		if (!Input.GetKey(jumpKey))
		{
			return Input.GetButton("Jump");
		}
		return true;
	}

	public bool JumpReleased()
	{
		if (!Input.GetKeyUp(jumpKey))
		{
			return Input.GetButtonUp("Jump");
		}
		return true;
	}

	public bool DashPressed()
	{
		if (!Input.GetKeyDown(dashKey))
		{
			return Input.GetButtonDown("Dash");
		}
		return true;
	}

	public bool DashHolded()
	{
		if (!Input.GetKey(dashKey))
		{
			return Input.GetButton("Dash");
		}
		return true;
	}

	public bool DashReleased()
	{
		if (!Input.GetKeyUp(dashKey))
		{
			return Input.GetButtonUp("Dash");
		}
		return true;
	}

	public bool KickPressed()
	{
		if (!Input.GetKeyDown(kickKey))
		{
			return Input.GetButtonDown("Kick");
		}
		return true;
	}

	public bool KickHolded()
	{
		if (!Input.GetKey(kickKey))
		{
			return Input.GetButton("Kick");
		}
		return true;
	}

	public bool KickReleased()
	{
		if (!Input.GetKeyUp(kickKey))
		{
			return Input.GetButtonUp("Kick");
		}
		return true;
	}

	public bool ZenPressed()
	{
		if (!Input.GetKeyDown(zenKey))
		{
			return Input.GetButtonDown("Zen");
		}
		return true;
	}

	public bool RestartPressed()
	{
		if (!Input.GetKeyDown(restartKey))
		{
			return Input.GetButtonDown("Restart");
		}
		return true;
	}

	private void ControllerTriggersUpdate()
	{
		Input.GetAxis("Triggers");
	}

	private void ControllerLeftStickUpdate()
	{
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		if (axis.Abs() > 0.1f)
		{
			h = axis;
		}
		if (axis2.Abs() > 0.1f)
		{
			v = axis2;
		}
	}

	public void RageON()
	{
		if (Game.time.defaultTimeScale == 1f && !StyleRanking.rage)
		{
			QuickEffectsPool.Get("Rage", t.position, tHead.rotation).Play();
			StyleRanking.rage = true;
			Game.time.SetDefaultTimeScale(0.5f);
			CameraController.shake.Shake(1);
			CameraController.sway.Sway(5f, 0f, 2.5f, 4f);
		}
	}

	public void RageOFF()
	{
		StyleRanking.rage = false;
		if (Game.mission.state != MissionState.MissionStates.Complete)
		{
			Game.time.SetDefaultTimeScale(1f);
		}
	}

	private void InputUpdate()
	{
		vTemp = 0f;
		vTemp += (Input.GetKey(upKey) ? 1 : 0);
		vTemp += (Input.GetKey(downKey) ? (-1) : 0);
		hTemp = 0f;
		hTemp += (Input.GetKey(leftKey) ? (-1) : 0);
		hTemp += (Input.GetKey(rightKey) ? 1 : 0);
		v = vTemp;
		h = hTemp;
		fov.Tick();
		if (ZenPressed())
		{
			if (!StyleRanking.rage)
			{
				RageON();
			}
			else
			{
				RageOFF();
			}
		}
		if (!gamepad)
		{
			if (Input.GetButtonDown("JoyA"))
			{
				gamepad = true;
				if (OnInputTypeChange != null)
				{
					OnInputTypeChange();
				}
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				gamepad = false;
				if (OnInputTypeChange != null)
				{
					OnInputTypeChange();
				}
			}
			ControllerLeftStickUpdate();
		}
		inputDir.x = h;
		inputDir.y = 0f;
		inputDir.z = v;
		inputDir = inputDir.normalized;
		if (JumpPressed())
		{
			jumpHolded = true;
			JumpOrClimb();
		}
		if (jumpHolded)
		{
			if (JumpHolded())
			{
				climb.TryToClimb();
			}
			else
			{
				jumpHolded = false;
			}
		}
		if (SlidePressed())
		{
			slide.Slide();
		}
		if (DashHolded())
		{
			if (!dashHolded && dash.Dash())
			{
				dashHolded = true;
			}
		}
		else if (dashHolded)
		{
			dashHolded = false;
		}
		if (RestartPressed() && Game.loading == null)
		{
			head.RestorePlayer();
		}
	}

	public Vector3 GetInputDir()
	{
		return tHead.TransformDirection(inputDir);
	}

	public Vector3 GetInputDirNotAligned()
	{
		return inputDir;
	}

	public Vector3 GetGroundedDirection()
	{
		return Vector3.ProjectOnPlane(tHead.forward, grounder.gNormal).normalized;
	}

	public int GetState()
	{
		if (grounder.grounded)
		{
			if (slide.slideState != 0)
			{
				return 1;
			}
			return 0;
		}
		if (parkourActionsCount != 0)
		{
			return 3;
		}
		return 2;
	}

	private void BobAndFootstepsUpdate()
	{
		if (slide.slideState == 0 && climb.state == 0 && dash.state == 0 && !rb.isKinematic)
		{
			camController.Angle(h * -5f - damageTimer * 6f);
		}
		if (grounder.grounded && inputDir.sqrMagnitude > 0.25f && slide.slideState == 0)
		{
			if (gVel.sqrMagnitude > 1f)
			{
				bob.Bob(speed);
				footsteps.Tick();
			}
			else
			{
				bob.Bob(speed, 0f);
			}
		}
		else
		{
			bob.Bob(speed, 0f);
		}
	}

	public void NewGroundedPoint()
	{
		if (OnNewGroundedPoint != null)
		{
			OnNewGroundedPoint();
		}
	}

	private void Update()
	{
		if (isDead)
		{
			return;
		}
		lifetime += Time.deltaTime;
		if (grounder.grounded && !dashPossible)
		{
			dashPossible = true;
		}
		if (onPlatformable != null)
		{
			return;
		}
		if (inputActive)
		{
			InputUpdate();
		}
		else
		{
			float num2 = (v = 0f);
			h = num2;
			inputDir.x = (inputDir.y = (inputDir.z = 0f));
		}
		if (dash.state > 0)
		{
			dash.DashingUpdate();
		}
		if (slide.slideState > 0)
		{
			slide.SlidingUpdate();
		}
		if (climb.state > 0)
		{
			climb.ClimbingUpdate();
		}
		if (airControlBlock.MoveTowards(0f))
		{
			airControl = 0f;
		}
		else
		{
			airControl.MoveTowards(1f);
		}
		if (damageTimer != 0f)
		{
			damageTimer = Mathf.MoveTowards(damageTimer, 0f, Time.deltaTime);
			DamageUI.instance.cg.alpha = damageTimer / 3f * 3f;
			if (damageTimer == 0f)
			{
				RageOFF();
			}
		}
		speed = defaultSpeed + weapons.KickingOrHolding() + damageTimer / 2f;
		gTimer.MoveTowards(0f);
		jumpBuffer.MoveTowards(0f);
		ringTimer.MoveTowards(0f);
		BobAndFootstepsUpdate();
	}

	private void LateUpdate()
	{
		if (inputActive)
		{
			if (onPlatformable != null)
			{
				onPlatformable.Tick();
			}
			headPosition.Tick();
			if (parkourActionsCount > 0 && grounder.grounded)
			{
				parkourActionsCount = 0;
			}
			if (platformableCounted)
			{
				platformableCounted = false;
			}
		}
	}

	private void FixedUpdate()
	{
		if (onPlatformable != null)
		{
			if (!rb.isKinematic)
			{
				oldVel = rb.velocity;
			}
			return;
		}
		if (grounder.grounded && lifetime > 0.1f && Vector3.Distance(lastGroundedPosition, t.position) > 2f)
		{
			if (grounder.gNormal == Vector3.up)
			{
				lastGroundedPosition = t.position;
				lastGroundedDirection = tHead.forward;
			}
			NewGroundedPoint();
		}
		vel = rb.velocity;
		gVel = Vector3.ProjectOnPlane(vel, grounder.gNormal);
		gDir = tHead.TransformDirection(inputDir);
		gDirCross = Vector3.Cross(Vector3.up, gDir).normalized;
		gDirCrossProject = Vector3.ProjectOnPlane(grounder.gNormal, gDirCross);
		gDir = Vector3.Cross(gDirCross, gDirCrossProject);
		if (slide.slideState == 0)
		{
			dot = Vector3.Dot(gDir, gVel);
			dotN = Vector3.Dot(gDir.normalized, gVel.normalized);
			if (inputDir.sqrMagnitude > 0.25f)
			{
				if (grounder.grounded)
				{
					rb.AddForce(gDir * (maxSpeed * maxSpeed) - gVel * maxSpeed * speed);
				}
				else if (airControl > 0f)
				{
					speedControl = dot <= maxSpeed;
					if (!speedControl)
					{
						Debug.DrawRay(t.position, Vector3.up, Color.blue, 2f);
					}
					if (dot <= maxSpeed)
					{
						rb.AddForce((gDir * (maxSpeed * maxSpeed) - gVel * (maxSpeed * speed)) * airControl * ((!grounder.onSlope) ? 1 : 0));
					}
					else
					{
						float magnitude = rb.velocity.With(null, 0f).magnitude;
						magnitude = Mathf.Clamp(magnitude, maxSpeed, float.PositiveInfinity);
						Vector3 velocity = rb.velocity;
						velocity = Vector3.MoveTowards(velocity, gDir * magnitude, Time.deltaTime * 100f * airControl);
						velocity.y = vel.y;
						rb.velocity = velocity;
					}
				}
			}
			else if (grounder.grounded && gVel.sqrMagnitude != 0f)
			{
				rb.AddForce(-gVel * 10f);
			}
		}
		else if (slide.slideState == 2)
		{
			if (h.Abs() > 0.1f)
			{
				rb.AddForce(Vector3.Cross(slide.slideDir, grounder.gNormal) * (15f * (0f - h)));
			}
			if (v < -0.5f)
			{
				rb.AddForce(-vel.normalized * 20f);
			}
		}
		rb.AddForce(grounder.gNormal * gravity);
		oldVel = rb.velocity;
		if (!rb.isKinematic && rb.collisionDetectionMode != ((!grounder.grounded) ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete))
		{
			rb.collisionDetectionMode = ((!grounder.grounded) ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete);
		}
		Debug.DrawRay(grounder.gPoint, gVel.normalized * 0.2f, Color.blue);
		Debug.DrawRay(grounder.grounded ? grounder.gPoint : t.position, gDir, grounder.grounded ? Color.green : Color.red);
		Debug.DrawRay(grounder.gPoint, grounder.gNormal * 0.2f, Color.yellow);
	}

	public float ViewAngle(Vector3 pos)
	{
		return Vector3.Angle(tHead.forward, tHead.position.DirTo(pos));
	}

	public float ViewAnglePlane(Vector3 pos)
	{
		return Vector3.Angle(tHead.forward.With(null, 0f), tHead.position.DirTo(pos).With(null, 0f));
	}
}
