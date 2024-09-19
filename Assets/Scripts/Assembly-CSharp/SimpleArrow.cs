using UnityEngine;

public class SimpleArrow : PooledMonobehaviour
{
	private void OnEnable()
	{
		base.rb.isKinematic = false;
	}

	private void Update()
	{
		if (!base.rb.isKinematic)
		{
			base.t.rotation = Quaternion.LookRotation(base.rb.velocity);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		base.rb.isKinematic = true;
	}
}
