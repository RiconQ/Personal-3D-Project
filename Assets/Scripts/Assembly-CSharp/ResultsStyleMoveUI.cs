using UnityEngine;
using UnityEngine.UI;

public class ResultsStyleMoveUI : MonoBehaviour
{
	public RectTransform t;

	public Text type;

	public Text fire;

	public Text knocked;

	public Text state;

	public void Setup(int i)
	{
		t.anchoredPosition3D = new Vector3(0f, i * -64, 0f);
	}
}
