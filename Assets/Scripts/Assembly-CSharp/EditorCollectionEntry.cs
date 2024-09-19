using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorCollectionEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public int myIndex;

	public bool selected;

	private QuickmapUI main;

	private CanvasGroup cg;

	public RectTransform t { get; private set; }

	public Text text { get; private set; }

	public void Setup(int i, string name, QuickmapUI ui)
	{
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		text = GetComponentInChildren<Text>();
		main = ui;
		text.text = name;
		myIndex = i;
	}

	public void Select(bool value)
	{
		selected = value;
		cg.alpha = (selected ? 1f : 0.5f);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		main.Set(myIndex);
	}

	public void OnPointerEnter(PointerEventData d)
	{
		cg.alpha = 1f;
	}

	public void OnPointerExit(PointerEventData d)
	{
		cg.alpha = (selected ? 1f : 0.5f);
	}
}
