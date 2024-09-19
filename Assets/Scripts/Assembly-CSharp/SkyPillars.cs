using System.Collections.Generic;
using UnityEngine;

public class SkyPillars : MonoBehaviour
{
	public GameObject[] prefabs;

	public List<GameObject> pillars = new List<GameObject>();

	[Button]
	public void AddPillars()
	{
		if (pillars.Count > 0)
		{
			foreach (GameObject pillar in pillars)
			{
				if ((bool)pillar)
				{
					Object.DestroyImmediate(pillar);
				}
			}
			pillars.Clear();
		}
		Transform parent = base.transform;
		for (int i = 0; i < 4; i++)
		{
			GameObject gameObject = Object.Instantiate(prefabs[Random.Range(0, prefabs.Length)], parent);
			gameObject.transform.localEulerAngles = new Vector3(-90f, Random.Range(0, 360), 0f);
			Vector3 localPosition = Quaternion.Euler(0f, i * 90, 0f) * Vector3.forward * 30f;
			localPosition.y = Random.Range(5, 20);
			gameObject.transform.localPosition = localPosition;
			gameObject.transform.localScale = Vector3.one * Random.Range(2f, 3f);
			pillars.Add(gameObject);
		}
	}
}
