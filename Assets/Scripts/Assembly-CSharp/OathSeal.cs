using System;
using System.Collections;
using UnityEngine;

public class OathSeal : PullableTarget
{
	private bool isSealed;

	public Transform t;

	private Vector3 aPos;

	private Vector3 bPos;

	private string line;

	private new void Awake()
	{
		t = base.transform;
		aPos = t.position;
		bPos = aPos;
		bPos.y -= 8f;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private new void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private new void Reset()
	{
		isSealed = false;
		t.position = aPos;
	}

	public override void Pull()
	{
		base.Pull();
		if (!isSealed)
		{
			StartCoroutine(Sealing());
		}
	}

	private void Update()
	{
		if (!isSealed)
		{
			t.position = aPos + new Vector3(0f, Mathf.Sin(Time.time + t.position.x + t.position.z) * 0.5f, 0f);
		}
	}

	private IEnumerator Sealing()
	{
		isSealed = true;
		yield return new WaitForSeconds(0.1f);
		float timer = 0f;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 4f);
			t.position = Vector3.Lerp(aPos, bPos, timer);
			yield return null;
		}
		QuickEffectsPool.Get("Oath Seal", t.position + Vector3.up).Play();
		CameraController.shake.Shake();
		yield return new WaitForSeconds(0.1f);
		if (OathScene.instance.GetNextLine(ref line))
		{
			TextBlob.instance.Show(line);
		}
	}
}
