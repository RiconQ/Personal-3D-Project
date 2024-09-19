using System;
using UnityEngine;

public class qmUI : MonoBehaviour
{
	public qmUI prevUI;

	public CanvasGroup cg;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public RectTransform t;

	private float timer;

	private Vector3 aPos;

	private Vector3 bPos;

	public bool isActive { get; private set; }

	protected virtual void Awake()
	{
		t = GetComponent<RectTransform>();
		if (!cg)
		{
			cg = GetComponent<CanvasGroup>();
		}
		InputsManager.OnAccept = (Action)Delegate.Combine(InputsManager.OnAccept, new Action(Accept));
		InputsManager.OnBack = (Action)Delegate.Combine(InputsManager.OnBack, new Action(Back));
	}

	protected virtual void OnDestroy()
	{
		InputsManager.OnAccept = (Action)Delegate.Remove(InputsManager.OnAccept, new Action(Accept));
		InputsManager.OnBack = (Action)Delegate.Remove(InputsManager.OnBack, new Action(Back));
	}

	protected virtual void Accept()
	{
	}

	protected virtual void Back()
	{
		if ((bool)prevUI)
		{
			prevUI.Activate();
			Deactivate();
		}
	}

	private void Update()
	{
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 4f);
			t.anchoredPosition3D = Vector3.LerpUnclamped(aPos, bPos, curve.Evaluate(timer));
			if ((bool)cg)
			{
				cg.alpha = timer;
			}
		}
	}

	protected virtual void OnEnable()
	{
		if ((bool)cg)
		{
			cg.alpha = 0f;
		}
		timer = 0f;
		aPos = (bPos = Vector3.zero);
		aPos.y -= 32f;
		t.anchoredPosition3D = aPos;
		Activate();
	}

	public virtual void Activate()
	{
		isActive = true;
		base.gameObject.SetActive(value: true);
	}

	public virtual void Deactivate()
	{
		if ((bool)cg)
		{
			cg.alpha = 0f;
		}
		isActive = false;
		base.gameObject.SetActive(value: false);
	}
}
