using UnityEngine;

public class ArrowFlying : PooledMonobehaviour
{
	public float speed = 10f;

	public int type;

	public StylePointTypes stylePoint = StylePointTypes.AirKick;

	public GameObject[] typeObjects = new GameObject[3];

	private Vector3 targetPos;

	private RaycastHit hit;

	private DamageData dmg = new DamageData();

	private BaseEnemy enemy;

	private float dist;

	private int intDist;

	private Vector3 lastPos;

	public Transform tMesh;

	public StyleMove styleMove;

	private float accuracy;

	protected override void Awake()
	{
		base.Awake();
		tMesh = base.t.Find("Mesh").transform;
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		lastPos = base.t.position;
		dist = (intDist = 0);
		Physics.Raycast(base.t.position, base.t.forward, out hit, 1.2f, 1);
		accuracy = BowController.accuracy;
		CrowdControl.instance.GetClosestEnemyToNormal(base.t.position, base.t.forward, 15f, 20f, out enemy);
		if ((bool)enemy && enemy.isStrafing > 0f && enemy.enabled)
		{
			enemy = null;
		}
	}

	public void Setup(DamageData damage, int type)
	{
		damage.CopyTo(dmg);
		this.type = type;
		for (int i = 1; i < typeObjects.Length; i++)
		{
			typeObjects[i].SetActive(i == type);
		}
	}

	public void Setup(int newType, StyleMove style, float holding = 1f)
	{
		type = newType;
		for (int i = 1; i < typeObjects.Length; i++)
		{
			typeObjects[i].SetActive(i == type);
		}
		styleMove = style;
	}

	public void SetTypeAndStylePoint(int newType, StylePointTypes newSP, float damage = 40f)
	{
		type = newType;
		for (int i = 1; i < typeObjects.Length; i++)
		{
			typeObjects[i].SetActive(i == type);
		}
		stylePoint = newSP;
		dmg.amount = damage;
	}

	private void Update()
	{
		base.t.Translate(Vector3.forward * (Time.deltaTime * speed));
		if (!enemy)
		{
			base.t.Rotate(Vector3.right * (Time.deltaTime * 10f));
		}
		else
		{
			base.t.rotation = Quaternion.RotateTowards(base.t.rotation, Quaternion.LookRotation(base.t.position.DirTo(enemy.GetActualPosition())), Time.deltaTime * accuracy);
		}
		tMesh.Rotate(0f, 0f, 360f * Time.deltaTime, Space.Self);
		dist += Time.deltaTime * speed;
	}

	private void FixedUpdate()
	{
		if ((int)dist * 2 != intDist)
		{
			intDist = (int)dist * 2;
			if (Physics.Linecast(lastPos, base.t.position, out hit, 1))
			{
				QuickPool.instance.Get("Arrow", hit.point, base.t.rotation);
				QuickEffectsPool.Get("Arrow Hit", hit.point, Quaternion.LookRotation(hit.normal)).Play();
				Deactivate();
			}
			Debug.DrawLine(lastPos, base.t.position, Color.cyan, 0.25f);
			lastPos = base.t.position;
		}
		if (dist > 40f)
		{
			Deactivate();
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		switch (c.gameObject.layer)
		{
		case 10:
			dmg.dir = base.t.forward;
			c.GetComponent<IDamageable<DamageData>>().Damage(dmg);
			QuickEffectsPool.Get("Arrow Hit", base.t.position, Quaternion.LookRotation(base.t.forward)).Play();
			Deactivate();
			break;
		case 14:
			dmg.dir = base.t.forward;
			c.GetComponent<IDamageable<DamageData>>().Damage(dmg);
			if (c.gameObject.activeInHierarchy)
			{
				QuickPool.instance.Get("Arrow", c.ClosestPoint(base.t.position), base.t.rotation).t.SetParent(c.attachedRigidbody ? c.transform : null);
			}
			QuickEffectsPool.Get("Arrow Hit", c.ClosestPoint(base.t.position), Quaternion.LookRotation(-base.t.forward)).Play();
			Deactivate();
			break;
		}
	}

	private void Deactivate()
	{
		switch (type)
		{
		case 1:
			QuickPool.instance.Get("Explosion A2", base.t.position, Quaternion.LookRotation(base.t.forward));
			break;
		case 2:
			QuickPool.instance.Get("Bubble Trap", base.t.position, Quaternion.LookRotation(base.t.forward));
			break;
		}
		enemy = null;
		base.gameObject.SetActive(value: false);
	}
}
