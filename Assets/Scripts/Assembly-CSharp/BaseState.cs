using System;
using UnityEngine;

public abstract class BaseState
{
	protected GameObject gameObject;

	protected Transform transform;

	public abstract void FirstCall();

	public abstract Type Tick();

	public abstract void LastCall();

	public abstract void ExternalCall();

	public BaseState(GameObject gameObject)
	{
		this.gameObject = gameObject;
		transform = gameObject.transform;
	}
}
