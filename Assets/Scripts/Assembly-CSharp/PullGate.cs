using System.Collections;
using UnityEngine;

public class PullGate : PullableTarget
{
	public Transform t;

	public ParticleSystem particle;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private bool isOpened;

	private Vector3 aPos;

	private Vector3 bPos;

	protected override void Awake()
	{
		base.Awake();
		aPos = t.position;
		bPos = t.position;
		bPos.y += 1.5f;
	}

	protected override void Reset()
	{
		base.Reset();
		StopAllCoroutines();
		t.position = aPos;
		particle.Clear();
		isOpened = false;
	}

	public override void Pull()
	{
		base.Pull();
		if (!isOpened)
		{
			isOpened = true;
			StartCoroutine(Opening());
		}
	}

	private IEnumerator Opening()
	{
		float timer = 0f;
		particle.Play();
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
			t.position = Vector3.LerpUnclamped(aPos, bPos, curve.Evaluate(timer));
			yield return null;
		}
	}
}
