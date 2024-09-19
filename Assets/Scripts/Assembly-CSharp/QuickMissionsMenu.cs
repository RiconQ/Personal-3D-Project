using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickMissionsMenu : QuickMenu
{
	public static string[] missionNames = new string[0];

	public static string[] missionDisplayNames = new string[0];

	public static int current;

	public RectTransform tContent;

	public GameObject missionCardPrefab;

	public GameObject lineBreak;

	private Vector3 pos;

	private TextAnimator[] animators;

	public List<MissionCard> missions = new List<MissionCard>();

	public static string[] rumenNums = new string[20]
	{
		"I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X",
		"XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX"
	};

	public static string CurrentMissionName()
	{
		if (current >= missionDisplayNames.Length)
		{
			return "Test Mission Name";
		}
		return missionDisplayNames[current];
	}

	public static int NextMission()
	{
		current = current.Next(missionNames.Length);
		return current;
	}

	public static string NextMissionName()
	{
		current = current.Next(missionNames.Length);
		if (current != 0)
		{
			return missionNames[current];
		}
		return "DemoEnd";
	}

	public override void Awake()
	{
		missionNames = new string[missions.Count];
		missionDisplayNames = new string[missions.Count];
		animators = new TextAnimator[missions.Count];
		for (int i = 0; i < missions.Count; i++)
		{
			GameObject gameObject = Object.Instantiate(missionCardPrefab, tContent);
			gameObject.GetComponent<QUI_LoadLevel>().levelToLoad = missions[i].sceneName;
			missionNames[i] = missions[i].sceneName;
			missionDisplayNames[i] = missions[i].displayName;
			gameObject.GetComponentsInChildren<Text>()[0].text = missions[i].displayName;
			animators[i] = gameObject.GetComponentInChildren<TextAnimator>();
			animators[i].ResetChars();
			if (i < missions.Count - 1)
			{
				Object.Instantiate(lineBreak, tContent);
			}
		}
		base.Awake();
	}

	public override void Activate()
	{
		base.Activate();
		Hub.lastPortal = null;
		Game.lastSceneWithPlayer = "";
		animators[index].Play();
		Refresh();
		tContent.anchoredPosition3D = pos;
	}

	public override void OnItemChange()
	{
		base.OnItemChange();
		animators[index].Play();
		Refresh();
	}

	public virtual void Refresh()
	{
		pos = tContent.anchoredPosition3D;
		pos.y = 0f - items[index].t.localPosition.y;
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
