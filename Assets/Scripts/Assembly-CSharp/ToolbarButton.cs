using UnityEngine;
using UnityEngine.EventSystems;

public class ToolbarButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public QuickmapCollectionUI collectionUI;

	public KeyCode hotkey;

	private QuickmapUI mainUI;

	public RectTransform t { get; private set; }

	private void Awake()
	{
		t = GetComponent<RectTransform>();
		mainUI = GetComponentInParent<QuickmapUI>();
		collectionUI.Setup();
		collectionUI.gameObject.SetActive(value: false);
	}

	public void Tick()
	{
		if (!collectionUI.gameObject.activeInHierarchy)
		{
			if (Input.GetKeyDown(hotkey))
			{
				Select();
			}
			return;
		}
		for (int i = 0; i < 10; i++)
		{
			if (Input.GetKeyDown((KeyCode)(49 + i)) && i < collectionUI.collection.prefabs.Length)
			{
				collectionUI.Set(i);
				break;
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!collectionUI.gameObject.activeInHierarchy)
		{
			Select();
		}
		else
		{
			collectionUI.gameObject.SetActive(value: false);
		}
	}

	public void Select()
	{
	}

	public void Unselect()
	{
		t.localScale = Vector3.one;
		collectionUI.gameObject.SetActive(value: false);
	}
}
