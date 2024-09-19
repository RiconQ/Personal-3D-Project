using UnityEngine;

public class ButtonManager : MonoBehaviour
{
	public delegate void ButtonUpdate();

	private static int selectedChannel;

	public static int SelectedChannel
	{
		get
		{
			return selectedChannel;
		}
		set
		{
			selectedChannel = value;
			ButtonManager.OnButtonUpdate();
		}
	}

	public static event ButtonUpdate OnButtonUpdate;
}
