using UnityEngine;

public class FlyingDoor : MonoBehaviour
{
	private Transform t;

	private DamageData damage = new DamageData();

	private float timer;

	private Vector3 dir;

	private void Awake()
	{
		t = base.transform;
	}

	private void OnEnable()
	{
		dir = PlayerController.instance.tHead.forward;
		timer = 0f;
	}

	private void Update()
	{
		if (timer != 1f)
		{
			t.Translate(dir * 20f * Time.deltaTime, Space.World);
			t.Rotate(Vector3.forward * (1440f * Time.deltaTime), Space.Self);
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
			if (timer == 1f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer != 9)
		{
			other.GetComponent<IDamageable<DamageData>>().Damage(damage);
			base.gameObject.SetActive(value: false);
		}
	}
}
