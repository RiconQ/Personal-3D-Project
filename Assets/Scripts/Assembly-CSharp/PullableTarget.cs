using System;
using UnityEngine;

public class PullableTarget : MonoBehaviour
{
	protected virtual void Awake()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	protected virtual void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	protected virtual void Reset()
	{
	}

	public virtual void Pull()
	{
	}
}
