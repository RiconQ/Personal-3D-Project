using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Mask))]
public class QuickMenuOptionItem : QuickMenuItem
{
	private QuickMenu menu;

	private RectTransform tValue;

	private Text myText;

	protected List<string> values = new List<string>();

	protected int index;

	protected int width = 256;

	protected int offset = 8;

	private List<RectTransform> tValues = new List<RectTransform>();

	public override void Awake()
	{
		base.Awake();
		if (values.Count == 0)
		{
			return;
		}
		menu = GetComponentInParent<QuickMenu>();
		myText = GetComponentInChildren<Text>();
		GetComponent<HorizontalLayoutGroup>().padding.right = 196;
		GameObject gameObject = new GameObject("Value");
		gameObject.transform.SetParent(myText.rectTransform);
		gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
		gameObject.AddComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
		tValue = gameObject.GetComponent<RectTransform>();
		tValue.anchorMin = new Vector2(1f, 0.5f);
		tValue.anchorMax = new Vector2(1f, 0.5f);
		for (int num = values.Count - 1; num >= 0; num--)
		{
			GameObject gameObject2 = new GameObject("Text");
			gameObject2.transform.SetParent(tValue);
			Text text = gameObject2.AddComponent<Text>();
			text.color = myText.color;
			text.text = values[num];
			text.font = myText.font;
			text.fontSize = myText.fontSize;
			if ((bool)myText.material)
			{
				text.material = myText.material;
			}
			tValues.Add(gameObject2.GetComponent<RectTransform>());
		}
		StartCoroutine(Waiting());
		MenuItem_Resolution.OnResolutionChange = (Action)Delegate.Combine(MenuItem_Resolution.OnResolutionChange, new Action(RefreshSizeAndAlign));
	}

	public override Vector2 GetSize()
	{
		Vector2 sizeDelta = myText.rectTransform.sizeDelta;
		sizeDelta.x += (float)offset + tValues[values.Count - 1 - index].sizeDelta.x;
		return sizeDelta;
	}

	public override Vector2 GetPosition()
	{
		Vector2 result = default(Vector2);
		result.x = GetSize().x / 2f;
		result.y = base.t.anchoredPosition3D.y;
		return result;
	}

	private void OnDestroy()
	{
		MenuItem_Resolution.OnResolutionChange = (Action)Delegate.Remove(MenuItem_Resolution.OnResolutionChange, new Action(RefreshSizeAndAlign));
	}

	private void RefreshSizeAndAlign()
	{
		StartCoroutine(Waiting());
	}

	private IEnumerator Waiting()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		float alpha = base.cg.alpha;
		base.cg.alpha = 0f;
		tValue.localScale = Vector3.one;
		tValue.anchoredPosition3D = new Vector3(width / 2 + offset, 0f, 0f);
		tValue.sizeDelta = new Vector2(width, base.t.rect.height * (float)values.Count);
		yield return new WaitForEndOfFrame();
		AlignPosition();
		base.cg.alpha = alpha;
	}

	public override void Refresh()
	{
		base.Refresh();
		AlignPosition();
	}

	public override void Next(int sign)
	{
		base.Next();
		int num = index.NextClamped(values.Count, sign);
		if (index != num)
		{
			index = num;
			AlignPosition();
			menu.OnMenuItemChange();
		}
	}

	private void AlignPosition()
	{
		float y = tValues[index].localPosition.y;
		tValue.anchoredPosition3D = new Vector3(width / 2 + offset, y, 0f);
	}

	private void SizeCorrection()
	{
		tValue.sizeDelta = new Vector2(width, base.t.rect.height * (float)values.Count);
	}
}
