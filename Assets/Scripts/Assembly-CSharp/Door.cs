using System;
using UnityEngine;

public class Door : MonoBehaviour
{
	public Action OnOpening = delegate
	{
	};

	protected bool opened;

	public virtual void SetupDeathLock(int count)
	{
	}

	public virtual void UpdateDeathLock(int index)
	{
	}

	public virtual void Open()
	{
		if (!opened)
		{
			opened = true;
			if (OnOpening != null)
			{
				OnOpening();
			}
		}
	}
}
