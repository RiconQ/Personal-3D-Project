using System.Collections.Generic;
using UnityEngine;

public class HubProgressFlags : MonoBehaviour
{
	public float Speed = 45f;

	public GameObject prefab;

	private List<GameObject> objects = new List<GameObject>();

	public HubData data;

	public float progress;

	public Transform t { get; private set; }

	public void Start()
	{
		t = base.transform;
		int num = 0;
		int num2 = 0;
		foreach (HubPortal portal in Hub.instance.portals)
		{
			if (portal.data.sceneType != SceneData.SceneType.Hub)
			{
				if (!LevelsData.instance.missions.TryGetValue(portal.data.sceneName, out portal.data.results))
				{
					LevelsData.instance.RegisterMission(portal.data.sceneName, out portal.data.results);
				}
				num++;
				num2 += ((portal.data.results.time != 0f) ? 1 : 0);
				LevelProgress component = Object.Instantiate(prefab).GetComponent<LevelProgress>();
				objects.Add(component.gameObject);
				component.gameObject.name = portal.data.name;
				component.Set((portal.data.results.time != 0f) ? 1 : 0);
				component.transform.SetParent(base.transform);
			}
		}
		Vector3 vector = new Vector3(0f, 0f, 6f);
		for (int i = 0; i < objects.Count; i++)
		{
			objects[i].transform.localPosition = vector;
			vector = Quaternion.Euler(0f, 360f / (float)objects.Count, 0f) * vector;
		}
		progress = (float)num2 / (float)num;
	}

	public void Update()
	{
		t.Rotate(0f, Time.deltaTime * Speed, 0f, Space.Self);
	}
}
