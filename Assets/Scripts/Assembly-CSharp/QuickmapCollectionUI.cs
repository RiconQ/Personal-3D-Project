using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class QuickmapCollectionUI : QuickmapUI
{
	public PrefabsCollection collection;

	public GameObject prefab;

	private List<EditorCollectionEntry> entries = new List<EditorCollectionEntry>(10);

	public int index { get; private set; }

	public RectTransform t { get; private set; }

	public CanvasGroup cg { get; private set; }

	public void Setup()
	{
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		for (int i = 0; i < collection.prefabs.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(prefab, t);
			entries.Add(gameObject.GetComponent<EditorCollectionEntry>());
			entries[i].Setup(i, collection.prefabs[i].name, this);
			entries[i].t.anchoredPosition3D = new Vector3(128 + ((i == index) ? 16 : 0), -16 - i * 32, 0f);
		}
	}

	public GameObject GetPrefab()
	{
		return collection.prefabs[index];
	}

	public void Next(int sign = 1)
	{
		index = index.Next(collection.prefabs.Length, sign);
		for (int i = 0; i < collection.prefabs.Length; i++)
		{
			entries[i].Select(index == i);
			entries[i].t.anchoredPosition3D = new Vector3(128 + ((i == index) ? 16 : 0), -16 - i * 32, 0f);
		}
	}

	public override void Set(int newIndex)
	{
		index = newIndex;
		for (int i = 0; i < collection.prefabs.Length; i++)
		{
			entries[i].Select(index == i);
			entries[i].t.anchoredPosition3D = new Vector3(128 + ((i == index) ? 16 : 0), -16 - i * 32, 0f);
		}
	}
}
