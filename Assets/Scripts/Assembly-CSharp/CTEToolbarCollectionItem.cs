using UnityEngine;

public class CTEToolbarCollectionItem : MonoBehaviour
{
	public GameObject prefab;

	private RectTransform tBackground;

	private bool selected;

	private Vector3 pos;

	public void Setup()
	{
		tBackground = base.transform.Find("Background").GetComponent<RectTransform>();
	}

	public void Select()
	{
		if (!selected)
		{
			tBackground.localPosition += Vector3.left * 8f;
			selected = true;
		}
	}

	public void Deselect()
	{
		if (selected)
		{
			tBackground.localPosition += Vector3.right * 8f;
			selected = false;
		}
	}
}
