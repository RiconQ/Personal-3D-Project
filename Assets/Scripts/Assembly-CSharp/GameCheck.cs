using UnityEngine;

public class GameCheck : MonoBehaviour
{
	private void Awake()
	{
		if (!Game.instance)
		{
			Object.Instantiate(Resources.Load("Prefabs/Game"));
		}
		base.gameObject.AddComponent<CrowdControl>();
		base.gameObject.AddComponent<WeaponsControl>();
		base.gameObject.AddComponent<BreakablesControl>();
		base.gameObject.AddComponent<PullableControl>();
	}
}
