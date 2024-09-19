using System.Collections;
using UnityEngine;

public class DemoEnd : MonoBehaviour
{
	public TextAnimator animatorThanks;

	public TextAnimator animatorSocial;

	public QuickMenu menu;

	private void Start()
	{
		StartCoroutine(Waiting());
	}

	private IEnumerator Waiting()
	{
		yield return new WaitForSeconds(1f);
		animatorThanks.Play();
		while (animatorThanks.isPlaying)
		{
			yield return null;
		}
		animatorSocial.Play();
		while (animatorSocial.isPlaying)
		{
			yield return null;
		}
		menu.Activate();
	}
}
