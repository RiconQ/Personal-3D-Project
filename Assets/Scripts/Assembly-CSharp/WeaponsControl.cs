using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsControl : MonoBehaviour
{
	public static WeaponsControl instance;

	public static List<Transform> allWeapons = new List<Transform>(50);

	private RaycastHit hit;

	private float dist;

	private float maxDist = 8f;

	private float closestAngle;

	private float currentAngle;

	public int index = -1;

	private SimpleFlare flare;

	private bool targetUnreachable;

	private void Awake()
	{
		instance = this;
		flare = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Flare"), null, instantiateInWorldSpace: true) as GameObject).GetComponent<SimpleFlare>();
		Game.OnAnyLevelUnloaded = (Action)Delegate.Combine(Game.OnAnyLevelUnloaded, new Action(OnSceneUnloaded));
	}

	private void OnDestroy()
	{
		Game.OnAnyLevelUnloaded = (Action)Delegate.Remove(Game.OnAnyLevelUnloaded, new Action(OnSceneUnloaded));
	}

	private void OnSceneUnloaded()
	{
		if (allWeapons.Count > 0)
		{
			allWeapons.Clear();
		}
	}

	private void Update()
	{
		if (!Game.player)
		{
			return;
		}
		index = -1;
		maxDist = 11f;
		closestAngle = 35f;
		currentAngle = 0f;
		if (Game.player.dashPossible)
		{
			for (int i = 0; i < allWeapons.Count; i++)
			{
				if (!allWeapons[i].gameObject.activeInHierarchy)
				{
					continue;
				}
				dist = Vector3.Distance(Game.player.tHead.position, allWeapons[i].position);
				if (dist > 11f)
				{
					continue;
				}
				Physics.Raycast(Game.player.tHead.position, Game.player.tHead.position.DirTo(allWeapons[i].position), out hit, 11f, 24577);
				if ((hit.distance != 0f && hit.collider.gameObject.layer != 13) || !(dist < maxDist))
				{
					continue;
				}
				Vector3 to = Game.player.tHead.position.DirTo(allWeapons[i].position);
				currentAngle = Vector3.Angle(Game.player.tHead.forward, to);
				if (currentAngle < closestAngle)
				{
					closestAngle = currentAngle;
					if (index != i)
					{
						index = i;
					}
					targetUnreachable = (Game.player.tHead.position - allWeapons[i].position).y.Abs() > 2f;
				}
			}
		}
		if (index > -1)
		{
			if (!flare.gameObject.activeInHierarchy)
			{
				flare.gameObject.SetActive(value: true);
			}
			flare.Tick(allWeapons[index], targetUnreachable && Game.player.grounder.grounded);
		}
		else if (flare.gameObject.activeInHierarchy)
		{
			flare.gameObject.SetActive(value: false);
		}
	}

	public Transform FindClosestByDistance()
	{
		Transform result = null;
		float num = 3f;
		float num2 = 0f;
		foreach (Transform allWeapon in allWeapons)
		{
			num2 = Vector3.Distance(Game.player.t.position, allWeapon.position);
			if (num2 < num)
			{
				num = num2;
				result = allWeapon;
			}
		}
		return result;
	}

	public Transform GetClosest()
	{
		if (index <= -1)
		{
			return null;
		}
		return allWeapons[index];
	}

	public Transform GetClosestTarget2()
	{
		if (index == -1)
		{
			return null;
		}
		if (Game.player.grounder.grounded && targetUnreachable)
		{
			return null;
		}
		return allWeapons[index];
	}

	public Transform GetClosestTarget()
	{
		if (index == -1)
		{
			return null;
		}
		Vector3 direction = PlayerController.instance.tHead.position.DirTo(allWeapons[index].position);
		Physics.Raycast(PlayerController.instance.tHead.position, direction, out hit, 50f, 25601);
		if (hit.distance == 0f || hit.collider.gameObject.layer != 0)
		{
			return allWeapons[index];
		}
		return null;
	}

	public Transform GetClosestWeapon()
	{
		if (index == -1)
		{
			return null;
		}
		Vector3 direction = PlayerController.instance.tHead.position.DirTo(allWeapons[index].position);
		Physics.Raycast(PlayerController.instance.tHead.position, direction, out hit, 100f, 25601);
		int layer = hit.collider.gameObject.layer;
		if (layer == 10 || layer == 13)
		{
			return allWeapons[index];
		}
		return null;
	}

	public Transform ClosestByAngle(float maxDist = 3f)
	{
		Transform transform = null;
		float num = 90f;
		float num2 = 0f;
		foreach (Transform allWeapon in allWeapons)
		{
			if (!allWeapon.gameObject.activeInHierarchy)
			{
				continue;
			}
			Vector3 vector = Game.player.tHead.position.DirTo(allWeapon.position);
			Physics.Raycast(Game.player.tHead.position, vector, out hit, 6f, 24577);
			if (!(hit.distance > maxDist) && (hit.distance == 0f || hit.collider.gameObject.layer == 13))
			{
				num2 = Vector3.Angle(Game.player.tHead.forward, vector);
				if (num2 < num)
				{
					num = num2;
					transform = allWeapon;
				}
			}
		}
		if ((bool)transform)
		{
			Debug.DrawLine(Game.player.t.position, transform.transform.position, Color.red, 2f);
		}
		return transform;
	}
}
