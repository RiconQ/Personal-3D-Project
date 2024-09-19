using UnityEngine;
using UnityEngine.UI;

public class StylePointCard : MonoBehaviour
{
	public StylePoint stylePoint;

	public Text text;

	public RectTransform tText;

	public TextBackground frame;

	public int count;

	public Image countableIcon;

	public Sprite[] countableSprites;

	public CanvasGroup cgIcon;

	private float lifetime;

	private Vector3 targetPosition;

	public RectTransform t { get; private set; }

	public CanvasGroup cg { get; private set; }

	public void Setup(StylePoint stylePoint, RectTransform tParent)
	{
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		tText = text.GetComponent<RectTransform>();
		frame = GetComponentInChildren<TextBackground>();
		t.SetParent(tParent);
		t.localScale = Vector3.one;
		this.stylePoint = stylePoint;
		base.name = stylePoint.name;
		text.text = stylePoint.GetPublicName();
	}

	public void Count()
	{
		if (lifetime < 1f)
		{
			count++;
			if (count - 1 >= 0)
			{
				countableIcon.sprite = countableSprites[count - 1];
				cgIcon.alpha = 1f;
			}
		}
	}

	public void ResetCount()
	{
		count = 0;
		cgIcon.alpha = 0f;
	}

	public void Refresh()
	{
		frame.t.anchoredPosition = tText.anchoredPosition;
		frame.Setup();
		lifetime = 0f;
	}

	public void SetupSize()
	{
		tText.anchoredPosition3D = (tText.sizeDelta / 2f).With(null, 0f);
		cg.alpha = 0f;
	}

	public void SetPos(Vector3 newPos, bool instant)
	{
		targetPosition = newPos;
		if (instant)
		{
			t.anchoredPosition3D = targetPosition;
		}
	}

	public void Tick(bool active)
	{
		lifetime += Time.deltaTime;
		if (cg.alpha != (float)(active ? 1 : 0))
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, active ? 1 : 0, Time.deltaTime);
		}
		if (t.anchoredPosition3D != targetPosition)
		{
			t.anchoredPosition3D = Vector3.Lerp(t.anchoredPosition3D, targetPosition, Time.unscaledDeltaTime * 8f);
		}
	}
}
