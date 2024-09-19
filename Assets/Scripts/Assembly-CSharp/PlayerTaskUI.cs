using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTaskUI : MonoBehaviour
{
	public Animation[] anims;

	public Text text;

	public RectTransform tText;

	public RectTransform[] tToggles;

	public RectTransform tShadow;

	public PlayerTask[] tasks;

	private int taskIndex = -1;

	private Vector2 temp;

	private PlayerTask currentTask;

	private CanvasGroup cg;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
		NextTask();
		StyleRanking.OnStylePoint = (Action<StylePointTypes>)Delegate.Combine(StyleRanking.OnStylePoint, new Action<StylePointTypes>(Check));
	}

	private void OnDestroy()
	{
		StyleRanking.OnStylePoint = (Action<StylePointTypes>)Delegate.Remove(StyleRanking.OnStylePoint, new Action<StylePointTypes>(Check));
	}

	private void Check(StylePointTypes stylePoint)
	{
		if (currentTask.type != stylePoint)
		{
			return;
		}
		anims[currentTask.index].Play("TaskToggle");
		currentTask.index++;
		if (currentTask.index == anims.Length)
		{
			for (int i = 0; i < anims.Length; i++)
			{
				anims[i].Play("TaskToggleReset");
			}
			NextTask();
		}
	}

	private void NextTask()
	{
		taskIndex++;
		if (taskIndex >= tasks.Length)
		{
			Game.mission.SetState(2);
			return;
		}
		currentTask = tasks[taskIndex];
		text.text = currentTask.discription;
		Invoke("UpdateSizeAndPosition", 0.1f);
	}

	private void UpdateSizeAndPosition()
	{
		temp.x = (0f - tText.sizeDelta.x) / 2f - 64f;
		temp.y = -96f;
		tText.anchoredPosition3D = temp;
		temp = tText.sizeDelta;
		temp.x += 96f;
		tShadow.sizeDelta = temp;
		tShadow.anchoredPosition3D = tText.anchoredPosition3D;
		temp.x = tText.anchoredPosition3D.x - tText.sizeDelta.x / 2f;
		temp.y = -96f;
		for (int i = 0; i < tToggles.Length; i++)
		{
			temp.x -= tToggles[i].sizeDelta.x;
			tToggles[i].anchoredPosition3D = temp;
		}
	}
}
