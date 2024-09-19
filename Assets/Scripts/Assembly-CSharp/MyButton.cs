using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MyButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Action OnClick = delegate
	{
	};

	public static MyButton last;

	public bool ToggleButton;

	public CanvasGroup cg;

	public bool toggled { get; private set; }

	protected virtual void Awake()
	{
		if (!cg)
		{
			cg = GetComponent<CanvasGroup>();
		}
		cg.alpha = 0.5f;
	}

	protected virtual void OnEnable()
	{
		cg.alpha = 0.5f;
	}

	public virtual void LeftClick()
	{
		if (OnClick != null)
		{
			OnClick();
		}
		if (!ToggleButton)
		{
			return;
		}
		toggled = !toggled;
		cg.alpha = (toggled ? 1f : 0.5f);
		if (toggled)
		{
			if (last != this)
			{
				if ((bool)last && last.toggled)
				{
					last.LeftClick();
				}
				last = this;
			}
		}
		else if (last == this)
		{
			last = null;
		}
	}

	public virtual void RightClick()
	{
	}

	public virtual void OnPointerClick(PointerEventData data)
	{
		if (data.button == PointerEventData.InputButton.Left)
		{
			LeftClick();
		}
		if (data.button == PointerEventData.InputButton.Right)
		{
			RightClick();
		}
	}

	public virtual void OnPointerEnter(PointerEventData data)
	{
		cg.alpha = 1f;
	}

	public virtual void OnPointerExit(PointerEventData data)
	{
		cg.alpha = (toggled ? 1f : 0.5f);
	}
}
