using System;
using UnityEngine;

public class InputsManager : MonoBehaviour
{
	public static Action<int> OnVerticalStep = delegate
	{
	};

	public static Action<int> OnHorizontalStep = delegate
	{
	};

	public static Action OnAccept = delegate
	{
	};

	public static Action OnBack = delegate
	{
	};

	[SerializeField]
	private string hName = "Horizontal";

	[SerializeField]
	private string vName = "Vertical";

	[SerializeField]
	private string acceptName = "Fire1";

	[SerializeField]
	private string backName = "Cancel";

	[SerializeField]
	private string triggersName = "Triggers";

	private float h;

	private float v;

	private float hTime;

	private float vTime;

	private float delay = 0.25f;

	private float triggersDeadzone = 0.25f;

	private float triggers;

	public static bool rTriggerPressed { get; private set; }

	public static bool rTriggerHolded { get; private set; }

	public static bool lTriggerPressed { get; private set; }

	public static bool lTriggerHolded { get; private set; }

	private void Update()
	{
		triggers = Input.GetAxis(triggersName);
		if (!lTriggerHolded && triggers > triggersDeadzone)
		{
			lTriggerPressed = true;
			lTriggerHolded = true;
		}
		if (!rTriggerHolded && triggers < 0f - triggersDeadzone)
		{
			rTriggerPressed = true;
			rTriggerHolded = true;
		}
		h = 0f;
		v = 0f;
		if (h == 0f)
		{
			h = Input.GetAxisRaw(hName);
		}
		if (v == 0f)
		{
			v = Input.GetAxisRaw(vName);
		}
		if (h.Abs() > 0.5f)
		{
			if (hTime <= 0f)
			{
				if (OnHorizontalStep != null)
				{
					OnHorizontalStep(h.Sign());
				}
				hTime += delay;
			}
			else
			{
				hTime -= Time.unscaledDeltaTime;
			}
		}
		else if (hTime != 0f)
		{
			hTime = 0f;
		}
		if (v.Abs() > 0.5f)
		{
			if (vTime <= 0f)
			{
				if (OnVerticalStep != null)
				{
					OnVerticalStep(v.Sign());
				}
				vTime += delay;
			}
			else
			{
				vTime -= Time.unscaledDeltaTime;
			}
		}
		else if (vTime != 0f)
		{
			vTime = 0f;
		}
	}

	private void LateUpdate()
	{
		triggers = Input.GetAxis(triggersName);
		lTriggerPressed = false;
		rTriggerPressed = false;
		if (triggers <= triggersDeadzone)
		{
			lTriggerHolded = false;
		}
		if (triggers >= 0f - triggersDeadzone)
		{
			rTriggerHolded = false;
		}
		if (Input.GetButtonDown(acceptName) && OnAccept != null)
		{
			OnAccept();
		}
		if (Input.GetButtonDown(backName) && OnBack != null)
		{
			OnBack();
		}
	}
}
