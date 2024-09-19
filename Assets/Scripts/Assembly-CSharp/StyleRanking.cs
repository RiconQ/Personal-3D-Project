using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StyleRanking : MonoBehaviour
{
	public static StyleRanking instance;

	public static Action<StylePointTypes> OnStylePoint = delegate
	{
	};

	public static Dictionary<StylePointTypes, StyleMoveData> spDictionary = new Dictionary<StylePointTypes, StyleMoveData>();

	public static bool rage;

	public CanvasGroup cg;

	public ComboCounter combo;

	public RankLetter letter;

	public Material matRankFrame;

	public Animation anim;

	public Animation animRank;

	public StylePointCard cardPrefab;

	private StylePointCard[] cards;

	private int index;

	private int rIndex;

	private RectTransform t;

	public List<StylePoint> usedStylePoints = new List<StylePoint>(100);

	public List<StylePoint> stylePoints = new List<StylePoint>(10);

	public List<StylePointCard> activeCards = new List<StylePointCard>(10);

	public int points { get; private set; }

	public int rankIndex { get; private set; }

	public float timer { get; private set; }

	private void Start()
	{
		instance = this;
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		t = GetComponent<RectTransform>();
		combo.Setup();
		StartCoroutine(SetupCards());
		points = 0;
		matRankFrame.SetFloat("_Power", 0f);
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
		PlayerController.OnDamage = (Action<Vector3>)Delegate.Combine(PlayerController.OnDamage, new Action<Vector3>(OnDamage));
		OceanScript.OnFall = (Action)Delegate.Combine(OceanScript.OnFall, new Action(OnFall));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
		PlayerController.OnDamage = (Action<Vector3>)Delegate.Remove(PlayerController.OnDamage, new Action<Vector3>(OnDamage));
		OceanScript.OnFall = (Action)Delegate.Remove(OceanScript.OnFall, new Action(OnFall));
	}

	private void Reset()
	{
		int num2 = (rankIndex = 0);
		int num4 = (points = num2);
		timer = num4;
		StylePointCard[] array = cards;
		for (num4 = 0; num4 < array.Length; num4++)
		{
			array[num4].cg.alpha = 0f;
		}
		activeCards.Clear();
		usedStylePoints.Clear();
		matRankFrame.SetFloat("_Power", 0f);
		combo.ResetCombo();
	}

	private void OnFall()
	{
		combo.ResetCombo();
	}

	private IEnumerator SetupCards()
	{
		cards = new StylePointCard[StyleData.instance.stylePoints.Length];
		int num = 0;
		StylePoint[] array = StyleData.instance.stylePoints;
		foreach (StylePoint stylePoint in array)
		{
			cards[num] = UnityEngine.Object.Instantiate(cardPrefab).GetComponent<StylePointCard>();
			cards[num].Setup(stylePoint, t);
			num++;
		}
		yield return new WaitForEndOfFrame();
		StylePointCard[] array2 = cards;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SetupSize();
		}
	}

	private void Update()
	{
		cg.alpha = ((Game.mission.state == MissionState.MissionStates.InProcess && timer > 0f) ? Mathf.Clamp01(timer * 10f) : 0f);
		timer = Mathf.Clamp(timer, 0f, 7f);
		letter.value = timer - (float)rankIndex;
		combo.Tick();
		if (timer > 0f)
		{
			if (!rage)
			{
				if (PlayerController.instance.slide.slideState == 0)
				{
					timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime * (0.02f + (float)rankIndex / 200f));
				}
			}
			else
			{
				timer = Mathf.MoveTowards(timer, 0f, Time.unscaledDeltaTime * (0.1f + (float)rankIndex / 7f));
				if (timer == 0f)
				{
					rage = false;
					CameraController.shake.Shake(1);
					Game.time.SetDefaultTimeScale(1f);
				}
			}
			if (rankIndex != Mathf.FloorToInt(timer))
			{
				if (!Game.mission.rawResults.noDamage && Mathf.FloorToInt(timer) > 4)
				{
					timer = Mathf.FloorToInt(timer);
				}
				else
				{
					if (rankIndex < Mathf.FloorToInt(timer))
					{
						animRank.PlayNow();
						rankIndex = Mathf.FloorToInt(timer);
						if (rankIndex > 0)
						{
							timer = (float)rankIndex + 0.2f;
						}
					}
					else
					{
						animRank.PlayNow();
						rankIndex = Mathf.FloorToInt(timer);
						timer = (float)rankIndex + 0.8f;
					}
					letter.SetRankLetter(rankIndex);
				}
			}
		}
		for (int i = 0; i < activeCards.Count; i++)
		{
			activeCards[i].SetPos(new Vector3(128 + ((i < 3) ? 32 : 28), -160 - i * 22, 0f), instant: false);
			activeCards[i].Tick(i < 3);
		}
	}

	public void OnDamage(Vector3 dir)
	{
		matRankFrame.SetFloat("_Power", 0.2f);
		combo.ResetCombo();
		timer = Mathf.Clamp(timer, 0f, 5f);
	}

	public int GetScore()
	{
		return points + usedStylePoints.Count * 2 * usedStylePoints.Count * 2;
	}

	public void RegStylePoint(StylePoint stylePoint)
	{
		if (rage || !stylePoint)
		{
			return;
		}
		bool flag = false;
		int num = 0;
		foreach (StylePointCard activeCard in activeCards)
		{
			if (activeCard.stylePoint == stylePoint)
			{
				activeCards.Remove(activeCard);
				activeCards.Insert(0, activeCard);
				activeCard.cg.alpha = 1f;
				activeCard.t.anchoredPosition3D = new Vector3(0f, -160f, 0f);
				anim.PlayNow();
				if (!usedStylePoints.Contains(stylePoint))
				{
					usedStylePoints.Add(stylePoint);
					points += 75;
				}
				else
				{
					points += 25;
				}
				if (num > 2 && stylePoint.type != StylePoint.Type.Chaos)
				{
					combo.AddCombo();
					letter.blink = 1f;
				}
				flag = true;
				break;
			}
			num++;
		}
		if (!flag)
		{
			StylePointCard[] array = cards;
			foreach (StylePointCard stylePointCard in array)
			{
				if (stylePoint == stylePointCard.stylePoint)
				{
					stylePointCard.cg.alpha = 1f;
					stylePointCard.t.anchoredPosition3D = new Vector3(0f, -160f, 0f);
					stylePointCard.Refresh();
					stylePointCard.ResetCount();
					activeCards.Insert(0, stylePointCard);
					anim.PlayNow();
					timer += ((rankIndex <= 3) ? 0.1f : 0.05f) + (float)combo.combo / (80f * Mathf.Pow((float)(rankIndex + 1) / 7f, 2f));
					if (!usedStylePoints.Contains(stylePoint))
					{
						usedStylePoints.Add(stylePoint);
						points += 75;
					}
					else
					{
						points += 25;
					}
					if (stylePoint.type != StylePoint.Type.Chaos)
					{
						combo.AddCombo();
						letter.blink = 1f;
					}
					break;
				}
			}
		}
		if (activeCards.Count > 6)
		{
			activeCards[6].cg.alpha = 0f;
			activeCards.RemoveAt(6);
		}
	}

	public void AddStylePoint(StylePointTypes type, int state = -1)
	{
		Debug.Log("Old Method " + type);
	}
}
