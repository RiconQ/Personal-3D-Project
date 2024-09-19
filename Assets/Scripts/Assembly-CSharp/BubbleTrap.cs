using UnityEngine;

public class BubbleTrap : PooledMonobehaviour
{
	public static BubbleTrap lastEnabled;

	public DamageData damage;

	private float timer;

	private Transform tMesh;

	private Collider[] colliders = new Collider[1];

	protected override void Awake()
	{
		base.Awake();
		tMesh = base.t.Find("Mesh").transform;
	}

	private void OnEnable()
	{
		timer = 0f;
		lastEnabled = this;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (lastEnabled == this)
		{
			lastEnabled = null;
		}
	}

	private void Update()
	{
		if (timer < 1f)
		{
			timer += Time.deltaTime * 2f;
			tMesh.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 2f, timer);
		}
	}

	public void Explode()
	{
		Physics.OverlapSphereNonAlloc(base.t.position, 1f, colliders, 17920);
		if ((bool)colliders[0])
		{
			damage.amount = 20f;
			damage.knockdown = true;
			damage.dir = Vector3.up;
			colliders[0].GetComponent<IDamageable<DamageData>>().Damage(damage);
			colliders[0] = null;
		}
		QuickEffectsPool.Get("Bubble Explosion", base.t.position).Play();
		base.gameObject.SetActive(value: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer != 9)
		{
			Explode();
			return;
		}
		QuickEffectsPool.Get("Bubble Explosion", base.t.position).Play();
		base.gameObject.SetActive(value: false);
	}
}
