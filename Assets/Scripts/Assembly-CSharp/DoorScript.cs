using UnityEngine;
using UnityEngine.AI;

public class DoorScript : Door, IKickable<Vector3>
{
	private Transform kickPoint;

	private Rigidbody rb;

	private NavMeshObstacle obstacle;

	[SerializeField]
	private AudioClip sound;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		obstacle = GetComponent<NavMeshObstacle>();
		kickPoint = base.transform.Find("Kick Point").transform;
	}

	public void Kick(Vector3 dir)
	{
		if (rb.isKinematic)
		{
			rb.isKinematic = false;
			obstacle.enabled = false;
			if (OnOpening != null)
			{
				OnOpening();
			}
		}
		rb.AddForceAtPosition(dir.normalized * 10f, kickPoint.position, ForceMode.Impulse);
		Game.sounds.PlayClipAtPosition(sound, 1f, kickPoint.position);
	}
}
