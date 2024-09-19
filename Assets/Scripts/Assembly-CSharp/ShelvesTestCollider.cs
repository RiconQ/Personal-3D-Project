using System;
using UnityEngine;

public class ShelvesTestCollider : BaseBreakable
{
	public float delay;

	private bool isTriggered;

	private ITriggerable triggerable;

	private MeshRenderer rend;

	public AudioSource source;

	public override void Awake()
	{
		base.Awake();
		triggerable = GetComponentInParent<ITriggerable>();
		rend = GetComponent<MeshRenderer>();
		source = GetComponent<AudioSource>();
		base.rb.centerOfMass = base.t.InverseTransformPoint(rend.bounds.center);
		Debug.DrawRay(base.rb.worldCenterOfMass, Vector3.forward, Color.blue, 2f);
		Debug.DrawRay(rend.bounds.center, Vector3.forward * 0.5f, Color.cyan, 2f);
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		CancelInvoke();
		isTriggered = false;
		base.gameObject.SetActive(value: true);
	}

	public override void Kick(Vector3 dir)
	{
		base.Kick(dir);
		HandleInteraction();
	}

	public override void Damage(DamageData dmg)
	{
		base.Damage(dmg);
		HandleInteraction();
	}

	private void HandleInteraction()
	{
		if (!isTriggered)
		{
			if (delay != 0f)
			{
				source.Play();
				Invoke("Trigger", delay);
			}
			else
			{
				Trigger();
			}
			isTriggered = true;
		}
	}

	private void Trigger()
	{
		triggerable.Trigger();
		Break();
	}

	public void Break()
	{
		QuickEffectsPool.Get("Wooden Debris B", rend.bounds.center).Play(25f);
		base.gameObject.SetActive(value: false);
	}
}
