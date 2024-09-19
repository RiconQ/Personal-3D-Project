using UnityEngine;

public class Damaged : PooledMonobehaviour
{
	private Rigidbody[] rigidbodies;

	[SerializeField]
	private float force = 10f;

	protected override void Awake()
	{
		base.Awake();
		rigidbodies = GetComponentsInChildren<Rigidbody>();
	}

	private void OnEnable()
	{
		for (int i = 0; i < rigidbodies.Length; i++)
		{
			rigidbodies[i].AddForce(MegaHelp.RandomVector3() * force, ForceMode.Impulse);
			rigidbodies[i].AddTorque(MegaHelp.RandomVector3() * 360f, ForceMode.Impulse);
		}
	}
}
