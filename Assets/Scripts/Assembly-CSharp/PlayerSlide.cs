using System;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerSlide : MonoBehaviour
{
	public static Action<Vector3> OnSlideStart = delegate
	{
	};

	public static Action OnSlideEnd = delegate
	{
	};

	public static Action OnSlideBash = delegate
	{
	};

	public PlayerController p;

	public Transform tSlideFX;

	public AudioClip sfxFailed;

	public GameObject triggerObj;

	public DamageType dmg_Bash;

	private Vector3 defaultCenter = Vector3.zero;

	private Vector4 slideCenter = new Vector3(0f, -0.4f, 0f);

	private DamageData damage = new DamageData();

	private Collider[] colliders = new Collider[1];

	public Vector3 slideDir { get; private set; }

	public bool isSliding => slideState != 0;

	public int slideState { get; private set; }

	public float slideTimer { get; private set; }

	private void Awake()
	{
		Grounder grounder = p.grounder;
		grounder.OnGrounded = (Action)Delegate.Combine(grounder.OnGrounded, new Action(OnGrounded));
		damage.amount = 30f;
		damage.knockdown = true;
		damage.newType = dmg_Bash;
	}

	private void OnDestroy()
	{
		Grounder grounder = p.grounder;
		grounder.OnGrounded = (Action)Delegate.Remove(grounder.OnGrounded, new Action(OnGrounded));
	}

	private void OnGrounded()
	{
		if (p.SlideHolded() && Physics.Raycast(p.t.position, Vector3.down, 2f, 1))
		{
			Slide(forced: true);
		}
	}

	public void Extend(float t = 0.25f)
	{
		if (slideState > 0)
		{
			slideTimer += t;
		}
	}

	public bool TryBash()
	{
		Physics.OverlapSphereNonAlloc(p.tHead.position + p.tHead.forward, 1f, colliders, 17408);
		if (colliders[0] != null)
		{
			int layer = colliders[0].gameObject.layer;
			bool isKinematic = colliders[0].attachedRigidbody.isKinematic;
			if (layer == 10 || !isKinematic)
			{
				Debug.DrawRay(p.t.position, Vector3.up, Color.yellow, 4f);
				p.grounder.Ungrounded(forced: true);
				p.airControlBlock = 0.2f;
				p.rb.position = colliders[0].ClosestPointOnBounds(p.rb.position);
				p.rb.velocity = Vector3.zero;
				p.rb.AddForce((Vector3.up - p.tHead.forward.With(null, 0f)).normalized * 15f, ForceMode.Impulse);
			}
			p.sway.Sway(0f, 0f, 15f, 3f);
			CameraController.shake.Shake(2);
			QuickEffectsPool.Get("Bash", colliders[0].bounds.center).Play();
			damage.dir = p.tHead.forward.With(null, 0.5f).normalized;
			colliders[0].GetComponent<IDamageable<DamageData>>().Damage(damage);
			if (OnSlideBash != null)
			{
				OnSlideBash();
			}
			colliders[0] = null;
			return layer == 10 || !isKinematic;
		}
		return false;
	}

	private void SlideBashWeaponGrab()
	{
		BodyCollider component = colliders[0].GetComponent<BodyCollider>();
		BaseEnemy baseEnemy = (component ? component.body.enemy : colliders[0].GetComponent<BaseEnemy>());
		if (baseEnemy.dead)
		{
			p.weapons.PickWeapon(baseEnemy.weapon.index);
		}
	}

	public void Interrupt()
	{
		DeactivateTrigger();
		if (slideState > 0)
		{
			slideState = 1;
		}
		slideTimer = 0f;
	}

	public void DeactivateTrigger()
	{
		if (triggerObj.activeInHierarchy)
		{
			triggerObj.SetActive(value: false);
		}
	}

	public void Slide(bool forced = false)
	{
		if (slideState != 0 || !p.grounder.grounded)
		{
			return;
		}
		if (p.grounder.grounded && (p.GetInputDirNotAligned().sqrMagnitude > 0.25f || forced))
		{
			slideTimer = 0.6f;
			slideState = 3;
			p.sway.Sway(0f, 0f, 2.5f, 2f);
			triggerObj.SetActive(value: true);
			QuickEffectsPool.Get("Poof", p.t.position).Play();
			if (OnSlideStart != null)
			{
				OnSlideStart(p.t.position);
			}
		}
		else if (p.GetInputDirNotAligned().sqrMagnitude > 0.25f)
		{
			p.headPosition.Bounce();
			p.sway.Sway(0f, 0f, 5f, 3f);
			Game.sounds.PlayClip(sfxFailed);
		}
	}

	public void SlidingUpdate()
	{
		switch (slideState)
		{
		case 3:
		{
			tSlideFX.gameObject.SetActive(value: true);
			float magnitude = p.rb.velocity.magnitude;
			float num = 1f - Mathf.Clamp01((magnitude - 12f) / 10f);
			num *= 5f;
			if (p.gDir.sqrMagnitude != 0f)
			{
				slideDir = p.gDir.normalized;
				p.rb.velocity = slideDir * magnitude;
				p.rb.AddForce(slideDir * num, ForceMode.Impulse);
				Debug.DrawRay(p.t.position, p.gDir, Color.blue, 2f);
			}
			else
			{
				slideDir = Vector3.ProjectOnPlane(p.tHead.forward, p.grounder.gNormal).normalized;
				p.rb.AddForce(slideDir * num, ForceMode.Impulse);
			}
			p.headPosition.ChangeYPosition(-0.25f);
			p.clldr.height = 0.8f;
			p.clldr.center = slideCenter;
			slideState--;
			break;
		}
		case 2:
			if (p.vel.sqrMagnitude >= 4f && slideTimer != 0f)
			{
				slideTimer = Mathf.MoveTowards(slideTimer, 0f, Time.deltaTime);
				Game.player.camController.Angle(8.5f + p.h * 2.5f);
				tSlideFX.rotation = Quaternion.LookRotation(p.rb.velocity, p.grounder.gNormal);
			}
			else if (p.gVel.y >= -1f)
			{
				slideState--;
			}
			break;
		case 1:
			if (Physics.Raycast(p.t.TransformPoint(p.clldr.center), Vector3.up, 1f, 1))
			{
				slideTimer = 0.2f;
				slideState = 2;
				break;
			}
			tSlideFX.gameObject.SetActive(value: false);
			p.headPosition.ChangeYPosition(0.75f);
			p.clldr.height = 2f;
			p.clldr.center = defaultCenter;
			p.sway.Sway(2f, 0f, 0f, 3f);
			if (p.grounder.grounded)
			{
				p.footsteps.PlayLandingSound();
				p.NewGroundedPoint();
			}
			slideState--;
			if (triggerObj.activeInHierarchy)
			{
				triggerObj.SetActive(value: false);
			}
			if (OnSlideEnd != null)
			{
				OnSlideEnd();
			}
			break;
		}
	}

	private void OnCollisionEnter(Collision c)
	{
		if (isSliding && p.weapons.kickController.isCharging && c.gameObject.layer == 10 && p.ViewAnglePlane(c.rigidbody.worldCenterOfMass) < 90f)
		{
			p.weapons.kickController.Release();
		}
	}
}
