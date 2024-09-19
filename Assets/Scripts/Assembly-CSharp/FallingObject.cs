using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class FallingObject : MonoBehaviour
{
	private int state;

	private float gravity;

	private Transform t;

	private Rigidbody rb;

	private BoxCollider clldr;

	private NavMeshObstacle obstacle;

	private Vector3 startPos;

	private Vector3 targetPos;

	private Vector4 damageInfo;

	private DamageData damage = new DamageData();

	private RaycastHit hit;

	private Coroutine falling;

	public GameObject dependsOnObject;

	public LayerMask mask;

	public AudioClip sound;

	private void Awake()
	{
		t = base.transform;
		startPos = t.position;
		rb = GetComponent<Rigidbody>();
		clldr = GetComponent<BoxCollider>();
		obstacle = GetComponent<NavMeshObstacle>();
		damage.amount = 1000f;
		damage.newType = Game.style.basicMill;
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Combine(BreakableB.OnBreak, new Action<GameObject>(OnBreak));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Remove(BreakableB.OnBreak, new Action<GameObject>(OnBreak));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		if (clldr.isTrigger)
		{
			clldr.isTrigger = false;
		}
		if (!obstacle.enabled)
		{
			obstacle.enabled = true;
		}
		gravity = 20f;
		t.position = startPos;
		state = 0;
	}

	private void Update()
	{
		switch (state)
		{
		case 1:
			Physics.Raycast(t.position, Vector3.down, out hit, 100f, 1);
			targetPos = hit.point;
			targetPos.y += clldr.size.y / 2f;
			clldr.isTrigger = true;
			obstacle.enabled = false;
			gravity = -10f;
			state++;
			break;
		case 2:
			gravity += Time.deltaTime * 60f;
			t.position = Vector3.MoveTowards(t.position, targetPos, Time.deltaTime * gravity);
			if (t.position == targetPos)
			{
				state++;
			}
			break;
		case 3:
			clldr.isTrigger = false;
			obstacle.enabled = true;
			QuickEffectsPool.Get("Falling Object FX", hit.point + Vector3.up * 0.5f).Play();
			Game.sounds.PlayClipAtPosition(sound, 1f, t.position);
			state++;
			break;
		}
	}

	private void OnBreak(GameObject obj)
	{
		if (state == 0 && (bool)dependsOnObject && obj == dependsOnObject)
		{
			state++;
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (clldr.isTrigger)
		{
			if (c.gameObject.layer == 9)
			{
				PlayerController.instance.Damage(new Vector4(0f, -1f, 0f, 999f));
			}
			else if ((int)mask == ((int)mask | (1 << c.gameObject.layer)))
			{
				Game.time.SlowMotion(0.5f, 0.3f, 0.05f);
				damage.dir = t.position.DirTo(c.transform.position).With(null, 0f);
				c.GetComponent<IDamageable<DamageData>>().Damage(damage);
			}
		}
	}
}
