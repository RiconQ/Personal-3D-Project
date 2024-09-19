using UnityEngine;

public class PortalPoint : MonoBehaviour
{
	public int channel;

	private void Awake()
	{
		PortalsManager.instance.AddPoint(this);
	}
}
