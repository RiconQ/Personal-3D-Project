using UnityEngine;
using UnityEngine.UI;

public class QUI_TowerCreator : QuickMenuItem
{
	public string customTowerName;

	public Image imgPreview;

	public Text txtHeader;

	public Text txtDescription;

	public void Setup(string name, Sprite image)
	{
		txtHeader.text = (customTowerName = name);
		imgPreview.sprite = image;
	}

	public void SetImage(Sprite sprite)
	{
		imgPreview.sprite = sprite;
	}

	public override bool Accept()
	{
		Quickmap.customMapName = customTowerName;
		Game.instance.LoadLevel("QuickmapWorld");
		return true;
	}
}
