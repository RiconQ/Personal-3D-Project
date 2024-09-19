using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ResultUI : MonoBehaviour
{
	public TextAnimator labelAnimator;

	public TextAnimator valueAnimator;

	public AudioClip sound;

	private CanvasGroup cg;

	private int state = -1;

	public bool isPlaying { get; private set; }

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
	}

	public void Play()
	{
		labelAnimator.text.color = Color.clear;
		valueAnimator.text.color = Color.clear;
		labelAnimator.ResetChars();
		valueAnimator.ResetChars();
		cg.alpha = 1f;
		state = 0;
		isPlaying = true;
	}

	public void Stop()
	{
		state = 3;
		cg.alpha = 1f;
		if (!valueAnimator.isPlaying)
		{
			valueAnimator.Play();
		}
		valueAnimator.StopAt();
		if (!labelAnimator.isPlaying)
		{
			labelAnimator.Play();
		}
		labelAnimator.StopAt();
	}

	private void Update()
	{
		if (state < 0)
		{
			return;
		}
		switch (state)
		{
		case 0:
			labelAnimator.Play();
			state++;
			break;
		case 1:
			if (!labelAnimator.isPlaying)
			{
				state++;
			}
			break;
		case 2:
			Game.sounds.PlayClip(sound);
			valueAnimator.Play();
			state++;
			break;
		case 3:
			if (!valueAnimator.isPlaying)
			{
				isPlaying = false;
				base.enabled = false;
			}
			break;
		}
	}
}
