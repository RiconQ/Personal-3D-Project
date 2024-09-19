using UnityEngine;

public class FireVat : TrapBase
{
	public DamageData dmg;

	private Collider[] colliders = new Collider[3];

	private ParticleSystem particle;

	private void Awake()
	{
		base.t = base.transform;
		particle = GetComponentInChildren<ParticleSystem>();
		dmg.amount = 10f;
		dmg.newType = Game.style.basicBurn;
	}

	public override void Trigger()
	{
		Physics.OverlapSphereNonAlloc(base.t.position, 6f, colliders, 1024);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] != null)
			{
				dmg.dir = base.t.position.DirTo(colliders[i].transform.position);
				dmg.dir.y += 1f;
				dmg.dir.Normalize();
				colliders[i].GetComponent<IDamageable<DamageData>>().Damage(dmg);
				colliders[i] = null;
			}
		}
		particle.Play();
	}
}
