using UnityEngine;

public class FireDetector : MonoBehaviour
{
	public BowController bow;

	public ParticleSystem fire;

	public ParticleSystem goo;

	private void OnTriggerEnter(Collider other)
	{
		if (bow.arrowType == 0 && (bow.attackIndex == 0 || bow.attackIndex == 1) && bow.attackState == 1)
		{
			bow.SetArrowtype(other.CompareTag("Fire") ? 1 : 2);
			if (bow.arrowType == 1)
			{
				fire.Play();
			}
			else
			{
				goo.Play();
			}
			QuickEffectsPool.Get("Block", base.transform.position).Play();
		}
	}
}
