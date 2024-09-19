using UnityEngine;

public class CustomGravity : MonoBehaviour
{
	private Rigidbody rb;

	private Vector3 gravity = new Vector3(0f, -40f, 0f);

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		rb.AddForce(gravity);
	}
}
