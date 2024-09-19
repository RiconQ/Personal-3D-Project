using UnityEngine;
using UnityEngine.EventSystems;

public class QuickmapButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public enum QuickmapButtonTypes
	{
		Other = 0,
		Quit = 1
	}

	public QuickmapButtonTypes type;

	public GameObject nextUI;

	private CanvasGroup cg;

	private bool active;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0.5f;
	}

	private void Accept()
	{
		switch (type)
		{
		case QuickmapButtonTypes.Quit:
			Quickmap.customMapName = "";
			Game.instance.LoadLevel("MainMenu");
			break;
		case QuickmapButtonTypes.Other:
			GetComponentInParent<QuickmapMainToolbarUI>().Deactivate();
			nextUI.SetActive(value: true);
			break;
		}
	}

	public void OnPointerClick(PointerEventData data)
	{
		Accept();
	}

	public void OnPointerEnter(PointerEventData data)
	{
		cg.alpha = 1f;
	}

	public void OnPointerExit(PointerEventData data)
	{
		cg.alpha = 0.5f;
	}
}
