using UnityEngine;
using UnityEngine.UI;

public class BuildInfoText : MonoBehaviour
{
	public Text txt;

	public string textTemplate = "Development Build {0}";

	private void Awake()
	{
		TextAsset textAsset = Resources.Load("app_build_time") as TextAsset;
		if ((bool)textAsset)
		{
			txt.text = string.Format(textTemplate, textAsset.text);
		}
	}
}
