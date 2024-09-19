using UnityEngine;
using UnityEngine.UI;

public class TowerMakerCursor : MonoBehaviour
{
	private RectTransform t;

	public Image rend;

	public Sprite point;

	public Sprite hold;

	private bool holding;

	private void Awake()
	{
		t = GetComponent<RectTransform>();
	}

	private void LateUpdate()
	{
		if (!holding)
		{
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				holding = true;
				rend.sprite = hold;
			}
		}
		else if (Cursor.lockState != CursorLockMode.Locked)
		{
			holding = false;
			rend.sprite = point;
		}
		if (!holding)
		{
			t.anchoredPosition = Input.mousePosition;
		}
		else
		{
			t.anchoredPosition = Vector2.Lerp(t.anchoredPosition, new Vector2(Screen.width / 2, Screen.height / 2), Time.deltaTime * 8f);
		}
	}
}
