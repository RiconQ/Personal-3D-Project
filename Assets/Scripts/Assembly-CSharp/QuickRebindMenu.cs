using System;
using UnityEngine;

public class QuickRebindMenu : QuickMenu
{
	public static Action OnRebinded = delegate
	{
	};

	public GameObject itemPrefab;

	public KeyboardInputs inputs;

	[HideInInspector]
	public int[] keycodes;

	[HideInInspector]
	public MenuItem_KeyBinding[] keys;

	private int width = 254;

	private int columnCount = 4;

	private RectTransform tLast;

	public override void Awake()
	{
		base.Awake();
		keycodes = (int[])Enum.GetValues(typeof(KeyCode));
		keys = new MenuItem_KeyBinding[inputs.playerKeys.Length];
		int i;
		int num;
		for (i = 0; i < keys.Length; i++)
		{
			num = Mathf.FloorToInt((float)i / (float)columnCount);
			keys[i] = UnityEngine.Object.Instantiate(itemPrefab, base.t).GetComponent<MenuItem_KeyBinding>();
			keys[i].Setup(inputs.playerKeys[i].name, inputs.playerKeys[i].key);
			keys[i].GetComponent<RectTransform>().anchoredPosition3D = new Vector3(width / 2 + num * width, 32 * columnCount / 2 - 16 + (i - num * columnCount) * -32, 0f);
			keys[i].GetComponent<RectTransform>().SetSiblingIndex(i);
		}
		items = GetComponentsInChildren<QuickMenuItem>();
		tLast = items[items.Length - 1].GetComponent<RectTransform>();
		i = items.Length - 1;
		num = Mathf.FloorToInt((float)i / (float)columnCount);
		tLast.anchoredPosition3D = new Vector3(width / 2 + num * width, 32 * columnCount / 2 - 16 + (i - num * columnCount) * -32, 0f);
	}

	public override void Back()
	{
		if (!locked)
		{
			base.Back();
		}
	}

	public override void ItemNext(int sign = 1)
	{
		base.Next(-sign);
	}

	public override void Next(int sign = 1)
	{
		base.Next(sign * columnCount);
	}

	public virtual int GetIndex()
	{
		return index;
	}

	public virtual KeyCode GetCurrentKey()
	{
		return inputs.playerKeys[index].key;
	}

	public virtual void ChangeKey(KeyCode key)
	{
		inputs.playerKeys[index].key = key;
		if (OnRebinded != null)
		{
			OnRebinded();
		}
	}

	public virtual void SwitchKey(KeyCode key, int i)
	{
		inputs.playerKeys[i].key = inputs.playerKeys[index].key;
		keys[i].Setup(inputs.playerKeys[i].name, inputs.playerKeys[index].key);
		inputs.playerKeys[index].key = key;
		if (OnRebinded != null)
		{
			OnRebinded();
		}
	}

	public virtual void Reset()
	{
		inputs.Reset();
		for (int i = 0; i < inputs.playerKeys.Length; i++)
		{
			keys[i].Setup(inputs.playerKeys[i].name, inputs.playerKeys[i].key);
			items[i].Refresh();
		}
		if (OnRebinded != null)
		{
			OnRebinded();
		}
	}
}
