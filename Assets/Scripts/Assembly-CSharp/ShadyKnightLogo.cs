using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShadyKnightLogo : MonoBehaviour
{
	public RectTransform[] knightLetters;

	public HorizontalLayoutGroup hg;

	public CanvasGroup cgShady;

	public CanvasGroup cgKnight;

	public Transform tCam;

	public CanvasGroup cgAnimator;

	public ParticleSystem particle;

	[TextArea]
	public string[] texts;

	private int i;

	private void Awake()
	{
		StopLogo();
		StartCoroutine(Playing());
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			StopLogo();
			StartCoroutine(Playing());
		}
	}

	private void StopLogo()
	{
		StopAllCoroutines();
		cgShady.alpha = 0f;
		hg.spacing = -100f;
	}

	private IEnumerator Playing()
	{
		cgAnimator.alpha = 0f;
		yield return new WaitForSeconds(1f);
		tCam.position = new Vector3(0f, 0f, 0f);
		while (true)
		{
			tCam.rotation = Quaternion.Slerp(tCam.rotation, Quaternion.identity, Time.deltaTime * 4f);
			tCam.position = Vector3.Lerp(tCam.position, new Vector3(0f, 0f, -20f), Time.deltaTime * 2f);
			cgShady.alpha = Mathf.Lerp(cgShady.alpha, 1f, Time.deltaTime * 10f);
			hg.spacing = Mathf.Lerp(hg.spacing, -125f, Time.deltaTime * 2f);
			yield return null;
		}
	}
}
