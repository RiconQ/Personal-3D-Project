using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class QuickMenuItem : MonoBehaviour
{
	public Action OnSelect = delegate
	{
	};

	public Action OnDeselect = delegate
	{
	};

	private Color colorSelected = new Color(1f, 0.86f, 0.65f, 1f);

	private Color color = new Color(0.9f, 0.9f, 0.9f, 1f);

	public CanvasGroup cg { get; private set; }

	public RectTransform t { get; private set; }

	public Text txt { get; private set; }

	public virtual void Awake()
	{
		cg = GetComponent<CanvasGroup>();
		t = GetComponent<RectTransform>();
		txt = GetComponentInChildren<Text>();
	}

	public virtual Vector2 GetSize()
	{
		return t.sizeDelta;
	}

	public virtual Vector2 GetPosition()
	{
		return t.anchoredPosition3D;
	}

	public virtual void Select()
	{
		if ((bool)cg)
		{
			txt.color = colorSelected;
			cg.alpha = 1f;
			if (OnSelect != null)
			{
				OnSelect();
			}
		}
	}

	public virtual void Deselect()
	{
		if ((bool)cg)
		{
			txt.color = color;
			cg.alpha = 0.5f;
			if (OnDeselect != null)
			{
				OnDeselect();
			}
		}
	}

	public virtual void Refresh()
	{
	}

	public virtual bool Accept()
	{
		return false;
	}

	public virtual void Next(int sign = 1)
	{
	}
}
