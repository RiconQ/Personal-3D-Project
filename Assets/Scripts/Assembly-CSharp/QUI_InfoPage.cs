using UnityEngine;

public class QUI_InfoPage : QUI_Title
{
	public InfoPanel[] panels;

	public override void Awake()
	{
		base.Awake();
		InfoPanel[] array = panels;
		foreach (InfoPanel obj in array)
		{
			obj.cg.alpha = 0f;
			obj.startPos = obj.t.anchoredPosition3D;
		}
	}

	public override void Activate()
	{
		base.Activate();
		InfoPanel[] array = panels;
		foreach (InfoPanel infoPanel in array)
		{
			infoPanel.cg.alpha = 0f;
			infoPanel.t.anchoredPosition3D = Vector3.Lerp(Vector3.zero, infoPanel.startPos, 0.75f);
		}
	}

	protected override void Update()
	{
		base.Update();
		InfoPanel[] array = panels;
		foreach (InfoPanel infoPanel in array)
		{
			infoPanel.cg.alpha = Mathf.MoveTowards(infoPanel.cg.alpha, 1f, Time.unscaledDeltaTime * 8f);
			infoPanel.t.anchoredPosition3D = Vector3.Lerp(infoPanel.t.anchoredPosition3D, infoPanel.startPos, Time.unscaledDeltaTime * 8f);
		}
	}
}
