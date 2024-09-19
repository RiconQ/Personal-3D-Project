using System;
using UnityEngine;

public class Deactivator : MonoBehaviour
{
	public GameObject root;

	public float activeTime = 3f;

	private float timer;

	private void Awake()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		root.SetActive(value: false);
	}

	private void OnEnable()
	{
		timer = 0f;
	}

	private void Update()
	{
		if (timer < activeTime)
		{
			timer += Time.deltaTime;
		}
	}

	private void OnBecameInvisible()
	{
		if (timer >= activeTime)
		{
			root.SetActive(value: false);
		}
	}
}
