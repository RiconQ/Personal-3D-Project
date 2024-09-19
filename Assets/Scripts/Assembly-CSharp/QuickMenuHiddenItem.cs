using UnityEngine;

[RequireComponent(typeof(QuickMenu))]
public class QuickMenuHiddenItem : MonoBehaviour
{
	public GameObject objItem;

	public int index;

	private QuickMenu menu;

	private void Awake()
	{
		menu = GetComponent<QuickMenu>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.BackQuote) && menu.active && !objItem.activeInHierarchy)
		{
			objItem.SetActive(value: true);
			menu.ResetItems(index);
		}
	}
}
