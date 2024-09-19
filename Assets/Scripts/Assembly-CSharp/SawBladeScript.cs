using System;
using UnityEngine;

public class SawBladeScript : PooledMonobehaviour
{
	private DamageData dmg = new DamageData();

	private Transform tMesh;

	private void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.layer != 9)
		{
			dmg.dir = base.t.up;
			dmg.amount = 200f;
			c.GetComponent<IDamageable<DamageData>>().Damage(dmg);
			if (c.gameObject.layer == 10)
			{
				StyleRanking.instance.AddStylePoint(StylePointTypes.SpinningBlade);
			}
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		tMesh.Rotate(1440f * Time.deltaTime, 0f, 0f, Space.Self);
	}

	protected override void Awake()
	{
		base.Awake();
		tMesh = base.t.Find("Mesh").transform;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		base.gameObject.SetActive(value: false);
	}
}
