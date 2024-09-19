using UnityEngine;
using UnityEngine.UI;

public class QuickmapLevelEntryUI : MyButton
{
	public Text txtName;

	public Image imgPreview;

	private string mapname;

	public void Setup(string quickmapName, Sprite quickmapPreview)
	{
		txtName.text = (mapname = quickmapName);
		imgPreview.sprite = quickmapPreview;
	}

	public override void LeftClick()
	{
		base.LeftClick();
		Quickmap.customMapName = mapname;
		Game.fading.InstantFade(1f);
		Game.instance.LoadLevel("QuickmapWorld");
	}
}
