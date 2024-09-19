using System;
using System.Collections;
using UnityEngine;

[SelectionBase]
public class Drawbridge : MonoBehaviour
{
	public Transform tBridge;

	public AnimationCurve curve;

	public PullablePoint pullPoint;

	private Vector3 angles;

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
		tBridge.localEulerAngles = Vector3.zero;
		pullPoint.gameObject.SetActive(value: true);
	}

	public void Pull()
	{
		StartCoroutine(Opening());
		pullPoint.gameObject.SetActive(value: false);
	}

	private IEnumerator Opening()
	{
		float timer = 0f;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 2f);
			angles.y = Mathf.LerpUnclamped(0f, -90f, curve.Evaluate(timer));
			tBridge.localEulerAngles = angles;
			yield return null;
		}
	}
}
