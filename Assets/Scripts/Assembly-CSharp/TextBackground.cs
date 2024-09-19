using UnityEngine;

public class TextBackground : MonoBehaviour
{
	public RectTransform t;

	public RectTransform tText;

	public Vector2 offset = new Vector2(8f, 0f);

	public Vector2 size { get; private set; }

	public void Setup()
	{
		t.sizeDelta = tText.sizeDelta + offset;
		size = t.sizeDelta;
	}
}
