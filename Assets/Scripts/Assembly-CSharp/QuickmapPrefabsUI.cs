using System;
using UnityEngine;

public class QuickmapPrefabsUI : MonoBehaviour
{
	public static int selectedPrefab = -1;

	public static QuickmapPrefabsUI instance;

	public GameObject prefabCard;

	public PrefabsCollection collection;

	public QuickmapPrefabsGroup[] groups;

	private PrefabCard[] cards;

	private RectTransform t;

	public int groupIndex { get; private set; }

	private void Awake()
	{
		instance = this;
		groupIndex = -1;
		t = GetComponent<RectTransform>();
		cards = new PrefabCard[collection.prefabs.Length];
		for (int i = 0; i < collection.prefabs.Length; i++)
		{
			cards[i] = UnityEngine.Object.Instantiate(prefabCard, t).GetComponent<PrefabCard>();
			if (collection.prefabs[i].TryGetComponent<QuickmapObject>(out var component))
			{
				cards[i].Setup(component.PublicName, i);
			}
			else
			{
				cards[i].Setup(collection.prefabs[i].name, i);
			}
			cards[i].image.color = Color.white * 0.75f;
		}
		QuickmapPrefabsGroup[] array = groups;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].cg.alpha = 0.5f;
		}
		PrefabCard.OnSelect = (Action<int>)Delegate.Combine(PrefabCard.OnSelect, new Action<int>(SelectCard));
	}

	private void OnDestroy()
	{
		PrefabCard.OnSelect = (Action<int>)Delegate.Remove(PrefabCard.OnSelect, new Action<int>(SelectCard));
	}

	private void Update()
	{
		if (QuickmapScene.instance.editorCamera.isActiveAndEnabled || QuickmapScene.instance.isPlaymode)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				ShowGroup(0);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				ShowGroup(1);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				ShowGroup(2);
			}
			for (int i = 0; i < cards.Length; i++)
			{
				cards[i].Tick();
			}
		}
	}

	public bool SnapToCellCenter()
	{
		return groups[groupIndex].snapToCellCenter;
	}

	public void SelectCard(int i = -1)
	{
		if (selectedPrefab.Inside(cards.Length))
		{
			cards[selectedPrefab].image.color = Color.white * 0.75f;
		}
		if (i.Inside(cards.Length))
		{
			cards[i].image.color = Color.black;
		}
		selectedPrefab = i;
	}

	public void ShowGroup(int index)
	{
		if (index == groupIndex || !index.Inside(groups.Length))
		{
			for (int i = 0; i < cards.Length; i++)
			{
				cards[i].SetAlpha(0f);
				cards[i].SetupPos(i - groups[groupIndex].startIndex);
			}
			QuickmapPrefabsGroup[] array = groups;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].cg.alpha = 0.5f;
			}
			groupIndex = -1;
			return;
		}
		QuickmapPrefabsGroup quickmapPrefabsGroup = groups[index];
		for (int k = 0; k < cards.Length; k++)
		{
			bool flag = (k >= quickmapPrefabsGroup.startIndex) & (k < quickmapPrefabsGroup.endIndex);
			if (!flag)
			{
				cards[k].cg.alpha = 0f;
			}
			cards[k].t.anchoredPosition3D = quickmapPrefabsGroup.cg.GetComponent<RectTransform>().position;
			cards[k].SetAlpha(flag ? 1 : 0);
			cards[k].SetupPos(k - quickmapPrefabsGroup.startIndex, quickmapPrefabsGroup.endIndex - quickmapPrefabsGroup.startIndex);
		}
		groupIndex = index;
		for (int l = 0; l < groups.Length; l++)
		{
			groups[l].cg.alpha = ((l == groupIndex) ? 1f : 0.5f);
		}
		SelectCard();
	}
}
