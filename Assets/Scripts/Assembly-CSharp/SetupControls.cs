using UnityEngine;

public class SetupControls : MonoBehaviour
{
	public DynamicPlayer dynamicPlayer;

	public GameObject layersPanel;

	public GameObject partsPanel;

	public GameObject errorPanel;

	public GameObject introPanel;

	private void Start()
	{
		Setup();
	}

	private void Setup()
	{
		introPanel.SetActive(value: false);
		if ((bool)dynamicPlayer)
		{
			if (!dynamicPlayer.musicSet)
			{
				layersPanel.SetActive(value: false);
				partsPanel.SetActive(value: false);
				errorPanel.SetActive(value: true);
			}
			else if (dynamicPlayer.musicSet.partSet)
			{
				layersPanel.SetActive(value: false);
				partsPanel.SetActive(value: true);
				errorPanel.SetActive(value: false);
			}
			else if (!dynamicPlayer.musicSet.partSet)
			{
				layersPanel.SetActive(value: true);
				partsPanel.SetActive(value: false);
				errorPanel.SetActive(value: false);
			}
		}
	}
}
