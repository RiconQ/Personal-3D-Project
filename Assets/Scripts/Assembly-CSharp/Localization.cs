using System;
using UnityEngine;
using UnityEngine.UI;

public class Localization : MonoBehaviour
{
	public static Action OnReset = delegate
	{
	};

	public StringsCollection[] languages;

	private int current;

	private void Awake()
	{
		string[] array = Resources.Load<TextAsset>("CSV/Localization").text.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(',');
			if (i == 0)
			{
				languages = new StringsCollection[array2.Length];
				for (int j = 0; j < languages.Length; j++)
				{
					languages[j] = new StringsCollection();
					languages[j].strings = new string[array.Length];
				}
			}
			for (int k = 0; k < array2.Length; k++)
			{
				languages[k].strings[i] = array2[k];
			}
		}
		ResetCurrent();
	}

	public bool ResetCurrent()
	{
		int value = Game.gamePrefs.GetValue("Language");
		if (current != value)
		{
			current = value;
			if (OnReset != null)
			{
				OnReset();
			}
			return true;
		}
		return false;
	}

	public string Get(int i)
	{
		return languages[current].strings[i];
	}

	public void Find(Text text)
	{
		for (int i = 0; i < languages[0].strings.Length; i++)
		{
			if (languages[0].strings[i] == text.text)
			{
				text.text = languages[current].strings[i];
				break;
			}
		}
	}
}
