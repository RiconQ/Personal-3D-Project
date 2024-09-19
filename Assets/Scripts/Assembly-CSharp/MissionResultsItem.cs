using System;
using UnityEngine;
using UnityEngine.UI;

public class MissionResultsItem : MonoBehaviour
{
	public enum ResultsItemType
	{
		noDamage = 0,
		rank = 1,
		kills = 2,
		stylePoints = 3,
		time = 4,
		maxCombo = 5,
		noFall = 6
	}

	public GameObject NewBestObj;

	public ResultsItemType type;

	public Vector3 aPos;

	public Vector3 bPos;

	private Text[] texts;

	public RectTransform t { get; private set; }

	public CanvasGroup cg { get; private set; }

	public bool completed { get; private set; }

	public void Setup()
	{
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		texts = GetComponentsInChildren<Text>();
		switch (type)
		{
		case ResultsItemType.noDamage:
			texts[0].text = "NODAMAGE";
			texts[1].text = (Game.mission.rawResults.noDamage ? "TRUE" : "FALSE");
			completed = Game.mission.rawResults.noDamage;
			NewBestObj.SetActive(Game.mission.rawResults.pbNoDamage);
			break;
		case ResultsItemType.noFall:
			texts[0].text = "NOFALLS";
			texts[1].text = (Game.mission.rawResults.noFalls ? "TRUE" : "FALSE");
			completed = Game.mission.rawResults.noFalls;
			NewBestObj.SetActive(Game.mission.rawResults.pbNoFalls);
			break;
		case ResultsItemType.rank:
		{
			texts[0].text = "RANK";
			string[] array = new string[7] { "D", "C", "B", "A", "S", "SS", "SSS" };
			texts[1].text = $"{array[Game.mission.rawResults.rank]}";
			completed = true;
			PlayerPrefs.SetInt($"{Game.mission.levelData.name}Rank", Game.mission.rawResults.rank + 1);
			break;
		}
		case ResultsItemType.kills:
			texts[0].text = "KILLS";
			break;
		case ResultsItemType.stylePoints:
			texts[0].text = "SCORE";
			texts[1].text = Game.mission.rawResults.points.ToString("n0");
			completed = true;
			NewBestObj.SetActive(Game.mission.rawResults.pbPoints);
			break;
		case ResultsItemType.time:
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(Game.mission.rawResults.time);
			texts[0].text = "TIME";
			texts[1].text = timeSpan.ToString("mm\\:ss\\:ff");
			completed = true;
			NewBestObj.SetActive(Game.mission.rawResults.pbTime);
			break;
		}
		case ResultsItemType.maxCombo:
			texts[0].text = "COMBO";
			texts[1].text = $"X{Game.mission.rawResults.combo.ToString()}";
			completed = Game.mission.rawResults.combo >= 15;
			NewBestObj.SetActive(Game.mission.rawResults.pbCombo);
			completed = true;
			break;
		}
	}
}
