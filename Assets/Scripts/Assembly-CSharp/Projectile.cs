using System;
using UnityEngine;

public class Projectile : PooledMonobehaviour
{
	public float distance = 20f;

	public float speed = 30f;

	public float pitch = 20f;

	public bool homing;

	private RaycastHit hit;

	public Transform tMesh;

	public DamageData dmg;

	public LayerMask triggerMask;

	private Vector3 halfSize = new Vector3(0.15f, 0.15f, 0.5f);

	private Collider[] colliders = new Collider[1];

	private float dist;

	private int intDist;

	private Vector3 lastPos;

	protected override void Awake()
	{
		base.Awake();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnActualEnable()
	{
		base.OnActualEnable();
		float num = Vector3.Distance(Game.player.tHead.position, base.t.position);
		base.t.Rotate((0f - num / 6f) * 3f, 0f, 0f, Space.Self);
		lastPos = base.t.position;
		dist = (intDist = 0);
		Physics.Raycast(base.t.position, base.t.forward, out hit, 1.2f, 1);
	}

	private void Update()
	{
		base.t.Translate(Vector3.forward * (Time.deltaTime * speed));
		if (!homing)
		{
			if (pitch != 0f)
			{
				base.t.Rotate(Vector3.right * (Time.deltaTime * pitch));
			}
		}
		else
		{
			base.t.rotation = Quaternion.RotateTowards(base.t.rotation, Quaternion.LookRotation(base.t.position.DirTo(Game.player.t.position)), Time.deltaTime * pitch);
		}
		tMesh.Rotate(0f, 0f, 360f * Time.deltaTime);
		dist += Time.deltaTime * speed;
		if ((int)dist * 2 != intDist)
		{
			intDist = (int)dist * 2;
			Physics.OverlapBoxNonAlloc(base.t.position, halfSize, colliders, base.t.rotation, triggerMask);
			if (colliders[0] != null)
			{
				if (colliders[0].gameObject.layer == 9)
				{
					PlayerController.instance.Damage(base.t.forward);
				}
				colliders[0] = null;
				QuickEffectsPool.Get("Arrow Hit", base.t.position, base.t.rotation).Play(-1f, 10);
				base.gameObject.SetActive(value: false);
			}
			else if (Physics.Linecast(lastPos, base.t.position, out hit, 1))
			{
				Debug.DrawLine(lastPos, base.t.position, Color.cyan, 0.25f);
				QuickEffectsPool.Get("Arrow Hit", hit.point, Quaternion.LookRotation(hit.normal)).Play(-1f, 10);
				base.gameObject.SetActive(value: false);
			}
			lastPos = base.t.position;
		}
		if (dist > 40f)
		{
			QuickEffectsPool.Get("Arrow Hit", base.t.position, base.t.rotation).Play();
			base.gameObject.SetActive(value: false);
		}
	}
}
