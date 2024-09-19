using System;
using UnityEngine;

public class DisableOnReset : MonoBehaviour
{
	private void Awake()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void Reset()
	{
		if (base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
