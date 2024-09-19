using System;
using UnityEngine;

public class CameraBob : MonoBehaviour
{
	private Transform t;

	private Transform tParent;

	private Vector3 bob;

	public float bobSpeed = 1f;

	public float mgtSpeed = 2f;

	private float mgt;

	private float bobingTime;

	[SerializeField]
	private float xAmp = 0.02f;

	[SerializeField]
	private float yAmp = 0.06f;

	private void Awake()
	{
		t = base.transform;
		tParent = t.parent;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void Reset()
	{
		bob.x = (bob.y = (mgt = (bobingTime = 0f)));
		t.localPosition = bob;
	}

	public void Bob(float speed = 1f, float targetMgt = 1f)
	{
		bobingTime = Mathf.MoveTowards(bobingTime, (float)Math.PI * 2f, Time.deltaTime * (1f / speed) * bobSpeed);
		if (bobingTime == (float)Math.PI * 2f)
		{
			bobingTime = 0f;
		}
		if (mgt != targetMgt)
		{
			mgt = Mathf.MoveTowards(mgt, targetMgt, Time.deltaTime * mgtSpeed);
		}
		bob.x = Mathf.Lerp(0f, Mathf.Sin(bobingTime * 16f) * xAmp * mgt, mgt);
		bob.y = Mathf.Lerp(0f, Mathf.Sin(bobingTime * 16f).Abs() * yAmp * mgt, mgt);
		bob *= (Game.player.weapons.IsAttacking() ? 0.25f : 1f);
		t.localPosition = bob;
	}
}
