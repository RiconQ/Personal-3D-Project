using System.Collections.Generic;
using UnityEngine;

namespace QuickmapEditor
{
	public class PrefabsButton : MyButton
	{
		public RectTransform tContent;

		public GameObject[] prefabs;

		public GameObject prefabEntryUI;

		public KeyCode key;

		private float scroll;

		private List<PrefabCard> cards = new List<PrefabCard>();

		public int index { get; private set; }

		protected override void Awake()
		{
			base.Awake();
			for (int i = 0; i < prefabs.Length; i++)
			{
				PrefabCard component = Object.Instantiate(prefabEntryUI, tContent).GetComponent<PrefabCard>();
				if (prefabs[i].TryGetComponent<QuickmapObject>(out var component2))
				{
					component.Setup(component2.PublicName, i);
				}
				component.button = this;
				component.SetAlpha(0f);
				component.SetupPos(i);
				cards.Add(component);
			}
		}

		public void Select(PrefabCard card)
		{
			if (!card)
			{
				index = -1;
			}
			for (int i = 0; i < cards.Count; i++)
			{
				if (cards[i] == card)
				{
					index = i;
					cards[i].SetAlpha(1f);
				}
				else
				{
					cards[i].SetAlpha(0.5f);
				}
			}
		}

		public void Refresh()
		{
			for (int i = 0; i < cards.Count; i++)
			{
				cards[i].SetAlpha((i == index) ? 1f : 0.5f);
			}
		}

		public override void LeftClick()
		{
			base.LeftClick();
			if (base.toggled)
			{
				index = 0;
				Refresh();
				for (int i = 0; i < cards.Count; i++)
				{
					cards[i].SetupPos(i, cards.Count);
				}
			}
			else
			{
				for (int j = 0; j < cards.Count; j++)
				{
					cards[j].SetAlpha(0f);
					cards[j].SetupPos(j);
				}
			}
		}

		private void Update()
		{
			if (!QuickmapScene.instance.editorCamera.enabled)
			{
				return;
			}
			for (int i = 0; i < cards.Count; i++)
			{
				cards[i].Tick();
			}
			if (Input.GetKeyDown(key))
			{
				LeftClick();
			}
			if (!base.toggled)
			{
				return;
			}
			scroll = Input.GetAxis("Mouse Wheel");
			if (scroll != 0f)
			{
				if (scroll > 0f)
				{
					index = index.NextClamped(prefabs.Length);
					scroll = 0f;
					Refresh();
				}
				if (scroll < 0f)
				{
					index = index.NextClamped(prefabs.Length, -1);
					scroll = 0f;
					Refresh();
				}
			}
		}
	}
}
