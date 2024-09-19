using System;
using System.Collections;
using UnityEngine;

public class Crane : PullableTarget
{
	public GameObject point;

	protected override void Awake()
	{
		base.Awake();
		ThrowedDagger.OnHit = (Action<GameObject>)Delegate.Combine(ThrowedDagger.OnHit, new Action<GameObject>(Check));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ThrowedDagger.OnHit = (Action<GameObject>)Delegate.Remove(ThrowedDagger.OnHit, new Action<GameObject>(Check));
	}

	public void Check(GameObject obj)
	{
		if (!(obj != point))
		{
			StartCoroutine(Turning());
		}
	}

	public override void Pull()
	{
		base.Pull();
	}

	private IEnumerator Turning()
	{
		Game.player.SetKinematic(value: true);
		Game.player.mouseLook.enabled = false;
		_ = Game.player.t.position;
		Vector3 dir = Game.player.t.position - base.transform.position;
		float timer = 0f;
		while (timer != 1f)
		{
			dir = Quaternion.Euler(0f, 45f * Time.deltaTime, 0f) * dir;
			Game.player.t.position = base.transform.position + dir - Vector3.up * Mathf.Sin(timer * (float)Math.PI) * 1f;
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
			Game.player.mouseLook.LookInDirSmooth(Vector3.Cross(dir.With(null, 0f), -Vector3.up));
			Game.player.camController.Angle(-20f);
			yield return null;
		}
		Game.player.mouseLook.enabled = true;
		Game.player.SetKinematic(value: false);
	}
}
