using System;
using UnityEngine;

public class BodyCollider : MonoBehaviour, IDamageable<DamageData>, IKickable<Vector3>
{
	public static Action<GameObject> OnBodyDamage = delegate
	{
	};

	public AudioClip sound;

	private Vector3 dir;

	private DamageData damage = new DamageData();

	private Quaternion deltaRotation;

	public Transform t { get; private set; }

	public Body body { get; private set; }

	private void Awake()
	{
		t = base.transform;
		body = GetComponentInParent<Body>();
	}

	public void Kick(Vector3 dir)
	{
		body.Kick(dir);
	}

	public void Damage(DamageData damage)
	{
		body.Damage(damage);
		if (OnBodyDamage != null)
		{
			OnBodyDamage(base.gameObject);
		}
	}

	private void OnCollisionEnter(Collision c)
	{
		if (body.damageTimer > 0f)
		{
			Debug.DrawRay(c.contacts[0].point, c.contacts[0].normal, Color.red, 3f);
			return;
		}
		int layer = c.gameObject.layer;
		float sqrMagnitude = c.relativeVelocity.sqrMagnitude;
		switch (layer)
		{
		case 0:
			if (sqrMagnitude > 16f)
			{
				if (sqrMagnitude > 225f || body.fallDist > 12f)
				{
					damage.dir = c.contacts[0].normal;
					damage.amount = 60f;
					damage.knockdown = true;
					damage.newType = Game.style.basicBluntHit;
					damage.stylePoint = Game.style.bodyHardLanding;
					body.Damage(damage);
					body.ResetFallDist();
				}
				else
				{
					QuickEffectsPool.Get("Poof", body.rb.position).Play();
				}
				Game.sounds.PlayClipAtPosition(body.sounds.impacts.GetRandom(), 0.5f, t.position);
			}
			break;
		case 10:
		{
			Debug.DrawRay(c.contacts[0].point, c.contacts[0].normal, Color.yellow, 3f);
			if (!(sqrMagnitude > 4f) || !c.rigidbody.isKinematic)
			{
				break;
			}
			Vector3 v = c.collider.ClosestPoint(t.position);
			dir = t.position.DirTo(v.With(null, t.position.y));
			BaseEnemy component = c.collider.GetComponent<BaseEnemy>();
			if ((bool)component)
			{
				bool flag = component.flammable.CheckFire(body.flammable);
				if (component.shieldDestroyed)
				{
					damage.dir = (dir + Vector3.up).normalized;
					damage.amount = 20f;
					damage.knockdown = true;
					damage.newType = StyleData.instance.basicBodyHit;
					damage.stylePoint = (flag ? StyleData.instance.bodyFlamingDomino : StyleData.instance.bodyDomino);
					component.Damage(damage);
					body.PushBody((-dir + Vector3.up).normalized, 8f);
					QuickEffectsPool.Get("Contact", c.contacts[0].point + c.contacts[0].normal).Play();
					CameraController.shake.Shake(1);
				}
				else
				{
					component.Kick(dir);
					damage.dir = (-dir + Vector3.up).normalized;
					damage.amount = 20f;
					damage.newType = StyleData.instance.basicBodyHit;
					damage.stylePoint = StyleData.instance.bodyShieldHit;
					body.Damage(damage);
					CameraController.shake.Shake(1);
				}
			}
			else
			{
				body.PushBody((-dir + Vector3.up).normalized, 8f);
			}
			QuickEffectsPool.Get("Kick Hit", c.collider.ClosestPoint(t.position)).Play();
			break;
		}
		case 14:
			if (!(sqrMagnitude > 4f) || c.collider.CompareTag("Target"))
			{
				break;
			}
			c.collider.GetComponent<BreakableB>();
			damage.dir = -c.contacts[0].normal;
			damage.amount = sqrMagnitude / 2.5f;
			damage.knockdown = true;
			damage.newType = Game.style.basicBluntHit;
			damage.dir = body.rb.position.DirTo(c.collider.ClosestPoint(body.rb.position));
			c.collider.GetComponent<IDamageable<DamageData>>().Damage(damage);
			if (!body.enemy.dead)
			{
				if (c.gameObject.activeInHierarchy)
				{
					damage.amount = 10f;
					damage.stylePoint = null;
					QuickEffectsPool.Get("Contact", c.contacts[0].point + c.contacts[0].normal).Play();
					body.Damage(damage);
					body.PushBody((Vector3.up / 2f + c.contacts[0].normal / 2f).normalized);
					body.RotateBody(0f, 45f, 0f);
				}
				else
				{
					damage.amount = 40f;
					damage.dir += Vector3.up;
					damage.stylePoint = Game.style.bodyBreakThrough;
					deltaRotation = Quaternion.Euler(-90f, 0f, 0f);
					body.Damage(damage, withKick: true);
				}
			}
			break;
		}
	}
}
