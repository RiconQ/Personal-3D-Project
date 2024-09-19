using System;
using UnityEngine;

public class ArrowScattered : PooledMonobehaviour
{
	private RaycastHit hit;

	private DamageData dmg = new DamageData();

	private MegaLineRenderer line;

	protected override void Awake()
	{
		base.Awake();
		line = GetComponentInChildren<MegaLineRenderer>();
		line.Setup();
		dmg.amount = 20f;
		dmg.knockdown = true;
		MegaLineRenderer megaLineRenderer = line;
		megaLineRenderer.OnStopped = (Action)Delegate.Combine(megaLineRenderer.OnStopped, new Action(Deactivate));
	}

	private void OnDestroy()
	{
		MegaLineRenderer megaLineRenderer = line;
		megaLineRenderer.OnStopped = (Action)Delegate.Remove(megaLineRenderer.OnStopped, new Action(Deactivate));
	}

	private void Deactivate()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		Fire();
	}

	public void Fire()
	{
		Physics.Raycast(base.t.position, base.t.forward, out hit, 20f, 17409);
		if (hit.distance != 0f)
		{
			line.SetPointsAndPlay(base.transform.position, hit.point, Color.white);
			if (hit.collider.gameObject.layer != 0)
			{
				dmg.dir = base.t.forward;
				hit.collider.GetComponent<IDamageable<DamageData>>().Damage(dmg);
			}
			QuickEffectsPool.Get("Arrow Hit", hit.point, Quaternion.LookRotation(hit.normal)).Play(-1f, 5);
		}
		else
		{
			line.SetPointsAndPlay(base.t.position, base.t.position + base.t.forward * UnityEngine.Random.Range(5f, 10f), Color.white);
		}
	}
}
