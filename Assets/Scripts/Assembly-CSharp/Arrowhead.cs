using System;
using UnityEngine;

public class Arrowhead : PooledMonobehaviour
{
	public AudioClip sfxThrow;

	public AudioClip sfxStrike;

	private BaseEnemy enemy;

	private Transform tMesh;

	private float timer;

	private float speed = 25f;

	private float torque;

	private Vector3 pos;

	private Vector3 targetPos;

	private Vector3 offsetNormal;

	private DamageData damage = new DamageData();

	private RaycastHit hit;

	private int intDist;

	private float dist;

	private float offset;

	private Vector3 lastPos;

	protected override void Awake()
	{
		base.Awake();
		damage.amount = 10f;
		damage.knockdown = false;
		tMesh = base.transform.Find("Mesh").transform;
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		torque = 0f;
		timer = 0f;
		pos = base.t.position;
		lastPos = base.t.position;
		offset = UnityEngine.Random.Range(1f, 3f);
		dist = (intDist = 0);
		speed = UnityEngine.Random.Range(30f, 30f);
		Game.sounds.PlayClip(sfxThrow, 0.4f);
	}

	public void Setup(BaseEnemy e)
	{
		enemy = e;
	}

	private void Update()
	{
		if (!enemy)
		{
			Deactivate();
			return;
		}
		timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 4f);
		targetPos = Vector3.Lerp(pos, enemy.GetActualPosition(), timer);
		targetPos.y += Mathf.Sin(timer * (float)Math.PI) * offset;
		base.t.position = targetPos;
		if (timer == 1f)
		{
			Deactivate();
		}
		torque = Mathf.Lerp(torque, 360f, Time.deltaTime * 4f);
		tMesh.Rotate(360f * Time.deltaTime * 6f, 0f, 0f, Space.Self);
	}

	private void FixedUpdate()
	{
		if ((int)dist * 2 != intDist)
		{
			intDist = (int)dist * 2;
			if (Physics.Linecast(lastPos, base.t.position, out hit, 1))
			{
				Deactivate();
			}
			Debug.DrawLine(lastPos, base.t.position, Color.cyan, 0.25f);
			lastPos = base.t.position;
		}
		if (dist > 16f)
		{
			Deactivate();
		}
	}

	private void Deactivate()
	{
		QuickEffectsPool.Get("Arrow Hit", base.t.position, Quaternion.LookRotation(-base.t.forward)).Play();
		base.gameObject.SetActive(value: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 10)
		{
			damage.dir = (-base.t.forward.With(null, 0f).normalized + Vector3.up * 2f).normalized;
			damage.newType = Game.style.basicBluntHit;
			damage.knockdown = true;
			other.GetComponent<IDamageable<DamageData>>().Damage(damage);
			Deactivate();
			Game.sounds.PlayClip(sfxStrike);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawLine(base.transform.position, targetPos);
	}
}
