using System;
using UnityEngine;

public class PullablePoint : MonoBehaviour, IDamageable<DamageData>
{
	public Action<PullablePoint> OnPulled = delegate
	{
	};

	public PullableTarget target;

	public bool horizontal;

	private Transform t;

	public void Awake()
	{
		t = base.transform;
		if (!PullableControl.pullables.Contains(t))
		{
			PullableControl.pullables.Add(t);
		}
	}

	public void OnDestroy()
	{
		if (PullableControl.pullables.Contains(t))
		{
			PullableControl.pullables.Remove(t);
		}
	}

	public void Damage(DamageData damage)
	{
		if (damage.newType != Game.player.weapons.daggerController.dmg_Pull)
		{
			return;
		}
		if (!target)
		{
			if (!horizontal)
			{
				Game.player.PullTo(t.position);
			}
			else
			{
				Game.player.PullInDir(t.position - Vector3.up);
			}
			return;
		}
		target.Pull();
		if (!Game.player.grounder.grounded)
		{
			Game.player.rb.velocity = Game.player.rb.velocity.With(null, 22f);
			Game.player.ParkourMove();
		}
	}
}
