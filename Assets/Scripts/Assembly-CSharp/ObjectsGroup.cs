using System;
using UnityEngine;

public class ObjectsGroup : MonoBehaviour
{
	public bool activeOnEnable;

	private void Awake()
	{
		if (base.gameObject.activeInHierarchy != activeOnEnable)
		{
			base.gameObject.SetActive(activeOnEnable);
		}
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		if (base.gameObject.activeInHierarchy != activeOnEnable)
		{
			base.gameObject.SetActive(activeOnEnable);
		}
	}
}
