using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour
{
	private CanvasGroup canvasGroup;

	private Text text;

	private TextBackground bg;

	private Coroutine fading;

	public void Setup()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		text = GetComponentInChildren<Text>();
		bg = GetComponentInChildren<TextBackground>();
		fading = null;
	}

	public void Show(string message)
	{
		text.text = message;
		Fade(1f);
	}

	public void Hide()
	{
		Fade();
	}

	private void Fade(float alpha = 0f, float speed = 4f)
	{
		if (fading != null)
		{
			StopCoroutine(fading);
		}
		fading = StartCoroutine(Fading(alpha, speed));
	}

	private IEnumerator Fading(float alpha = 0f, float speed = 4f)
	{
		yield return new WaitForEndOfFrame();
		bg.Setup();
		yield return new WaitForEndOfFrame();
		while (canvasGroup.alpha != alpha)
		{
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, alpha, Time.unscaledDeltaTime * speed);
			yield return null;
		}
		fading = null;
	}
}
