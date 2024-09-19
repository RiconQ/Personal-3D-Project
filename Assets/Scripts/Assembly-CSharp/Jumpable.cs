using UnityEngine;

public class Jumpable : MonoBehaviour
{
	public float jumpForce = 1.5f;

	public float blockTimer = 0.1f;

	public float velMult = 0.5f;

	public GameObject prefab;

	public void OnJump()
	{
		if (velMult != 1f)
		{
			Vector3 velocity = Game.player.rb.velocity;
			velocity.x *= velMult;
			velocity.z *= velMult;
			Game.player.rb.velocity = velocity;
		}
		Game.player.airControlBlock = blockTimer;
		Game.player.BasicJump(jumpForce);
		QuickPool.instance.Get(prefab, base.transform.position, Quaternion.identity);
	}
}
