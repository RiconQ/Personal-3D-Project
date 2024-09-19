using UnityEngine;

public class HubFloorPortal : MonoBehaviour
{
	public Vector3 aPos;

	public Vector3 bPos;

	private void OnTriggerEnter(Collider other)
	{
		PlayerController.instance.grounder.Ungrounded();
		PlayerController.instance.mouseLook.LookInDir(aPos.DirTo(bPos));
		PlayerController.instance.airControlBlock = 1f;
		other.transform.position = aPos;
		other.attachedRigidbody.AddBallisticForce(bPos, 1.5f, -40f);
		Game.fading.InstantFade(1f);
		Game.fading.Fade(0f);
		CameraController.shake.Shake(2);
	}
}
