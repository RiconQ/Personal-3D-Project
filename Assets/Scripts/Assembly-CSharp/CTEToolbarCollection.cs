using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Mask))]
public class CTEToolbarCollection : MonoBehaviour
{
	public RectTransform t { get; private set; }

	public CanvasGroup cg { get; private set; }

	public CTEToolbarCollectionItem[] items { get; private set; }

	public int index { get; private set; }

	private void Awake()
	{
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		items = GetComponentsInChildren<CTEToolbarCollectionItem>();
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Setup();
		}
		Refresh();
	}

	public void Refresh()
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (i == index)
			{
				items[i].Select();
			}
			else
			{
				items[i].Deselect();
			}
		}
	}

	public void Next()
	{
		index = index.Next(items.Length);
		Refresh();
	}
}
