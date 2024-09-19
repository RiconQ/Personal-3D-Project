using System;
using UnityEngine;

public class EnemyWalljumpState : BaseState
{
	private BaseEnemy enemy;

	private RaycastHit hit;

	private float timer;

	private float speed;

	private int state;

	private Vector3 posA;

	private Vector3 posB;

	private Vector3 nextPos;

	private Vector3 normal = Vector3.up * 4f;

	private float rHeight;

	public EnemyWalljumpState(BaseEnemy e)
		: base(e.gameObject)
	{
		enemy = e;
	}

	public override void ExternalCall()
	{
	}

	public override void FirstCall()
	{
		enemy.t.LookAt(enemy.targetNormal.With(null, enemy.t.position.y));
		state = 0;
		timer = 0f;
		enemy.animator.Play("Dash", -1, 0f);
		enemy.clldr.isTrigger = true;
		enemy.agent.enabled = false;
	}

	public override void LastCall()
	{
		if (timer != 1f)
		{
			enemy.animator.Play("Land Attack", -1, 0f);
		}
		enemy.agent.enabled = true;
		enemy.clldr.isTrigger = false;
		enemy.tMesh.localEulerAngles = new Vector3(-90f, 0f, 0f);
		enemy.CheckLink();
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
				enemy.tMesh.gameObject.SetActive(value: false);
				QuickEffectsPool.Get("Armless Jump", enemy.t.position, Quaternion.FromToRotation(enemy.t.up, enemy.t.position.DirTo(enemy.targetNormal))).Play();
				enemy.t.position = enemy.targetNormal;
			}
			break;
		case 1:
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 8f);
			if (timer != 1f)
			{
				break;
			}
			timer = 0f;
			state = 2;
			rHeight = UnityEngine.Random.Range(3f, 4f);
			Physics.Raycast(enemy.t.position, enemy.t.position.DirTo(enemy.tTarget.position), out hit, 20f, 513);
			if (hit.distance > 6f && hit.collider.gameObject.layer == 9)
			{
				Physics.Raycast(hit.point, Vector3.down, out hit, 4f, 1);
				if (hit.distance != 0f)
				{
					enemy.CheckNavMeshPos(ref enemy.targetPosition, hit.point);
				}
			}
			posA = enemy.t.position;
			posB = enemy.targetPosition;
			speed = 1.25f;
			enemy.t.LookAt(enemy.targetPosition.With(null, enemy.t.position.y));
			enemy.tMesh.gameObject.SetActive(value: true);
			enemy.animator.Play("Jump Attack", -1, 0f);
			break;
		case 2:
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
			nextPos = Vector3.Lerp(posA, posB, timer);
			nextPos.y += Mathf.Sin(timer * (float)Math.PI) * (4f + (posA.y - posB.y).Abs() / 12f * rHeight);
			enemy.t.position = nextPos;
			enemy.tMesh.localEulerAngles = new Vector3(-90f + Mathf.Lerp(-10f, 10f, timer), 0f, 0f);
			if (timer == 1f)
			{
				enemy.OnLanded();
				enemy.animator.Play("Land Attack", -1, 0f);
				enemy.tMesh.localEulerAngles = new Vector3(-90f, 0f, 0f);
				enemy.actionTime = enemy.landAttackRecover;
				return typeof(EnemyActionState);
			}
			break;
		}
		return null;
	}
}
