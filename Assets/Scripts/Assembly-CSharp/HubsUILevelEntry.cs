using UnityEngine;
using UnityEngine.UI;

public class HubsUILevelEntry : MonoBehaviour
{
	public RectTransform t;

	public Text txtName;

	public Text txtRank;

	public CanvasGroup cg;

	public SceneData data;

	public void Setup(SceneData data, string name = "")
	{
		txtName.text = ((name.Length > 0) ? name : data.publicName);
		if (data.sceneType > SceneData.SceneType.Tutorial && data.results.time > 0f)
		{
			txtRank.text = Game.style.ranks.ranks[data.results.rank].name;
		}
		else
		{
			txtRank.gameObject.SetActive(value: false);
		}
		cg.alpha = 0.5f;
		this.data = data;
	}

	public void Select(bool value)
	{
		if (value)
		{
			cg.alpha = 0f;
		}
	}

	public void Tick()
	{
		if (cg.alpha != 1f)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, ((data.results.reached > 0) | (data.sceneType == SceneData.SceneType.Hub)) ? 1f : 0.25f, Time.deltaTime * 4f);
		}
	}
}
