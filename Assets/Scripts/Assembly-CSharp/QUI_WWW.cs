using UnityEngine;

public class QUI_WWW : QuickMenuItem
{
	public string externalLink;

	public override bool Accept()
	{
		Application.OpenURL(externalLink);
		return true;
	}
}
