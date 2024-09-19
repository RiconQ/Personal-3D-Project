using System;
using System.Collections.Generic;
using UnityEngine;

public class StyleData : MonoBehaviour
{
	public static StyleData instance;

	public static Dictionary<StylePointTypes, StyleMoveData> data = new Dictionary<StylePointTypes, StyleMoveData>();

	public TextAsset styleMovesCSV;

	public StyleRanksInfo ranks;

	public StylePoint[] stylePoints;

	[Header("Common Damage Types")]
	public DamageType playerDash;

	public DamageType playerHeadPunch;

	public DamageType basicBluntHit;

	public DamageType basicBodyHit;

	public DamageType basicBurn;

	public DamageType basicMill;

	public DamageType basicStun;

	public DamageType basicSpikes;

	[Header("Special Style Points")]
	public StylePoint kickCornered;

	public StylePoint objectShield;

	public StylePoint savingDash;

	public StylePoint backStab;

	public StylePoint bodyDomino;

	public StylePoint bodyFlamingDomino;

	public StylePoint bodyShieldHit;

	public StylePoint SwordThrow;

	public StylePoint SwordThrowAir;

	public StylePoint SwordKicked;

	public StylePoint SwordSlideSlash;

	public StylePoint BowVoidShot;

	public StylePoint EnemyNailed;

	public StylePoint EnemyHalved;

	public StylePoint EnemyMilled;

	public StylePoint EnemyImpiled;

	public StylePoint EnemyTripped;

	public StylePoint swordParry;

	public StylePoint bowParry;

	public StylePoint ObjectKicked;

	public StylePoint ObjectThrowed;

	public StylePoint ObjectPulled;

	public StylePoint bodyBreakThrough;

	public StylePoint bodyHardLanding;

	public StylePoint bodyFreeFall;

	public StylePoint slamWall;

	public StylePoint slamFloor;

	public StylePoint slamCeiling;

	public StylePoint WeaponsQuickAirPick;

	public StylePoint WeaponsSlidePick;

	public StylePoint WeaponsJumpPick;

	public StylePoint DaggerBoost;

	public static StyleMoveData GetData(int i)
	{
		return data[(StylePointTypes)i];
	}

	public static StyleMoveData GetData(StylePointTypes type)
	{
		return data[type];
	}

	private void Awake()
	{
		instance = this;
		stylePoints = Resources.LoadAll<StylePoint>("Stylepoints");
	}

	private void ReadCSV()
	{
		styleMovesCSV = Resources.Load("CSV/StyleMoves") as TextAsset;
		stylePoints = Resources.LoadAll<StylePoint>("Stylepoints");
		string[] array = styleMovesCSV.text.Split('\n');
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(',');
			if (array2.Length != 0)
			{
				StyleMoveData styleMoveData = new StyleMoveData();
				StylePointTypes key = (StylePointTypes)Enum.Parse(typeof(StylePointTypes), array2[0]);
				styleMoveData.playerAction = int.Parse(array2[1]) == 0;
				int.TryParse(array2[2], out styleMoveData.points);
				styleMoveData.jump = int.Parse(array2[3]) == 1;
				styleMoveData.slide = int.Parse(array2[4]) == 1;
				styleMoveData.parkour = int.Parse(array2[5]) == 1;
				styleMoveData.knocked = int.Parse(array2[6]) == 1;
				styleMoveData.fire = int.Parse(array2[7]) == 1;
				styleMoveData.countable = float.Parse(array2[8]) / 10f;
				styleMoveData.description = array2[9];
				styleMoveData.screenName = array2[10];
				styleMoveData.name = key.ToString();
				data.Add(key, styleMoveData);
			}
		}
	}
}
