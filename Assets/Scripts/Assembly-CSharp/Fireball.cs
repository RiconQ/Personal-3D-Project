using UnityEngine;

public class Fireball : PooledMonobehaviour
{
	private Collider[] colliders = new Collider[3];

	private Vector4 dir;

	private Vector3 gravity = new Vector3(0f, -9.81f, 0f);

	public LayerMask mask;

	private void FixedUpdate()
	{
		base.rb.AddForce(gravity);
	}

	private void OnCollisionEnter(Collision c)
	{
		Physics.OverlapSphereNonAlloc(base.t.position, 2f, colliders, mask);
		for (int i = 0; i < 3; i++)
		{
			if (colliders[i] != null)
			{
				dir = base.t.position.DirTo(colliders[i].transform.position);
				dir.w = 75f;
				colliders[i].GetComponent<IDamageable<Vector4>>().Damage(dir);
				colliders[i] = null;
			}
		}
		QuickEffectsPool.Get("Fireball Hit", base.t.position, Quaternion.LookRotation(c.contacts[0].normal)).Play();
		base.gameObject.SetActive(value: false);
	}
}
