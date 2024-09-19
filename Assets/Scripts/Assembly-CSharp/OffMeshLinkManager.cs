using System;
using UnityEngine;
using UnityEngine.AI;

public class OffMeshLinkManager : MonoBehaviour
{
	public static OffMeshLinkManager instance;

	public MegaCubeWorld world;

	public NavMeshSurface surface;

	private RaycastHit hit;

	private NavMeshHit navHitA;

	private NavMeshHit navHitB;

	private Vector3 posA;

	private Vector3 posB;

	private Vector3 temp;

	private void Awake()
	{
		instance = this;
		surface = GetComponent<NavMeshSurface>();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		surface.BuildNavMesh();
	}

	[Button]
	public void CreateLinks()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			if (componentsInChildren[num].parent == base.transform)
			{
				UnityEngine.Object.DestroyImmediate(componentsInChildren[num].gameObject);
			}
		}
		if (TryGetComponent<ViewPoints>(out var component))
		{
			foreach (Vector3 point in component.points)
			{
				posA = point;
				posA.y += 1f;
				temp = Vector3.forward;
				temp = Quaternion.Euler(0f, 45f, 0f) * temp;
				for (int i = 0; i < 3; i++)
				{
					posB = posA + temp * 8f;
					temp = Quaternion.Euler(0f, 120f, 0f) * temp;
					if (!Physics.Linecast(posA, posB, 1))
					{
						Physics.Raycast(posB, Vector3.down, out hit, 10f, 1);
						if (!(hit.distance < 2f) && NavMesh.SamplePosition(posA, out navHitA, 1f, -1) && NavMesh.SamplePosition(hit.point, out navHitB, 1f, -1))
						{
							CreateNewLink();
						}
					}
				}
			}
		}
		if (!world)
		{
			world = UnityEngine.Object.FindObjectOfType<MegaCubeWorld>();
		}
		if (!world)
		{
			return;
		}
		Vector3Int v = default(Vector3Int);
		foreach (MegaCubeRegion region in world.regions)
		{
			int num3 = (v.z = 0);
			int x = (v.y = num3);
			v.x = x;
			for (int j = 0; j < 8; j++)
			{
				for (int k = 0; k < 8; k++)
				{
					for (int l = 0; l < 8; l++)
					{
						v.x = region.origin.x * (world.side * 8) + j * world.side;
						v.y = region.origin.y * (world.side * 8) + (l + 1) * world.side;
						v.z = region.origin.z * (world.side * 8) + k * world.side;
						FindLinksFromPoint(v);
						world.RegionContains(v);
						_ = -1;
					}
				}
			}
		}
	}

	private void FindLinksFromPoint(Vector3Int temp)
	{
		Physics.Raycast(temp, Vector3.down, out hit, 5.5f, 1);
		if (hit.distance == 0f)
		{
			return;
		}
		posA = hit.point;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				posB = posA + CubeMesh.faceDirections[i] * (6 * (j + 1));
				if (!Physics.Linecast(posA + Vector3.up, posB + Vector3.up, 1))
				{
					Physics.Raycast(posB + Vector3.up, Vector3.down, out hit, 14f, 1);
					bool flag = hit.distance != 0f;
					if (hit.distance != 0f)
					{
						flag = ((j != 0) ? (hit.distance < 12f) : (hit.distance > 1f));
					}
					if ((hit.point.y - posA.y).Abs() < 1f && Physics.Raycast((hit.point + posA) / 2f + Vector3.up, Vector3.down, 2f, 1))
					{
						break;
					}
					if (hit.distance != 0f && flag && NavMesh.SamplePosition(posA + CubeMesh.faceDirections[i] * 2, out navHitA, 1f, -1) && NavMesh.SamplePosition(hit.point + CubeMesh.faceDirections[i] * 2, out navHitB, 1f, -1))
					{
						CreateNewLink();
						break;
					}
					if (hit.distance != 0f && hit.distance < 4f)
					{
						break;
					}
				}
			}
		}
	}

	private void CreateNewLink()
	{
		Transform transform = new GameObject("OffMeshLink").transform;
		Transform transform2 = new GameObject("Target").transform;
		transform.SetParent(base.transform);
		transform2.SetParent(transform);
		transform.position = navHitA.position;
		transform2.position = navHitB.position;
		OffMeshLink offMeshLink = transform.gameObject.AddComponent<OffMeshLink>();
		offMeshLink.startTransform = transform;
		offMeshLink.endTransform = transform2;
		offMeshLink.UpdatePositions();
		offMeshLink.area = 2;
	}
}
