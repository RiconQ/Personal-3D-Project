using System;
using System.Collections.Generic;
using UnityEngine;

public class ThrowedWeapon : PooledMonobehaviour
{
	public Weapon data;

	public string hitEffect = "Arrow Hit";

	public bool charged;

	public bool returnOnHit;

	public float gravity = -20f;

	public Vector3 meshRotation = new Vector3(0f, 0f, 90f);

	private int intDist;

	private float dist;

	private float speed = 35f;

	private Vector3 lastPos;

	private Transform tMesh;

	private RaycastHit hit;

	private BaseEnemy targetEnemy;

	public DamageData dmg = new DamageData();

	private List<BaseEnemy> targets = new List<BaseEnemy>();

	protected override void Awake()
	{
		base.Awake();
		tMesh = base.t.Find("Mesh");
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
		lastPos = base.t.position;
		dist = (intDist = 0);
		targets.Clear();
		Physics.Raycast(base.t.position, base.t.forward, out hit, 1.2f, 1);
		CrowdControl.instance.GetClosestEnemyToNormal(base.t.position, base.t.forward, 15f, 20f, out targetEnemy);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		targetEnemy = null;
	}

	private void Update()
	{
		base.t.Translate(0f, 0f, speed * Time.deltaTime);
		tMesh.Rotate(meshRotation * Time.deltaTime);
		if (!targetEnemy)
		{
			base.t.Rotate(Time.deltaTime * gravity, 0f, 0f);
		}
		else
		{
			base.t.rotation = Quaternion.RotateTowards(base.t.rotation, Quaternion.LookRotation(base.t.position.DirTo(targetEnemy.GetActualPosition())), Time.deltaTime * 80f);
		}
		dist += Time.deltaTime * speed;
	}

	private void FixedUpdate()
	{
		if ((int)dist * 2 != intDist)
		{
			intDist = (int)dist * 2;
			Physics.Linecast(lastPos, base.t.position, out hit, 1);
			Debug.DrawLine(lastPos, base.t.position, Color.cyan, 0.25f);
			lastPos = base.t.position;
		}
		if (dist > 40f || hit.distance != 0f)
		{
			Stop();
		}
	}

