using UnityEngine;
using UnityEngine.UI;

public class QuickLevelGroup : QuickMenu
{
	public RectTransform tContent;

	public GameObject levelCard;

	[Range(1f, 4f)]
	public int columnsCount = 2;

	private Vector3 pos;

	public override void Awake()
	{
		GameObject gameObject = new GameObject();
		int num = columnsCount - 1;
		for (int i = 0; i < 10; i++)
		{
			if (num == columnsCount - 1)
			{
				GameObject obj = new GameObject();
				obj.transform.SetParent(tContent);
				obj.transform.localScale = Vector3.one;
				HorizontalLayoutGroup horizontalLayoutGroup = obj.AddComponent<HorizontalLayoutGroup>();
				horizontalLayoutGroup.childForceExpandWidth = false;
				horizontalLayoutGroup.spacing = 1f;
				gameObject = obj;
				num = 0;
			}
			else
			{
				num++;
			}
			GameObject gameObject2 = Object.Instantiate(levelCard, gameObject.GetComponent<RectTransform>());
			string customTowerName = $"CustomTower{i}";
			gameObject2.GetComponent<QUI_TowerCreator>().customTowerName = customTowerName;
			WWW wWW = new WWW(Quickmap.PreviewPictureName(customTowerName));
			if (wWW != null)
			{
				Texture2D texture2D = new Texture2D(Quickmap.previewPictureWidth, Quickmap.previewPictureHeight, TextureFormat.ARGB32, mipChain: false);
				wWW.LoadImageIntoTexture(texture2D);
				gameObject2.GetComponentInChildren<QUI_TowerCreator>().SetImage(Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero));
			}
		}
		base.Awake();
	}

	public override void Activate()
	{
		base.Activate();
		pos = tContent.anchoredPosition3D;
		pos.y = 0f - items[index].GetComponent<RectTransform>().parent.localPosition.y;
		tContent.anchoredPosition3D = pos;
		Refresh();
	}

	public override void Next(int sign = 1)
	{
		if (!base.active || locked)
		{
			return;
		}
		int num = index.NextClamped(items.Length, -sign * columnsCount);
		if (index != num)
		{
			items[index].Deselect();
			items[num].Select();
			index = num;
			OnItemChange();
			if ((bool)sounds)
			{
				Game.sounds.PlayClip(sounds.next, 0.5f);
			}
		}
	}

	public override void ItemNext(int sign = 1)
	{
		base.Next(-sign);
	}

	public override void OnItemChange()
	{
		base.OnItemChange();
		Refresh();
	}

	public virtual void Refresh()
	{
		pos = tContent.anchoredPosition3D;
		pos.y = 0f - items[index].GetComponent<RectTransform>().parent.localPosition.y;
	}

	protected override void Update()
	{
		base.Update();
		if (base.active && tContent.anchoredPosition3D != pos)
		{
			tContent.anchoredPosition3D = Vector3.Lerp(tContent.anchoredPosition3D, pos, Time.deltaTime * 8f);
		}
	}
}
