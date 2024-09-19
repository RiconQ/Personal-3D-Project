using UnityEngine;
using UnityEngine.UI;

public class HeaderUI : MonoBehaviour
{
	private CanvasGroup cg;

	private Text text;

	private TextAnimator animator;

	private float timer;

	private void Awake()
	{
		animator = GetComponentInChildren<TextAnimator>();
		animator.ResetChars();
		text = GetComponentInChildren<Text>();
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
	}

	public void Show(string newText)
	{
		text.text = newText;
		animator.StopAt();
		animator.ResetAndPlay();
		cg.alpha = 1f;
		timer = 2f;
	}

	public void Reset()
	{
		animator.StopAt();
		cg.alpha = 0f;
		timer = 0f;
	}

	private void Update()
	{
		if (timer != 0f && !animator.isPlaying)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);
			cg.alpha = Mathf.Clamp01(timer);
		}
	}
}
