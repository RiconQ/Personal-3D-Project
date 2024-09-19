using System;
using UnityEngine;

public class LoadingIcon : MonoBehaviour
{
	public CanvasGroup cg;

	private float alpha;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		Loading.OnLoadingStart = (Action)Delegate.Combine(Loading.OnLoadingStart, new Action(Play));
		Loading.OnLoadingEnd = (Action)Delegate.Combine(Loading.OnLoadingEnd, new Action(Stop));
	}

	public void Tick()
	{
		cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.deltaTime * 4f);
	}

	public void Stop()
	{
		alpha = 0f;
	}

	public void Play()
	{
		alpha = 1f;
	}
}
