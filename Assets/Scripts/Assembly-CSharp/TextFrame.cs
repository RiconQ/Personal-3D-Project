using UnityEngine;

public class TextFrame : MonoBehaviour
{
	public RectTransform t;

	public RectTransform tTarget;

	public Vector2 offset = new Vector2(64f, 32f);

	public Vector2 temp;

	private void LateUpdate()
	{
		t.position = tTarget.position;
		t.sizeDelta = Vector2.Lerp(t.sizeDelta, tTarget.rect.size + offset, Time.deltaTime * 2f);
	}
}
