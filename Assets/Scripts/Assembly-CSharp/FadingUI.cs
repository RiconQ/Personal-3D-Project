using System.Collections;
using UnityEngine;

public class FadingUI : MonoBehaviour
{
	public AnimationCurve curveIn;

	public AnimationCurve curveOut;

	public float speed = 4f;

	public CanvasGroup cg { get; private set; }

	public Coroutine fading { get; private set; }

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 1f;
	}

	public void InstantFade(float alpha)
	{
		if (fading != null)
		{
			StopCoroutine(fading);
			fading = null;
		}
		if (cg.alpha != alpha)
		{
			cg.alpha = alpha;
		}
	}

	public void Fade(float alpha, float speed = 1.5f)
	{
		if (fading != null)
		{
			StopCoroutine(fading);
			fading = null;
		}
		this.speed = speed;
		fading = StartCoroutine(Fading(alpha));
	}

	private IEnumerator Fading(float alpha)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		float a = cg.alpha;
		float timer = 0f;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.unscaledDeltaTime * speed * (float)((!Game.paused) ? 1 : 2));
			cg.alpha = Mathf.LerpUnclamped(a, alpha, (alpha == 1f) ? curveOut.Evaluate(timer) : curveIn.Evaluate(timer));
			yield return null;
		}
		speed = 4f;
		fading = null;
	}
}
