using UnityEngine;

public class MissionCardUI : MonoBehaviour
{
	public GameObject[] objs;

	private LevelResults mission;

	private void Awake()
	{
		for (int i = 0; i < objs.Length; i++)
		{
			objs[i].SetActive(value: false);
		}
	}

	public void Setup()
	{
	}
}
