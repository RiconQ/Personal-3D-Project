using UnityEngine;
using UnityEngine.UI;

public class LocalizeText : MonoBehaviour
{
	public int index = -1;

	private Text text;

	private void Awake()
	{
		text = GetComponentInChildren<Text>();
		if (index > -1)
		{
			text.text = Game.localization.Get(index);
		}
		else
		{
			Game.localization.Find(text);
		}
	}
}
