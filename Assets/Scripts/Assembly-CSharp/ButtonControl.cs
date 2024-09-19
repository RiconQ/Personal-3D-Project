using UnityEngine;
using UnityEngine.UI;

public class ButtonControl : MonoBehaviour
{
	public int channelID;

	public Text buttonLabel;

	public DynamicPlayer dynamicPlayer;

	private Button myButton;

	private void Start()
	{
		myButton = GetComponent<Button>();
		if ((bool)dynamicPlayer.musicSet && channelID < dynamicPlayer.musicSet.audioClips.Length)
		{
			buttonLabel.text = dynamicPlayer.musicSet.audioClips[channelID].name;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		CheckButtonState();
	}

	public void SwitchToPart()
	{
		dynamicPlayer.SwitchParts(channelID);
		ButtonManager.SelectedChannel = channelID;
	}

	private void CheckButtonState()
	{
		_ = ButtonManager.SelectedChannel;
		_ = channelID;
	}

	private void OnEnable()
	{
		ButtonManager.OnButtonUpdate += CheckButtonState;
	}

	private void OnDisable()
	{
		ButtonManager.OnButtonUpdate -= CheckButtonState;
	}
}
