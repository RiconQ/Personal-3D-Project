using System;
using UnityEngine;

public class CameraSway : MonoBehaviour
{
	public AnimationCurve curve;

	private Transform t;

	private Vector3 result;

	private int count = 10;

	private SwayEntry[] sways;

	private void Awake()
	{
		t = base.transform;
		sways = new SwayEntry[count];
		for (int i = 0; i < count; i++)
		{
			sways[i] = new SwayEntry();
		}
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
		for (int i = 0; i < count; i++)
		{
			sways[i].Reset();
		}
		result.x = (result.y = (result.z = 0f));
		t.localEulerAngles = result;
	}

	public void Sway(float x, float y, float z, float speed = 1f)
	{
		for (int i = 0; i < count; i++)
		{
			if (sways[i].time == 0f)
			{
				sways[i].Setup(x, y, z, speed);
				break;
			}
		}
	}

	private void Update()
	{
		result.x = 0f;
		result.y = 0f;
		result.z = 0f;
		for (int i = 0; i < count; i++)
		{
			if (sways[i].time != 0f)
			{
				result += sways[i].GetSway(curve);
			}
		}
		if (t.localEulerAngles != result)
		{
			t.localEulerAngles = result;
		}
	}
}
