using System;
using UnityEngine;

public class HunterEnemyState : BaseState
{
	private BaseEnemy enemy;

	private float timer;

	private int shotCount;

	private int state;

	private Vector3 posA;

	private Vector3 posB;

	private Vector3 nextPos;

	private RaycastHit hit;

	private Quaternion rotA;

	private Quaternion rotB;

	public HunterEnemyState(BaseEnemy e)
		: base(e.gameObject)
	{
		enemy = e;
	}

	public override void ExternalCall()
	{
	}

	public override void FirstCall()
	{
		enemy.agent.enabled = false;
		enemy.clldr.isTrigger = true;
		state = 0;
		shotCount = 0;
		if (enemy.lockJumpRotation)
		{
			timer = 0.75f;
			enemy.animator.Play("Predash", -1, 0f);
			enemy.t.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(enemy.t.position.DirTo(enemy.targetPosition), Vector3.up));
		}
		else
		{
			timer = 0f;
			enemy.animator.Play("Spider PreJump", -1, 0f);
			enemy.t.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(enemy.t.position.DirTo(enemy.targetPosition), Vector3.up));
		}
		posA = enemy.t.position;
		posB = enemy.targetPosition;
		rotA = enemy.t.rotation;
		rotB = Quaternion.LookRotation(Vector3.ProjectOnPlane(enemy.t.position.DirTo(enemy.tTarget.position), enemy.targetNormal), enemy.targetNormal);
	}

	public override void LastCall()
	{
		state = (shotCount = 0);
		enemy.agent.enabled = true;
		enemy.clldr.isTrigger = false;
		enemy.tMesh.localEulerAngles = new Vector3(-90f, 0f, 0f);
		enemy.tMesh.gameObject.SetActive(value: true);
		if (enemy.agent.isOnOffMeshLink)
		{
			enemy.agent.CompleteOffMeshLink();
		}
	}

	public override Type Tick()
	{
		switch (state)
		{
		case 0:
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 4f);
			if (timer == 1f)
			{
				timer = 0f;
				state = 1;
				enemy.clldr.enabled = false;
				enemy.tMesh.gameObject.SetActive(value: false);
				QuickEffectsPool.Get("Armless Jump", enemy.t.position, Quaternion.FromToRotation(enemy.t.up, enemy.t.position.DirTo(enemy.targetPosition))).Play();
			}
			break;
		case 1:
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 8f);
			if (timer == 1f)
			{
				timer = 0f;
				state = 2;
				enemy.clldr.enabled = true;
				enemy.t.position = enemy.targetPosition;
				enemy.t.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(enemy.t.position.DirTo(enemy.tTarget.position), enemy.targetNormal), enemy.targetNormal);
				enemy.tMesh.gameObject.SetActive(value: true);
				enemy.animator.Play("Idle 2", -1, 0f);
			}
			break;
		case 2:
			if (shotCount == 0)
			{
				timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 10f);
				if (timer != 1f)
				{
					break;
				}
				timer = 0f;
				posA = enemy.t.position + enemy.t.up;
				posB = posA.DirTo(enemy.tTarget.position);
				Physics.Raycast(posA, posB, out hit, 24f, 513);
				rotA = Quaternion.LookRotation(Vector3.ProjectOnPlane(enemy.t.position.DirTo(enemy.tTarget.position), enemy.targetNormal), enemy.targetNormal);
				enemy.t.rotation = rotA;
				if (hit.distance != 0f && hit.collider.gameObject.layer == 9)
				{
					shotCount++;
					if (CrowdControl.instance.GetToken(enemy))
					{
						QuickEffectsPool.Get("Warning", enemy.clldr.bounds.center, enemy.t.rotation).Play();
						enemy.animator.Play("Shoot", -1, 0f);
					}
				}
				else
				{
					shotCount++;
				}
				timer = 0f;
				break;
			}
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 0.5f);
			if (timer != 1f)
			{
				break;
			}
			if (!enemy.CheckJumpPosNearPlayer(ref enemy.targetPosition))
			{
				if (Physics.Raycast(enemy.t.position + enemy.t.up * 6f, Vector3.down, out hit, 20f, 1))
				{
					enemy.targetPosition = hit.point;
				}
				else
				{
					enemy.targetPosition = enemy.tTarget.position;
				}
			}
			enemy.t.position += enemy.t.up;
			enemy.t.LookAt(enemy.tTarget.position);
			return typeof(EnemyJumpState);
		}
		return null;
	}
}
