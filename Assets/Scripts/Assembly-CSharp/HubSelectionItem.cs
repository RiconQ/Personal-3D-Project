using System;
using UnityEngine;
using UnityEngine.UI;

public class HubSelectionItem : QuickMenuItem
{
	public GameObject objLabel;

	public GameObject objBackground;

	public TextAnimator animator;

	public Material mat;

	public Text hubName;

	public Image thumbnail;

	private bool unlocked;

	private string sceneToLoad;

	public void Setup(HubData hub)
	{
		unlocked = LevelsData.instance.GetHubState(hub) != 0;
		hubName.text = (unlocked ? hub.GetFirstLevel().publicName.Replace(" ", Environment.NewLine).ToUpper() : "UNREACHED");
		animator.ResetChars();
		sceneToLoad = hub.GetFirstLevel().sceneReference.ScenePath;
		thumbnail.sprite = hub.thumbnail;
		thumbnail.material = UnityEngine.Object.Instantiate(thumbnail.material);
		mat = thumbnail.material;
		mat.SetFloat("_Desaturation", (!unlocked) ? 1 : 0);
	}

	public override void Select()
	{
		base.cg.alpha = 1f;
		objLabel.SetActive(value: true);
		objBackground.SetActive(value: true);
		animator.Play();
		if (OnSelect != null)
		{
			OnSelect();
		}
	}

	public override void Deselect()
	{
		base.cg.alpha = 0.5f;
		objLabel.SetActive(value: false);
		objBackground.SetActive(value: false);
		if (OnDeselect != null)
		{
			OnDeselect();
		}
	}

	public override bool Accept()
	{
		base.Accept();
		if (unlocked)
		{
			Game.lastSceneWithPlayer = string.Empty;
			Game.instance.LoadLevel(sceneToLoad);
			GetComponentInParent<QuickMenu>().locked = true;
			return true;
		}
		return false;
	}
}
