using UnityEngine;

public class HubLock : MonoBehaviour
{
	public HubPortal portal;

	private void Awake()
	{
		portal = GetComponent<HubPortal>();
	}

	private void Start()
	{
		Debug.Log("Hub progress is " + (float)Hub.instance.progress / (float)Hub.instance.levelsCount);
		portal.isLocked = Hub.instance.levelsCount != Hub.instance.progress;
	}
}
