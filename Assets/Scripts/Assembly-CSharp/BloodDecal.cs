using System;
using UnityEngine;

public class BloodDecal : PooledMonobehaviour
{
	public Projector proj;

	private float timer;

	protected override void Awake()
	{
		base.Awake();
		proj.material = new Material(proj.material);
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		timer = 1f;
	}

	private void Update()
	{
		if (timer != 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime * 0.25f);
			proj.material.SetFloat("_Power", timer);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
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
