using UnityEngine;

public class Kim_PauseMenu : MonoBehaviour
{
	private CanvasGroup cg;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
	}

	public void Show()
	{
		cg.alpha = 1f;
		cg.interactable = true;
		cg.blocksRaycasts = true;
	}

	public void Hide()
	{
		cg.alpha = 0f;
		cg.interactable = false;
		cg.blocksRaycasts = false;
	}
}
