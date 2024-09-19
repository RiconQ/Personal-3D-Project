using UnityEngine;

public class EnemyViewPoint : MonoBehaviour
{
	public bool occupied;

	public float delay;

	public Transform t;

	private void Awake()
	{
		t = base.transform;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = (occupied ? Color.red : Color.green);
		Gizmos.DrawWireSphere(t.position, 1f);
	}

	private void OnTriggerStay()
	{
		occupied = true;
	}
}
