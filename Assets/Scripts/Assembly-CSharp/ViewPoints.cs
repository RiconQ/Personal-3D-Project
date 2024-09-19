using System.Collections.Generic;
using UnityEngine;

public class ViewPoints : MonoBehaviour
{
	public static ViewPoints instance;

	public List<Vector3> points = new List<Vector3>();

	public GameObject viewPointPrefab;

	public List<EnemyViewPoint> viewPoints = new List<EnemyViewPoint>();

	private RaycastHit hit;

	private Vector3 tempA;

	private Vector3 tempB;

	private void Awake()
	{
		instance = this;
		viewPointPrefab = Resources.Load("Prefabs/Enemy View Point") as GameObject;
		foreach (Vector3 point in points)
		{
			viewPoints.Add(Object.Instantiate(viewPointPrefab, point, Quaternion.identity).GetComponent<EnemyViewPoint>());
		}
	}

	private void FixedUpdate()
	{
		foreach (EnemyViewPoint viewPoint in viewPoints)
		{
			if (viewPoint.delay > 0f)
			{
				viewPoint.delay = Mathf.MoveTowards(viewPoint.delay, 0f, Time.deltaTime);
			}
			else if (viewPoint.occupied)
			{
				viewPoint.occupied = false;
			}
		}
	}

	public bool GetClosest(Vector3 fromPos, out Vector3 result)
	{
		result.x = (result.y = (result.z = 0f));
		float num = 16f;
		int num2 = -1;
		for (int i = 0; i < viewPoints.Count; i++)
		{
			if (viewPoints[i].occupied)
			{
				continue;
			}
			float num3 = viewPoints[i].t.position.y - fromPos.y;
			if (num3 < 3f || num3 > 12f)
			{
				continue;
			}
			tempB = viewPoints[i].t.position;
			tempB.y += 1f;
			tempA = fromPos;
			tempA.y = tempB.y;
			if (Vector3.Dot(fromPos.DirTo(Game.player.t.position), tempA.DirTo(tempB)) < 0f)
			{
				continue;
			}
			Physics.Linecast(tempA, tempB, out hit, 1);
			if (hit.distance == 0f)
			{
				float num4 = Vector3.Distance(fromPos, viewPoints[i].t.position);
				if (num4 < num)
				{
					num = num4;
					result = viewPoints[i].t.position;
					num2 = i;
				}
			}
		}
		if (num2 > -1)
		{
			viewPoints[num2].delay = 2f;
			viewPoints[num2].occupied = true;
		}
		return result.sqrMagnitude != 0f;
	}
}
