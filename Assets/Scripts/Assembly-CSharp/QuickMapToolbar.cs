using UnityEngine;

public class QuickMapToolbar : QuickmapUI
{
	private QuickmapCollectionUI[] items;

	private EditorCollectionEntry[] entries;

	public int index { get; private set; }

	public GameObject GetSelectedPrefab()
	{
		return items[index].GetPrefab();
	}

	private void Awake()
	{
		entries = GetComponentsInChildren<EditorCollectionEntry>();
		for (int i = 0; i < entries.Length; i++)
		{
			entries[i].Setup(i, "", this);
		}
		index = -1;
		items = base.transform.root.GetComponentsInChildren<QuickmapCollectionUI>();
		for (int j = 0; j < items.Length; j++)
		{
			items[j].gameObject.SetActive(index == j);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			Set(0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			Set(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			Set(2);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			Set(3);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			Set(4);
		}
	}

	public override void Set(int newIndex)
	{
		index = newIndex;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].gameObject.SetActive((index == i) ? true : false);
		}
	}
}
