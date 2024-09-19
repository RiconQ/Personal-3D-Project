using System;
using UnityEngine;

public class StartWeaponUI : MonoBehaviour
{
	public static StartWeaponUI instance;

	public bool weaponIsPicked;

	public CanvasGroup cg;

	public int alpha;

	public Transform[] tWeapons;

	public int index { get; private set; }

	private void Awake()
	{
		instance = this;
		index = -1;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Start()
	{
		if ((bool)LevelsData.currentPlayableLevel)
		{
			base.gameObject.SetActive(LevelsData.currentPlayableLevel.sceneType > SceneData.SceneType.Tutorial);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		weaponIsPicked = false;
		index = -1;
		for (int i = 0; i < tWeapons.Length; i++)
		{
			tWeapons[i].localScale = ((index + 1 == i) ? (Vector3.one * 1.2f) : Vector3.one);
		}
		cg.alpha = 0f;
	}

	private void Update()
	{
		cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.unscaledDeltaTime * 4f);
		if (cg.alpha == 1f)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			Vector2 vector = Input.mousePosition;
			vector.x -= Screen.width / 2;
			vector.y -= Screen.height / 2;
			if (vector.x.Abs() > 32f)
			{
				index = ((vector.x > 0f) ? 1 : 0);
			}
			else
			{
				index = -1;
			}
			for (int i = 0; i < tWeapons.Length; i++)
			{
				tWeapons[i].localScale = ((index + 1 == i) ? (Vector3.one * 1.2f) : Vector3.one);
			}
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}
}
