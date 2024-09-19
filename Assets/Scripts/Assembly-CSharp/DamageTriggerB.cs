using UnityEngine;

public class DamageTriggerB : MonoBehaviour
{
	public DamageData damage;

	public bool contactDirection = true;

	private Transform t;

	private void Awake()
	{
		t = base.transform;
	}

	private void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.layer == 9)
		{
			return;
		}
		if (contactDirection)
		{
			damage.dir = t.position.DirTo(c.transform.position).With(null, 0f);
		}
		else if ((bool)c.attachedRigidbody)
		{
			if (c.attachedRigidbody.isKinematic)
			{
				damage.dir = c.transform.forward;
			}
			else
			{
				damage.dir = c.attachedRigidbody.velocity.normalized;
			}
		}
		Debug.DrawLine(t.position, c.transform.position, Color.blue, 2f);
		Debug.DrawRay(t.position, damage.dir, Color.red, 3f);
		c.GetComponent<IDamageable<DamageData>>().Damage(damage);
	}
}
