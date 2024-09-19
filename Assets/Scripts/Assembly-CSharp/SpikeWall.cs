using System;
using UnityEngine;

public class SpikeWall : MonoBehaviour
{
	public Transform tSpikes;

	private Transform t;

	private Animator animator;

	private AudioSource source;

	private bool activated;

	private DamageData damage = new DamageData();

	private Vector4 pDamage;

	private void Awake()
	{
		t = base.transform;
		animator = GetComponentInChildren<Animator>();
		source = GetComponent<AudioSource>();
		tSpikes.localPosition = new Vector3(1f, 0f, 0f);
		pDamage = tSpikes.right;
		pDamage.w = 300f;
		damage.dir = base.transform.right;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		activated = false;
		animator.Rebind();
		tSpikes.localPosition = new Vector3(1f, 0f, 0f);
	}

	private void OnTriggerEnter(Collider c)
	{
		if (!activated)
		{
			animator.SetTrigger("Out");
			switch (c.gameObject.layer)
			{
			case 9:
				c.GetComponent<IDamageable<Vector4>>().Damage(pDamage);
				animator.SetTrigger("In");
				break;
			case 10:
				damage.amount = 300f;
				damage.dir = t.right;
				damage.newType = Game.style.basicSpikes;
				c.GetComponent<IDamageable<DamageData>>().Damage(damage);
				activated = true;
				break;
			case 14:
			{
				Vector3 normal = -t.right;
				normal = CrowdControl.instance.GetClosestDirectionToNormal(c.transform.position, normal, 30f);
				c.attachedRigidbody.velocity = Vector3.zero;
				c.GetComponent<IKickable<Vector3>>().Kick(normal);
				c.attachedRigidbody.AddForce(Vector3.up * 10f, ForceMode.Impulse);
				animator.SetTrigger("In");
				Debug.DrawRay(base.transform.position, normal, Color.magenta, 2f);
				break;
			}
			}
			QuickEffectsPool.Get("Spikes", t.position, Quaternion.LookRotation(-t.right)).Play();
		}
	}
}
