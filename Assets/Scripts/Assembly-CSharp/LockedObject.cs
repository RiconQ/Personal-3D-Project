using UnityEngine;

public class LockedObject : MonoBehaviour
{
	private void Start()
	{
		if (Hub.instance.progress != 1)
		{
			base.gameObject.SetActive(value: false);
		}
		Debug.Log(Hub.instance.progress);
	}
}
