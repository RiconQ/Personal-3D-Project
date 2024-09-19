using UnityEngine;

public class FloorChaser : PooledMonobehaviour
{
	public DamageData damage = new DamageData();

	private float timer;

	private Vector3 targetPosition;

	private Vector3 dir;

	private Vector3 normal = Vector3.up;

	private BaseEnemy enemy;

	private RaycastHit hit;

	private ParticleSystem particle;

	protected override void Awake()
	{
		base.Awake();
		particle = GetComponentInChildren<ParticleSystem>();
	}

	private void OnEnable()
	{
		Physics.Raycast(base.t.position + Vector3.up, Vector3.down, out hit, 2f, 1);
		if (hit.distance != 0f)
		{
			base.t.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(base.t.forward, hit.normal));
			base.t.position = hit.point;
		}
		CrowdControl.instance.GetClosestEnemyToNormal(base.t.position, base.t.forward, 30f, 18f, out enemy);
		timer = 0f;
	}

	private void Update()
	{
		timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
		if (timer == 1f)
		{
			Deactivate();
		}
		if ((bool)enemy)
		{
			targetPosition = enemy.GetActualPosition();
			base.t.rotation = Quaternion.RotateTowards(base.t.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(base.t.position.DirTo(targetPosition), normal)), Time.deltaTime * 90f);
		}
		base.t.Translate(Vector3.forward * (20f * Time.deltaTime), Space.Self);
	}

	private void FixedUpdate()
	{
		Physics.Raycast(base.t.position + Vector3.up, Vector3.down, out hit, 1.5f, 1);
		if (hit.distance != 0f)
		{
			normal = hit.normal;
		}
		else
		{
			Deactivate();
		}
	}

	private void Deactivate()
	{
		QuickEffectsPool.Get("Poof", base.t.position, base.t.rotation).Play();
		base.gameObject.SetActive(value: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer != 9)
		{
			damage.dir = (-base.t.forward + Vector3.up / 2f).normalized;
			other.GetComponent<IDamageable<DamageData>>().Damage(damage);
			if (other.gameObject.layer == 10)
			{
				StyleRanking.instance.AddStylePoint(StylePointTypes.GroundShock);
			}
			QuickEffectsPool.Get("Arrow Hit", base.t.position, base.t.rotation).Play();
			CameraController.shake.Shake(1);
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDrawGizmos()
	{
		if ((bool)base.t)
		{
			Gizmos.DrawLine(base.t.position, targetPosition);
		}
	}
}
