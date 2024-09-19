using System;
using UnityEngine;

public class Spikes : MonoBehaviour
{
	public bool triggerOnce;

	public Collider clldr;

	public DamageData damage = new DamageData();

	private Transform t;

	private void Awake()
	{
		t = base.transform;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		triggerOnce = false;
		clldr.enabled = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		switch (other.gameObject.layer)
		{
		case 10:
			if (!other.attachedRigidbody.isKinematic)
			{
				damage.dir = clldr.ClosestPoint(other.attachedRigidbody.position);
				other.GetComponent<IDamageable<DamageData>>().Damage(damage);
			}
			if (triggerOnce)
			{
				clldr.enabled = false;
			}
			break;
		case 14:
			damage.dir = (t.forward + t.up / 2f).normalized;
			other.GetComponent<IDamageable<DamageData>>().Damage(damage);
			break;
		}
	}
}
