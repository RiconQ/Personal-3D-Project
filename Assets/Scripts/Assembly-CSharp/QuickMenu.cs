using System;
using UnityEngine;
using UnityEngine.UI;

public class QuickMenu : QuickUI
{
	public Action OnActivated = delegate
	{
	};

	public Action OnItemChanged = delegate
	{
	};

	public bool dontResetIndex;

	public RectTransform tBackground;

	protected QuickMenuItem[] items = new QuickMenuItem[0];

	protected int index;

	[HideInInspector]
	public bool locked;

	public void ResetItems(int startIndex)
	{
		items = GetComponentsInChildren<QuickMenuItem>();
		index = startIndex;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Deselect();
		}
		items[startIndex].Select();
		index = startIndex;
		OnItemChange();
	}

	public override void Awake()
	{
		base.Awake();
		items = GetComponentsInChildren<QuickMenuItem>();
		if ((bool)tBackground)
		{
			tBackground.SetAsFirstSibling();
			tBackground.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
		}
		InputsManager.OnVerticalStep = (Action<int>)Delegate.Combine(InputsManager.OnVerticalStep, new Action<int>(Next));
		InputsManager.OnHorizontalStep = (Action<int>)Delegate.Combine(InputsManager.OnHorizontalStep, new Action<int>(ItemNext));
		InputsManager.OnAccept = (Action)Delegate.Combine(InputsManager.OnAccept, new Action(Accept));
	}

	public override void OnDisable()
	{
		InputsManager.OnVerticalStep = (Action<int>)Delegate.Remove(InputsManager.OnVerticalStep, new Action<int>(Next));
		InputsManager.OnHorizontalStep = (Action<int>)Delegate.Remove(InputsManager.OnHorizontalStep, new Action<int>(ItemNext));
		InputsManager.OnAccept = (Action)Delegate.Remove(InputsManager.OnAccept, new Action(Accept));
		base.OnDisable();
	}

	public override void Activate()
	{
		if (locked)
		{
			locked = false;
		}
		if (!dontResetIndex)
		{
			index = -1;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].isActiveAndEnabled)
			{
				if (index == -1)
				{
					index = i;
					items[i].Select();
				}
				else if (i == index)
				{
					items[i].Select();
				}
				else
				{
					items[i].Deselect();
				}
			}
		}
		UpdateFrame();
		if (OnActivated != null)
		{
			OnActivated();
		}
		base.Activate();
	}

	public virtual void Next(int sign = 1)
	{
		if (!base.active || locked || items.Length == 0)
		{
			return;
		}
		int num = index.NextClamped(items.Length, -sign);
		int num2 = items.Length;
		while (!items[num].isActiveAndEnabled && num2 > -1)
		{
			num -= sign;
			if (num >= items.Length)
			{
				num = 0;
			}
			if (num < 0)
			{
				num = items.Length - 1;
			}
			num2--;
		}
		if (index != num)
		{
			items[index].Deselect();
			items[num].Select();
			index = num;
			OnItemChange();
		}
	}

	public virtual void OnItemChange()
	{
		UpdateFrame();
		if ((bool)sounds)
		{
			Game.sounds.PlaySound(sounds.next, 2);
		}
		if (OnItemChanged != null)
		{
			OnItemChanged();
		}
	}

	public virtual void OnMenuItemChange()
	{
		UpdateFrame();
		if ((bool)sounds)
		{
			Game.sounds.PlaySound(sounds.nextItem, 2);
		}
	}

	public void UpdateFrame()
	{
		if ((bool)tBackground)
		{
			tBackground.anchoredPosition3D = items[index].GetPosition();
			tBackground.sizeDelta = items[index].GetSize() + new Vector2(36f, 18f);
		}
	}

	public virtual void ItemNext(int sign = 1)
	{
		if (base.active && !locked && items.Length != 0)
		{
			items[index].Next(sign);
		}
	}

	public virtual void Accept()
	{
		if (base.active && !locked && items[index].Accept() && (bool)sounds)
		{
			Game.sounds.PlaySound(sounds.accept, 2);
		}
	}

	public QuickMenuItem GetCurrentMenuItem()
	{
		return items[index];
	}
}
