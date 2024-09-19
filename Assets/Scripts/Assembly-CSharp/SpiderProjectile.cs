using UnityEngine;

public class SpiderProjectile : PooledMonobehaviour, IDamageable<DamageData>
{
	private float _timer;

	private Vector3 _Direction;

	public Transform tMesh;

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		_timer = 5f;
	}

	public void Damage(DamageData dmg)
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	public void Update()
	{
		_Direction = base.t.position.DirTo(Game.player.t.position);
		tMesh.Rotate(360f * Time.deltaTime, 0f, 0f);
		base.t.rotation = Quaternion.RotateTowards(base.t.rotation, Quaternion.LookRotation(_Direction), Time.deltaTime * 30f);
		base.t.Translate(base.t.forward * (10f * Time.deltaTime), Space.World);
		if (_timer != 0f)
		{
			_timer = Mathf.MoveTowards(_timer, 0f, Time.deltaTime);
			if (_timer == 0f)
			{
				QuickEffectsPool.Get("Goo Splash", base.t.position).Play();
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		switch (other.gameObject.layer)
		{
		case 0:
			QuickEffectsPool.Get("Goo Splash", base.t.position).Play();
			base.gameObject.SetActive(value: false);
			break;
		case 9:
			Game.player.Damage(base.t.forward);
			QuickEffectsPool.Get("Goo Splash", base.t.position).Play();
			base.gameObject.SetActive(value: false);
			break;
		}
	}
}
