using System;
using System.Collections.Generic;
using UnityEngine;

public class BreakablesControl : MonoBehaviour
{
	public static BreakablesControl instance;

	public static List<BaseBreakable> allBreakables = new List<BaseBreakable>(100);

	private Vector3 dir;

	private RaycastHit hit;

	private void Awake()
	{
		instance = this;
		Game.OnAnyLevelUnloaded = (Action)Delegate.Combine(Game.OnAnyLevelUnloaded, new Action(OnSceneUnloaded));
	}

	private void OnDestroy()
	{
		Game.OnAnyLevelUnloaded = (Action)Delegate.Remove(Game.OnAnyLevelUnloaded, new Action(OnSceneUnloaded));
	}

	private void OnSceneUnloaded()
	{
		if (allBreakables.Count > 0)
		{
			allBreakables.Clear();
		}
	}

	public void Add(BaseBreakable b)
	{
		if (!allBreakables.Contains(b))
		{
			allBreakables.Add(b);
		}
	}

	public bool GetClosest(float maxDist, float maxAngle, out BaseBreakable result)
	{
		result = null;
		float num = maxAngle;
		float num2 = 0f;
		foreach (BaseBreakable allBreakable in allBreakables)
		{
			Debug.DrawLine(Game.player.tHead.position, allBreakable.t.position, Color.red, 0.25f);
			if (!allBreakable.gameObject.activeInHierarchy || (allBreakable.rb.isKinematic && !allBreakable.CompareTag("Pullable")) || Vector3.Distance(allBreakable.clldr.bounds.center, Game.player.tHead.position) > maxDist)
			{
				continue;
			}
			dir = Game.player.tHead.position.DirTo(allBreakable.rb.worldCenterOfMass);
			num2 = Vector3.Angle(Game.player.tHead.forward, dir);
			if (!(num2 < num))
			{
				continue;
			}
			Physics.Raycast(Game.player.tHead.position, dir, out hit, maxDist, 548865);
			if (hit.distance != 0f && hit.collider.gameObject.layer != 14 && hit.collider.gameObject.layer != 19)
			{
				if (hit.distance != 0f)
				{
					Debug.DrawLine(Game.player.tHead.position, hit.point, Color.red, 0.25f);
				}
			}
			else
			{
				num = num2;
				result = allBreakable;
			}
		}
		if ((bool)result)
		{
			Debug.DrawLine(result.rb.worldCenterOfMass, Game.player.tHead.position, Color.green, 2f);
		}
		return result != null;
	}
}
