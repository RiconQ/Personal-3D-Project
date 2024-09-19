using UnityEngine;

public class QUI_ResetBinding : QuickMenuItem
{
	public override bool Accept()
	{
		GetComponentInParent<QuickRebindMenu>().Reset();
		return true;
	}

	public override Vector2 GetSize()
	{
		return base.txt.rectTransform.sizeDelta;
	}

	public override Vector2 GetPosition()
	{
		Vector2 result = default(Vector2);
		result.x = GetSize().x / 2f + base.t.anchoredPosition3D.x - base.t.sizeDelta.x / 2f;
		result.y = base.t.anchoredPosition3D.y;
		return result;
	}
}
