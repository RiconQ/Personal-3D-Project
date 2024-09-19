using System;
using UnityEngine;

public class QuickMenuAlignWithSelected : MonoBehaviour
{
	private QuickMenu menu;

	private Vector3 posNew;

	private Vector3 posOld;

	private float timer;

	public float followScale = 0.5f;

	public float speed = 2f;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private void Awake()
	{
		menu = GetComponent<QuickMenu>();
		QuickMenu quickMenu = menu;
		quickMenu.OnActivated = (Action)Delegate.Combine(quickMenu.OnActivated, new Action(UpdateAndSetPos));
		QuickMenu quickMenu2 = menu;
		quickMenu2.OnItemChanged = (Action)Delegate.Combine(quickMenu2.OnItemChanged, new Action(UpdatePos));
	}

	private void OnDestroy()
	{
		QuickMenu quickMenu = menu;
		quickMenu.OnActivated = (Action)Delegate.Remove(quickMenu.OnActivated, new Action(UpdateAndSetPos));
		QuickMenu quickMenu2 = menu;
		quickMenu2.OnItemChanged = (Action)Delegate.Remove(quickMenu2.OnItemChanged, new Action(UpdatePos));
	}

	private void UpdateAndSetPos()
	{
		UpdatePos();
		menu.t.anchoredPosition3D = posNew;
	}

	private void UpdatePos()
	{
		posNew = (posOld = menu.t.anchoredPosition3D);
		posNew.y = (0f - menu.GetCurrentMenuItem().t.localPosition.y) * followScale;
		timer = 0f;
	}

	private void Update()
	{
		if (menu.active && menu.t.anchoredPosition3D != posNew)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.unscaledDeltaTime * speed);
			menu.t.anchoredPosition3D = Vector3.LerpUnclamped(posOld, posNew, curve.Evaluate(timer));
		}
	}
}
