using System;
using QuickmapEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PrefabCard : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public static Action<int> OnSelect = delegate
	{
	};

	private Vector2 pos;

	private Vector2 offset;

	private Vector3 temp;

	private float xSpeed;

	private float ySpeed;

	private int myIndex;

	private float alpha;

	public PrefabsButton button;

	public RectTransform t { get; private set; }

	public CanvasGroup cg { get; private set; }

	public TextBackground bg { get; private set; }

	public Text text { get; private set; }

	public Image image { get; private set; }

	public void OnPointerClick(PointerEventData eventData)
	{
		if (alpha != 0f)
		{
			if (OnSelect != null)
			{
				OnSelect(myIndex);
			}
			button.Select(this);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		offset = Vector2.right * 12f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		offset = Vector2.zero;
	}

	public void Setup(string name, int index)
	{
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		bg = GetComponent<TextBackground>();
		text = GetComponentInChildren<Text>();
		text.text = name;
		bg.Invoke("Setup", 0.1f);
		image = GetComponentInChildren<Image>();
		myIndex = index;
	}

	public void SetupPos(int i)
	{
		pos = t.anchoredPosition3D;
		pos.x = 0f - bg.size.x;
		xSpeed = 8f - (float)i / 2f;
	}

	public void SetAlpha(float a)
	{
		alpha = a;
	}

	public void SetupPos(int i, int count)
	{
		pos.x = 64f + bg.size.x / 2f;
		pos.y = -96 - i * 36;
		t.anchoredPosition3D = pos.With((0f - bg.size.x) / 2f);
		ySpeed = 4f;
		xSpeed = 8f - (float)i / 2f;
	}

	public void Tick()
	{
		temp = t.anchoredPosition3D;
		temp.x = Mathf.Lerp(t.anchoredPosition3D.x, pos.x + offset.x, Time.deltaTime * xSpeed);
		temp.y = Mathf.Lerp(t.anchoredPosition3D.y, pos.y + offset.y, Time.deltaTime * ySpeed);
		t.anchoredPosition3D = temp;
		if (cg.alpha != alpha)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.deltaTime * 6f);
		}
	}
}
