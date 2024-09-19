using UnityEngine;

public class PlayerMovementDebug : MonoBehaviour
{
	private Vector3 oldPos;

	private PlayerController player;

	private void Awake()
	{
		player = GetComponent<PlayerController>();
		oldPos = player.t.position;
	}

	private void FixedUpdate()
	{
		Debug.DrawLine(oldPos, player.t.position, player.rb.isKinematic ? Color.grey : ((!player.grounder.grounded) ? ((player.airControlBlock > 0f) ? Color.magenta : ((player.gTimer > 0f) ? (Color.red / 2f) : ((player.onPlatformable != null) ? Color.cyan : Color.red))) : ((player.slide.slideState == 0) ? Color.green : Color.yellow)), 2f);
		oldPos = player.t.position;
	}
}
