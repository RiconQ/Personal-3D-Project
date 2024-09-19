using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DemoEndLinkButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public string url;

	public AudioClip sfxEnter;

	private RectTransform t;

	private Image image;

	private CanvasGroup cg;

	private Vector3 sizeA = Vector3.one;

	private Vector3 sizeB = new Vector3(1.05f, 1.05f, 1.05f);

	private void Awake()
	{
		t = GetComponent<RectTransform>();
		image = GetComponent<Image>();
		cg = GetComponent<CanvasGroup>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (cg.alpha > 0.5f)
		{
			Application.OpenURL(url);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		t.localScale = sizeB;
		image.color = new Color(1f, 0.9f, 0.9f);
		if ((bool)sfxEnter)
		{
			Game.sounds.PlayClip(sfxEnter);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		t.localScale = sizeA;
		image.color = Color.white;
	}
}
