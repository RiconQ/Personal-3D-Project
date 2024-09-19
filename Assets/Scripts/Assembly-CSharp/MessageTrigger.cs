using UnityEngine;

public class MessageTrigger : MonoBehaviour
{
	[SerializeField]
	[TextArea]
	private string messageText;

	private void OnTriggerEnter()
	{
		Game.message.Show(messageText);
	}

	private void OnTriggerExit()
	{
		Game.message.Hide();
	}
}