	private void Stop()
	{
		if (data.index == 3 && charged)
		{
			Game.player.weapons.PickWeapon(3);
			base.gameObject.SetActive(value: false);
			return;
		}
		if (hit.distance != 0f)
		{
			if (hit.collider.CompareTag("Wood") || (data.index == 2 && charged))
			{
				PooledWeapon pooledWeapon = QuickPool.instance.Get(data.prefabPickable, hit.point, Quaternion.LookRotation(base.t.forward)) as PooledWeapon;
				pooledWeapon.t.position += base.t.forward * -0.5f;
				pooledWeapon.t.Rotate(90f, 0f, 0f, Space.Self);
				pooledWeapon.SetKinematic(value: true);
				if (data.index == 2 && charged)
				{
					pooledWeapon.GetComponentInChildren<SpearLock>().Check();
				}
				QuickEffectsPool.Get(hitEffect, hit.point, Quaternion.LookRotation(hit.normal)).Play();
			}
			else
			{
				QuickPool.instance.Get(data.prefabPickable, hit.point + hit.normal * 0.5f, tMesh.rotation).rb.AddForceAndTorque(hit.normal * 5f, Vector3.one * 90f);
				QuickEffectsPool.Get(hitEffect, hit.point, Quaternion.LookRotation(hit.normal)).Play();
			}
		}
		else
		{
			Rigidbody rigidbody = QuickPool.instance.Get(data.prefabPickable, base.t.position, tMesh.rotation).rb;
			rigidbody.velocity = base.t.forward * speed;
			rigidbody.AddTorque(Vector3.one * 180f, ForceMode.Impulse);
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.layer == 9)
		{
			return;
		}
		switch (data.index)
		{
		case 0:
			switch (c.gameObject.layer)
			{
			case 10:
				dmg.dir = base.t.forward;
				c.GetComponent<IDamageable<DamageData>>().Damage(dmg);
				Debug.DrawRay(base.t.position, dmg.dir * 10f, Color.cyan, 2f);
				if (!BaseEnemy.lastDamaged.dead)
				{
					(QuickPool.instance.Get(data.prefabPickable, base.t.position, tMesh.rotation) as PooledWeapon).rb.AddForceAndTorque(Vector3.up * 8f, Vector3.one * 90f);
				}
				base.gameObject.SetActive(value: false);
				break;
			case 14:
				dmg.dir = base.t.forward;
				c.GetComponent<IDamageable<DamageData>>().Damage(dmg);
				Debug.DrawRay(base.t.position, dmg.dir * 10f, Color.cyan, 2f);
				(QuickPool.instance.Get(data.prefabPickable, base.t.position, tMesh.rotation) as PooledWeapon).rb.AddForceAndTorque(Vector3.up * 8f, Vector3.one * 90f);
				base.gameObject.SetActive(value: false);
				break;
			}
			break;
		case 1:
			dmg.dir = base.t.forward;
			c.GetComponent<IDamageable<DamageData>>().Damage(dmg);
			CameraController.shake.Shake(2);
			QuickEffectsPool.Get("Contact", base.t.position).Play();
			(QuickPool.instance.Get(data.prefabPickable, base.t.position, tMesh.rotation) as PooledWeapon).rb.AddForceAndTorque(Vector3.up * 10f, Vector3.one * 90f);
			base.gameObject.SetActive(value: false);
			break;
		case 3:
			switch (c.gameObject.layer)
			{
			case 10:
			{
				if (!c.TryGetComponent<BaseEnemy>(out var component2))
				{
					component2 = c.GetComponent<BodyCollider>().body.enemy;
				}
				if (targets.Contains(component2))
				{
					break;
				}
				targets.Add(component2);
				dmg.amount = targets.Count * 30;
				dmg.dir = base.t.forward;
				c.GetComponent<IDamageable<DamageData>>().Damage(dmg);
				CameraController.shake.Shake(2);
				QuickEffectsPool.Get("Contact", base.t.position).Play();
				CrowdControl.instance.GetClosestEnemy(base.t.position, out targetEnemy, targets);
				if (!charged)
				{
					if (!targetEnemy || targetEnemy.dead)
					{
						(QuickPool.instance.Get(data.prefabPickable, base.t.position, tMesh.rotation) as PooledWeapon).rb.AddForceAndTorque(-base.t.forward * 6f, Vector3.one * 90f);
						base.gameObject.SetActive(value: false);
						break;
					}
					Game.time.SlowMotion(0.2f);
					Vector3 vector2 = base.t.position.DirTo(targetEnemy.GetActualPosition());
					base.t.rotation = Quaternion.LookRotation(vector2);
					Debug.DrawRay(targetEnemy.GetActualPosition(), vector2 * 10f, Color.yellow, 2f);
				}
				else
				{
					Game.player.weapons.PickWeapon(3);
					base.gameObject.SetActive(value: false);
				}
				break;
			}
			case 14:
				dmg.dir = base.t.forward;
				c.GetComponent<IDamageable<DamageData>>().Damage(dmg);
				Debug.DrawRay(base.t.position, dmg.dir * 10f, Color.cyan, 2f);
				if (!charged)
				{
					(QuickPool.instance.Get(data.prefabPickable, base.t.position, tMesh.rotation) as PooledWeapon).rb.AddForceAndTorque(Vector3.up * 8f, Vector3.one * 90f);
				}
				else
				{
					Game.player.weapons.PickWeapon(3);
					base.gameObject.SetActive(value: false);
				}
				base.gameObject.SetActive(value: false);
				break;
			}
			break;
		case 2:
			switch (c.gameObject.layer)
			{
			case 10:
			{
				targetEnemy = null;
				if (!c.TryGetComponent<BaseEnemy>(out var component))
				{
					component = c.GetComponent<BodyCollider>().body.enemy;
				}
				if (targets.Contains(component))
				{
					break;
				}
				targets.Add(component);
				dmg.dir = base.t.forward;
				component.Damage(dmg);
				if (component.dead)
				{
					base.gameObject.SetActive(value: false);
					break;
				}
				CrowdControl.instance.GetClosestEnemyToNormal(base.t.position, base.t.forward, 90f, 24f, out targetEnemy, targets);
				if (!targetEnemy)
				{
					if (!BaseEnemy.lastDamaged.dead)
					{
						PooledWeapon obj = QuickPool.instance.Get(data.prefabPickable, base.t.position, tMesh.rotation) as PooledWeapon;
						Vector3 vector = base.t.position.DirTo(Game.player.tHead.position);
						vector += Vector3.up;
						vector.Normalize();
						obj.rb.AddForceAndTorque(vector * 8f, new Vector3(90f, -45f, 30f));
					}
					base.gameObject.SetActive(value: false);
				}
				break;
			}
			case 14:
				dmg.dir = base.t.forward;
				c.GetComponent<IDamageable<DamageData>>().Damage(dmg);
				Debug.DrawRay(base.t.position, dmg.dir * 10f, Color.cyan, 2f);
				(QuickPool.instance.Get(data.prefabPickable, base.t.position, tMesh.rotation) as PooledWeapon).rb.AddForceAndTorque(Vector3.up * 8f, Vector3.one * 90f);
				base.gameObject.SetActive(value: false);
				break;
			}
			break;
		}
	}
}
