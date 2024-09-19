using UnityEngine;

public class TempObjects : MonoBehaviour
{
	public string keyword = "Armless1";

	public GameObject[] objects;

	public void Awake()
	{
		if (PlayerPrefs.GetInt(keyword) == 1)
		{
			GameObject[] array = objects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}
		else
		{
			PlayerPrefs.SetInt(keyword, 1);
		}
	}
}
