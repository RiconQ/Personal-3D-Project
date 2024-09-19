using UnityEngine;
using UnityEngine.UI;

public class SliderControl : MonoBehaviour
{
	public int channelID;

	public Text sliderLabel;

	public DynamicPlayer dynamicPlayer;

	private void Start()
	{
		if ((bool)dynamicPlayer.musicSet && channelID < dynamicPlayer.musicSet.audioClips.Length)
		{
			sliderLabel.text = dynamicPlayer.musicSet.audioClips[channelID].name + " Volume";
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void SetVolume(float newVolume)
	{
		dynamicPlayer.SetSourceVolume(channelID, newVolume);
	}
}
