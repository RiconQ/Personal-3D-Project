using System;
using System.IO;
using UnityEngine;

public class QuickmapLevels : qmUI
{
	public GameObject QuickmapLevelCard;

	public RectTransform tContent;

	private Vector3 newPos;

	protected override void Awake()
	{
		base.Awake();
		string[] files = Directory.GetFiles(Quickmap.GetPathToMyLevels(), "*.quickmap");
		for (int i = 0; i < files.Length; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
			QuickmapLevelEntryUI component = UnityEngine.Object.Instantiate(QuickmapLevelCard, tContent).GetComponent<QuickmapLevelEntryUI>();
			component.OnClick = (Action)Delegate.Combine(component.OnClick, new Action(Check));
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
	}

	private new void OnEnable()
	{
		cg.alpha = 0f;
		newPos = tContent.anchoredPosition3D;
	}

	private void Update()
	{
		if (cg.alpha != 1f)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, 1f, Time.deltaTime * 4f);
		}
		float axis = Input.GetAxis("Mouse Wheel");
		if (axis.Abs() != 0f)
		{
			newPos.y = Mathf.Clamp(newPos.y + axis * 256f, (0f - tContent.sizeDelta.y) / 2f + 100f, tContent.sizeDelta.y / 2f - 100f);
		}
		if (tContent.anchoredPosition3D != newPos)
		{
			tContent.anchoredPosition3D = Vector3.Lerp(tContent.anchoredPosition3D, newPos, Time.deltaTime * 8f);
		}
	}

	private void Check()
	{
	}
}
