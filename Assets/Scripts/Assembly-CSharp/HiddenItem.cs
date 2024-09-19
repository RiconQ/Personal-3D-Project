using UnityEngine;

public class HiddenItem : MonoBehaviour
{
	public HubData hub;

	public bool value;

	private void Awake()
	{
		base.gameObject.SetActive((LevelsData.instance.GetHubState(hub) != 0) ? value : (!value));
	}
}
