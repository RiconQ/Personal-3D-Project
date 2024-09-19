using UnityEngine;
using UnityEngine.UI;

public class StyleMoveDataCard : MonoBehaviour
{
	public int index;

	public Text title;

	public Text description;

	public Text combos;

	private StyleMoveData data;

	private void Awake()
	{
		data = StyleData.GetData(index);
		Setup();
	}

	public void Setup()
	{
		title.text = data.name;
		description.text = data.description;
	}
}
