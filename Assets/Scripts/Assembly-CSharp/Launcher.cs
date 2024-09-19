using UnityEngine;

public class Launcher : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		_ = Game.player.slide.isSliding;
		Game.player.grounder.Ungrounded();
		Game.player.rb.velocity *= 0.5f;
		Game.player.rb.velocity = Game.player.rb.velocity.With(null, 40f);
		Game.player.airControlBlock = 0.2f;
		Game.player.ParkourMove();
		CameraController.shake.Shake(2);
	}
}
