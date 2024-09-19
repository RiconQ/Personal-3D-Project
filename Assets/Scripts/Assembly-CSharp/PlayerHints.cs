using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHints : MonoBehaviour
{
	public static bool showHints;

	public GameObject[] weaponHits;

	public AudioClip sfxOnNextHint;

	private Text[] texts;

	private string[] hashedTexts;

	private PlayerController player;

	private CanvasGroup cg;

	private int hintIndex = -1;

	private int tempIndex = -1;

	private bool isShowing;

	private void Start()
	{
		showHints = false;
		player = Game.player;
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		texts = new Text[weaponHits.Length];
		hashedTexts = new string[weaponHits.Length];
		for (int i = 0; i < weaponHits.Length; i++)
		{
			weaponHits[i].gameObject.SetActive(value: false);
			texts[i] = weaponHits[i].GetComponent<Text>();
			hashedTexts[i] = texts[i].text;
		}
		RefreshTexts();
		QuickRebindMenu.OnRebinded = (Action)Delegate.Combine(QuickRebindMenu.OnRebinded, new Action(RefreshTexts));
	}

	private void OnDestroy()
	{
		QuickRebindMenu.OnRebinded = (Action)Delegate.Remove(QuickRebindMenu.OnRebinded, new Action(RefreshTexts));
	}

	private void RefreshTexts()
	{
		for (int i = 0; i < weaponHits.Length; i++)
		{
			texts[i].text = string.Format(hashedTexts[i], PlayerController.instance.attackKey, PlayerController.instance.altKey);
		}
	}

	private void Update()
	{
		if (!showHints)
		{
			return;
		}
		bool flag = (bool)player.weapons.rbLifted || player.weapons.currentWeapon != -1;
		tempIndex = ((!player.weapons.rbLifted) ? (player.weapons.currentWeapon + 1) : 0);
		if (player.weapons.currentWeapon > -1)
		{
			if (player.grounder.grounded)
			{
				if (player.slide.slideState == 0)
				{
					tempIndex = 1;
				}
				else
				{
					tempIndex = 3;
				}
			}
			else
			{
				tempIndex = 2;
			}
		}
		if (isShowing != flag)
		{
			isShowing = flag;
			if (!isShowing)
			{
				cg.alpha = 0f;
				hintIndex = -1;
				for (int i = 0; i < weaponHits.Length; i++)
				{
					weaponHits[i].gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			if (!isShowing)
			{
				return;
			}
			if (hintIndex != tempIndex)
			{
				cg.alpha = 0f;
				hintIndex = tempIndex;
				for (int j = 0; j < weaponHits.Length; j++)
				{
					weaponHits[j].gameObject.SetActive(hintIndex == j);
				}
			}
			cg.alpha = Mathf.MoveTowards(cg.alpha, 1f, Time.unscaledDeltaTime * 4f);
		}
	}
}
