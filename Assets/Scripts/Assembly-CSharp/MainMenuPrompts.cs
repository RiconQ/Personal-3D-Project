using UnityEngine;

public class MainMenuPrompts : MonoBehaviour
{
	public CanvasGroup cg;

	private void Awake()
	{
		cg.alpha = 0f;
	}

	private void Update()
	{
		if (cg.alpha != 1f)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, 1f, Time.deltaTime);
		}
	}

	private void OnDisable()
	{
		cg.alpha = 0f;
	}
}
