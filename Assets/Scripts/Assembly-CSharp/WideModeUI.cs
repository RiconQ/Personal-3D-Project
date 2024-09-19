using UnityEngine;

public class WideModeUI : MonoBehaviour
{
	private float speed = 4f;

	public CanvasGroup cg { get; private set; }

	public float alpha { get; private set; }

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
	}

	public void Set(float newAlpha)
	{
		if (newAlpha == -1f)
		{
			CanvasGroup canvasGroup = cg;
			float num2 = (alpha = ((cg.alpha != 1f) ? 1 : 0));
			canvasGroup.alpha = num2;
		}
		else
		{
			CanvasGroup canvasGroup2 = cg;
			float num2 = (alpha = newAlpha);
			canvasGroup2.alpha = num2;
		}
	}

	public void Show()
	{
		alpha = 1f;
	}

	public void Hide()
	{
		alpha = 0f;
	}

	public void Tick()
	{
		if (cg.alpha != alpha)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.unscaledDeltaTime * speed);
		}
	}
}
