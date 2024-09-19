using System.Collections;
using UnityEngine;

public class DemoEndB : MonoBehaviour
{
	public GameObject stuff;

	private TextAnimator[] animators;

	private CanvasGroup[] cgs;

	private bool wait;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		cgs = stuff.GetComponentsInChildren<CanvasGroup>();
		for (int i = 0; i < cgs.Length; i++)
		{
			cgs[i].alpha = 0f;
		}
		animators = stuff.GetComponentsInChildren<TextAnimator>();
		GetComponent<PerlinShake>().Shake();
		StartCoroutine(Ending());
	}

	private IEnumerator Ending()
	{
		wait = true;
		yield return new WaitForSeconds(0.3f);
		animators[0].ResetAndPlay();
		yield return new WaitForEndOfFrame();
		cgs[0].alpha = 1f;
		while (animators[0].isPlaying)
		{
			yield return null;
		}
		int i = 0;
		while (i < cgs.Length)
		{
			if (i == cgs.Length - 1)
			{
				wait = false;
			}
			while (cgs[i].alpha != 1f)
			{
				cgs[i].alpha = Mathf.MoveTowards(cgs[i].alpha, 1f, Time.deltaTime * 1.5f);
				yield return null;
			}
			int num = i + 1;
			i = num;
		}
	}

	private void OnDestroy()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		if (!wait && Input.GetKeyDown(KeyCode.Escape))
		{
			Game.instance.LoadLevel("Hub4");
		}
	}
}
