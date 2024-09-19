using System;
using UnityEngine;

public class PullableObjectSpawner : PullableTarget
{
	public GameObject prefab;

	private GameObject myObject;

	private new void Awake()
	{
		myObject = UnityEngine.Object.Instantiate(prefab, base.transform.position + base.transform.forward * 2f, base.transform.rotation);
		myObject.SetActive(value: false);
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private new void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private new void Reset()
	{
		myObject.SetActive(value: false);
	}

	public override void Pull()
	{
		base.Pull();
		if (!myObject.activeInHierarchy)
		{
			myObject.transform.SetPositionAndRotation(base.transform.position + base.transform.forward * 2f, base.transform.rotation);
			myObject.SetActive(value: true);
			myObject.GetComponent<Rigidbody>().velocity = (base.transform.forward + Vector3.up).normalized * 10f;
		}
	}
}
