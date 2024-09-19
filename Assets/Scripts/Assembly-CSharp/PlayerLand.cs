using System.Collections.Generic;
using UnityEngine;

public class PlayerLand : MonoBehaviour
{
	public PlayerController p;

	private List<BaseEnemy> closestEnemies = new List<BaseEnemy>(10);

	private DamageData dmg = new DamageData();

	private int count;

	public DamageType dmg_Pound;

	public void Land(bool chainLanding)
	{
		p.sway.Sway(10f, 0f, 5f, 3f);
		closestEnemies.Clear();
		CrowdControl.instance.GetClosestEnemies(closestEnemies, p.tHead.position, p.tHead.forward.With(null, 0f).normalized, 10f, 60f);
		dmg.knockdown = true;
		dmg.amount = (chainLanding ? 40 : 20);
		dmg.newType = (chainLanding ? p.weapons.daggerController.dmg_Pound : dmg_Pound);
		count = 0;
		foreach (BaseEnemy closestEnemy in closestEnemies)
		{
			dmg.dir = (chainLanding ? (p.tHead.forward.With(null, 0f).normalized + Vector3.up) : (Vector3.up + p.t.position.DirTo(closestEnemy.GetActualPosition())).normalized);
			closestEnemy.Damage(dmg);
			count++;
			if (count >= 3)
			{
				break;
			}
		}
		if (count > 0)
		{
			Game.time.SlowMotion(0.1f, 0.3f, 0.1f);
			CameraController.shake.Shake(2);
		}
		if (chainLanding)
		{
			p.mouseLook.LookInDir(Vector3.ProjectOnPlane(p.tHead.forward, p.grounder.gNormal));
			QuickEffectsPool.Get("ChainLand", p.grounder.gPoint, Quaternion.LookRotation(p.tHead.forward)).Play();
			CameraController.shake.Shake(1);
		}
		else
		{
			CameraController.shake.Shake(1);
			QuickEffectsPool.Get("HardLanding", p.grounder.gPoint, Quaternion.LookRotation(p.tHead.forward.With(null, 0f))).Play();
		}
	}
}
