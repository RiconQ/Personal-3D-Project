using System;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
	public DamageData dmg;

	private bool triggered;

	private void Awake()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		triggered = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (triggered || !Physics.Raycast(base.transform.position, base.transform.position.DirTo(other.transform.position), out var hitInfo, 100f, 17920))
		{
			return;
		}
		triggered = true;
		if (hitInfo.collider.gameObject.layer == 9)
		{
			if (hitInfo.collider.GetComponent<IDamageable<Vector4>>() != null)
			{
				hitInfo.collider.GetComponent<IDamageable<Vector4>>().Damage(Vector3Extensions.With(base.transform.forward, null, null, null, 115f));
			}
		}
		else
		{
			dmg.dir = base.transform.forward;
			hitInfo.collider.GetComponent<IDamageable<DamageData>>().Damage(dmg);
		}
	}
}
