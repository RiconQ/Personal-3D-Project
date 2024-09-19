using System;
using UnityEngine;

public class TextBlob : MonoBehaviour
{
	public static TextBlob instance;

	private RectTransform tShadow;

	private TextAnimator animator;

	[TextArea]
	public string[] testStrings;

	public Vector2 sizeOffset = new Vector2(16f, 0f);

	private int index;

	private float timer;

	private CanvasGroup cg;

	private CanvasGroup cgShadow;

	private void Awake()
	{
		instance = this;
		tShadow = base.transform.Find("Shadow").GetComponent<RectTransform>();
		cgShadow = tShadow.GetComponent<CanvasGroup>();
		animator = GetComponentInChildren<TextAnimator>();
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		timer = 0f;
		cg.alpha = 0f;
		if (animator.isPlaying)
		{
			animator.StopAt();
		}
	}

	public void Show(string message)
	{
		if (message.Length != 0)
		{
			cg.alpha = 1f;
			cgShadow.alpha = 0f;
			timer = 10f;
			animator.text.text = message;
			animator.ResetAndPlay();
			Invoke("RefreshShadowSize", Time.fixedDeltaTime);
		}
	}

	private void RefreshShadowSize()
	{
		tShadow.sizeDelta = animator.t.sizeDelta + sizeOffset;
	}

	private void Update()
	{
		if (timer != 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
			cg.alpha = Mathf.Clamp01(timer);
			if (cgShadow.alpha != 1f)
			{
				cgShadow.alpha = Mathf.MoveTowards(cgShadow.alpha, 1f, Time.deltaTime * 4f);
			}
		}
	}
}
