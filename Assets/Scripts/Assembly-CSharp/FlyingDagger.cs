using System;
using UnityEngine;

public class FlyingDagger : MonoBehaviour
{
	public static Action OnDaggerHit = delegate
	{
	};

	public float speed = 25f;

	public float speed2 = 1f;

	private float chasingSpeed = 3f;

	private Transform t;

	private Transform tTarget;

	private Rigidbody rb;

	private Vector3 p0;

	private Vector3 p1;

	private Vector3 p2;

	private Vector3 p3;

	private float timer;

	private float normal;

	private bool chasing;

	private DamageData damage;

	private void Awake()
	{
		t = base.transform;
		rb = GetComponent<Rigidbody>();
		damage = new DamageData();
		damage.amount = 10f;
		damage.knockdown = true;
	}

	private void OnEnable()
	{
		rb.isKinematic = false;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
		timer = 0f;
		speed2 = 1f;
		t.rotation = Quaternion.LookRotation(new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
		chasing = false;
		normal = 8f;
		if ((bool)tTarget)
		{
			normal = Vector3.Distance(t.position, tTarget.position);
		}
		if (normal > 8f)
		{
			normal = 8f;
		}
		p0 = t.position;
		p1 = t.position + t.forward * normal;
		p1 = t.position + t.forward * normal;
		_ = (bool)tTarget;
	}

	private void FixedUpdate()
	{
		if (!tTarget)
		{
			return;
		}
		if (!chasing)
		{
			rb.isKinematic = true;
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			timer = 0f;
			speed2 = 1f;
			normal = 8f;
			if ((bool)tTarget)
			{
				normal = Vector3.Distance(t.position, tTarget.position);
			}
			if (normal > 8f)
			{
				normal = 8f;
			}
			p0 = t.position;
			p1 = t.position + t.forward * normal;
			chasing = true;
		}
		timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * chasingSpeed);
		Vector3 vector = MegaHelp.CalculateCubicBezierPoint(timer, p0, p1, tTarget.position + tTarget.position.DirTo(p0).normalized * (normal / 2f), tTarget.position);
		t.rotation = Quaternion.LookRotation(t.position.DirTo(vector));
		t.position = vector;
		if (timer != 1f)
		{
			return;
		}
		QuickEffectsPool.Get("Arrow Hit", t.position, Quaternion.LookRotation(-t.forward)).Play();
		damage.dir = Vector3.up;
		if (tTarget.gameObject.activeInHierarchy)
		{
			tTarget.GetComponentInChildren<IDamageable<DamageData>>().Damage(damage);
			if (OnDaggerHit != null)
			{
				OnDaggerHit();
			}
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!chasing)
		{
			if (OnDaggerHit != null)
			{
				OnDaggerHit();
			}
			base.gameObject.SetActive(value: false);
		}
	}
}
