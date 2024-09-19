using UnityEngine;

public class ShieldFireslide : MonoBehaviour
{
	public DamageData dmg;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 10)
		{
			other.GetComponent<IDamageable<DamageData>>().Damage(dmg);
		}
	}
}
