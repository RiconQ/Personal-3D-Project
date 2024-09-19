using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshLink))]
public class NavMeshLinkHandle : MonoBehaviour
{
	[HideInInspector]
	public NavMeshLink link;

	public void Set(Vector3 pos)
	{
		if (!link)
		{
			link = GetComponent<NavMeshLink>();
			link.startPoint = Vector3.zero;
		}
		link.endPoint = (pos - base.transform.position).Snap();
	}
}
