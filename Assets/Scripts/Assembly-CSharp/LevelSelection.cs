using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelection : QuickMenu
{
	public HubsUIEntry[] hubs;

	public Transform tLevelsRoot;

	public GameObject levelUI;

	public int hubIndex;

	public bool hubIsLocked;

	public Text txtHeader;

	public Image imgThumbnail;

	public RectTransform tThumbnail;

	public CanvasGroup cgThumbnail;

	public CanvasGroup cgInfo;

	public AudioClip sfxSwitch;

	private Material mat;

	private void Start()
	{
		mat = imgThumbnail.material;
		Game.lastSceneWithPlayer = "";
		HubsUIEntry[] array = hubs;
		foreach (HubsUIEntry hubsUIEntry in array)
		{
			hubsUIEntry.items = new List<HubsUILevelEntry>();
			if (hubsUIEntry.extraLevels.Length == 0)
			{
				for (int j = 0; j < hubsUIEntry.data.levels.Count; j++)
				{
					if (!hubsUIEntry.data.levels[j].testLevel)
					{
						LevelsData.instance.RegisterMission(hubsUIEntry.data.levels[j]);
						HubsUILevelEntry component = Object.Instantiate(levelUI, tLevelsRoot).GetComponent<HubsUILevelEntry>();
						component.Setup(hubsUIEntry.data.levels[j], (j == 0) ? "Hub" : "");
						hubsUIEntry.items.Add(component);
					}
				}
			}
			else
			{
				for (int k = 0; k < hubsUIEntry.extraLevels.Length; k++)
				{
					LevelsData.instance.RegisterMission(hubsUIEntry.extraLevels[k]);
					HubsUILevelEntry component = Object.Instantiate(levelUI, tLevelsRoot).GetComponent<HubsUILevelEntry>();
					component.Setup(hubsUIEntry.extraLevels[k], (k == 0) ? "Hub" : "");
					hubsUIEntry.items.Add(component);
				}
			}
		}
	}

	public override void Activate()
	{
		base.Activate();
		SwitchHub();
	}

	public override void Next(int sign = 1)
	{
		if (base.active && !locked && hubs[hubIndex].items.Count > 0)
		{
			if (hubs[hubIndex].NextItem(sign) && (bool)sounds)
			{
				Game.sounds.PlaySound(sounds.next, 2);
			}
			RectTransform obj = tLevelsRoot as RectTransform;
			obj.anchoredPosition3D = obj.anchoredPosition3D.With(null, 0f - hubs[hubIndex].items[hubs[hubIndex].index].t.localPosition.y);
		}
	}

	public override void ItemNext(int sign = 1)
	{
		if (base.active && !locked)
		{
			SwitchHub(sign);
		}
	}

	public override void Accept()
	{
		if (base.active && !locked && !hubIsLocked)
		{
			if (hubs[hubIndex].index == -1)
			{
				Hub.lastHub = hubs[hubIndex].data.levels[0].sceneName;
				Game.instance.LoadLevel(hubs[hubIndex].data.levels[0].sceneReference.ScenePath);
				locked = true;
			}
			else if (hubs[hubIndex].Load())
			{
				locked = true;
			}
			if ((bool)sounds)
			{
				Game.sounds.PlaySound(sounds.accept, 2);
			}
		}
	}

	public void SwitchHub(int sign = 0)
	{
		int num = 0;
		if (sign != 0)
		{
			num = hubIndex.NextClamped(hubs.Length, sign);
		}
		else
		{
			for (int i = 0; i < hubs.Length && LevelsData.instance.GetHubState(hubs[i].data) != 0; i++)
			{
				num = i;
			}
		}
		hubIsLocked = LevelsData.instance.GetHubState(hubs[num].data) == 0;
		if ((hubIndex != num && !hubIsLocked) || sign == 0)
		{
			hubIndex = num;
			hubs[hubIndex].index = 0;
			hubs[hubIndex].NextItem(0);
			RectTransform obj = tLevelsRoot as RectTransform;
			obj.anchoredPosition3D = obj.anchoredPosition3D.With(null, 0f - hubs[hubIndex].items[hubs[hubIndex].index].t.localPosition.y);
			cgThumbnail.alpha = 0f;
			tThumbnail.anchoredPosition3D = Vector3.zero;
			imgThumbnail.sprite = hubs[hubIndex].data.thumbnail;
			mat = imgThumbnail.materialForRendering;
			mat.SetFloat("_Desaturation", hubIsLocked ? 1 : 0);
			cgInfo.alpha = ((!hubIsLocked) ? 1 : 0);
			if (!hubIsLocked)
			{
				txtHeader.text = hubs[hubIndex].data.levels[0].publicName;
				txtHeader.GetComponent<TextAnimator>().ResetAndPlay();
			}
			if ((bool)sounds)
			{
				Game.sounds.PlaySound(sfxSwitch, 2);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		hubs[hubIndex].Tick();
		tThumbnail.anchoredPosition3D = Vector3.Lerp(tThumbnail.anchoredPosition3D, new Vector3(-96f, 0f, 0f), Time.deltaTime);
		cgThumbnail.alpha = Mathf.Lerp(cgThumbnail.alpha, 1f, Time.deltaTime * 4f);
	}
}
