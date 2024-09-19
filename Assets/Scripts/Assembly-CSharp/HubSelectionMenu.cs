using UnityEngine;

public class HubSelectionMenu : QuickMenu
{
	public HubData[] hubs;

	public GameObject hubCard;

	public RectTransform tContent;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float speed = 2f;

	private float timer;

	private Vector3 oldPos;

	private Vector3 pos;

	public override void Awake()
	{
		HubData[] array = hubs;
		foreach (HubData hub in array)
		{
			Object.Instantiate(hubCard, tContent).GetComponent<HubSelectionItem>().Setup(hub);
		}
		base.Awake();
	}

	public override void ItemNext(int sign = 1)
	{
		base.Next(-sign);
		Refresh();
	}

	public override void Next(int sign = 1)
	{
		base.ItemNext(sign);
		Refresh();
	}

	public override void Activate()
	{
		base.Activate();
		Refresh();
		tContent.anchoredPosition3D = pos;
		timer = 1f;
	}

	private void Refresh()
	{
		timer = 0f;
		oldPos = (pos = tContent.anchoredPosition3D);
		pos.x = 0f - items[index].t.localPosition.x;
	}

	private void LateUpdate()
	{
		if (base.active && timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
			tContent.anchoredPosition3D = Vector3.LerpUnclamped(oldPos, pos, curve.Evaluate(timer));
		}
	}
}
