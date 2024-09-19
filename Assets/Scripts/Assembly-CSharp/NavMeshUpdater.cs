using UnityEngine;
using UnityEngine.AI;

public class NavMeshUpdater : MonoBehaviour
{
	public static NavMeshUpdater instance;

	private NavMeshSurface surface;

	private void Awake()
	{
		instance = this;
		surface = GetComponent<NavMeshSurface>();
	}

	public void UpdateSurface()
	{
		surface.UpdateNavMesh(surface.navMeshData);
	}
}
