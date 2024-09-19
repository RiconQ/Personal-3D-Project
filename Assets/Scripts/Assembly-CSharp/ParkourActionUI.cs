using System;
using UnityEngine;

public class ParkourActionUI : MonoBehaviour
{
	public CanvasGroup cg;

	private float timer;

	private void Awake()
	{
		cg.alpha = 0f;
		PlayerController.OnParkourMove = (Action)Delegate.Combine(PlayerController.OnParkourMove, new Action(Blink));
	}

	private void OnDestroy()
	{
		PlayerController.OnParkourMove = (Action)Delegate.Remove(PlayerController.OnParkourMove, new Action(Blink));
	}

	private void Blink()
	{
		timer = 1f;
		cg.alpha = 0f;
	}

	private void Update()
	{
		if (timer != 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
		}
		cg.alpha = Mathf.Lerp(cg.alpha, timer, Time.deltaTime * 10f);
	}
}
