using UnityEngine;

public class CTEToolbarUI : MonoBehaviour
{
	public static CTEToolbarUI instance;

	private CTEToolbarCollection[] collections;

	private RectTransform t;

	private int index = -1;

	private Vector3 offset;

	public GameObject GetPrefab()
	{
		if (index == -1)
		{
			return null;
		}
		return collections[index].items[collections[index].index].prefab;
	}

	private void Awake()
	{
		instance = this;
		t = GetComponent<RectTransform>();
		collections = GetComponentsInChildren<CTEToolbarCollection>();
	}

	private void SwitchCollection(int newIndex)
	{
		if (index == newIndex)
		{
			collections[index].Next();
			return;
		}
		index = newIndex;
		offset = Vector3.zero;
		collections[index].t.anchoredPosition = Vector3.down * 400f;
		collections[index].Refresh();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			SwitchCollection(0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			SwitchCollection(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			SwitchCollection(2);
		}
		if (index > -1)
		{
			collections[index].t.position = Vector3.Lerp(collections[index].t.position, t.position + offset, Time.deltaTime * 4f);
		}
		for (int i = 0; i < collections.Length; i++)
		{
			collections[i].cg.alpha = Mathf.Lerp(collections[i].cg.alpha, (i == index) ? 1 : 0, Time.deltaTime * 8f);
		}
	}
}
