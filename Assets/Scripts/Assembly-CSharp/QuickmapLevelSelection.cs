using System.IO;
using UnityEngine;

public class QuickmapLevelSelection : QuickMenu
{
	public GameObject prefab;

	public override void Awake()
	{
		string[] files = Directory.GetFiles(Quickmap.GetPathToMyLevels(), "*.quickmap");
		for (int i = 0; i < files.Length; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
			GameObject obj = Object.Instantiate(prefab, base.transform);
			obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(i * 256, 0f, 0f);
			QUI_TowerCreator component = obj.GetComponent<QUI_TowerCreator>();
			WWW wWW = new WWW(Quickmap.PreviewPictureName(fileNameWithoutExtension));
			if (wWW != null)
			{
				Texture2D texture2D = new Texture2D(Quickmap.previewPictureWidth, Quickmap.previewPictureHeight, TextureFormat.ARGB32, mipChain: false);
				wWW.LoadImageIntoTexture(texture2D);
				Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
				sprite.name = fileNameWithoutExtension;
				component.Setup(fileNameWithoutExtension, sprite);
			}
		}
		base.Awake();
	}

	public override void Back()
	{
		if (!locked)
		{
			base.Back();
		}
	}

	public override void ItemNext(int sign = 1)
	{
		base.Next(-sign);
	}

	public override void Next(int sign = 1)
	{
		base.ItemNext(sign);
	}

	protected override void Update()
	{
		base.Update();
		for (int i = 0; i < items.Length; i++)
		{
			items[i].t.anchoredPosition3D = new Vector3((i - index) * 200, (index == i) ? 32 : 0, 0f);
		}
	}
}
