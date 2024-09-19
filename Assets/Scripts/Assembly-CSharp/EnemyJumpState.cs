using System;
using UnityEngine;

public class EnemyJumpState : BaseState
{
	private BaseEnemy enemy;

	private bool wAttack;

	private float timer;

	private float speed;

	private float dist;

	private Vector3 posA;

	private Vector3 posB;

	private Vector3 nextPos;

	private Vector3 normal = Vector3.up * 4f;

	private float rHeight;

	public EnemyJumpState(BaseEnemy e)
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
		if (enemy.t.up.y.Abs() < 0.5f)
		{
			enemy.t.position += enemy.t.up;
		}
		if (enemy.lockJumpRotation)
		{
			enemy.lockJumpRotation = false;
		}
		else
		{
			enemy.t.LookAt(enemy.targetPosition.With(null, enemy.t.position.y));
		}
		wAttack = enemy.hasLandAttack & (Vector3.Distance(Game.player.t.position, enemy.targetPosition) < 3f);
		enemy.animator.Play(wAttack ? "Jump Attack" : "Jump", -1, 0f);
		enemy.clldr.isTrigger = true;
		timer = 0f;
		rHeight = UnityEngine.Random.Range(3f, 4f);
		posA = enemy.t.position;
		posB = enemy.targetPosition;
		Debug.DrawLine(posA, posB, Color.blue, 2f);
		Debug.DrawRay(posA, Vector3.up, Color.blue, 2f);
		Debug.DrawRay(posB, Vector3.up, Color.blue, 2f);
		dist = Vector3.Distance(posA, posB);
		speed = 1.25f;
	}

	public override void LastCall()
	{
		if (timer != 1f)
		{
			enemy.animator.Play(wAttack ? "Land Attack" : "Land", -1, 0f);
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
		if (enemy.staggered)
		{
			return null;
		}
		timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
		nextPos = Vector3.Lerp(posA, posB, timer);
		nextPos.y += Mathf.Sin(timer * (float)Math.PI) * (4f + (posA.y - posB.y).Abs() / 12f * rHeight);
		enemy.t.position = nextPos;
		enemy.tMesh.localEulerAngles = new Vector3(-90f + Mathf.Lerp(-10f, 10f, timer), 0f, 0f);
		if (timer == 1f)
		{
			enemy.OnLanded();
			enemy.animator.Play(wAttack ? "Land Attack" : "Land", -1, 0f);
			enemy.tMesh.localEulerAngles = new Vector3(-90f, 0f, 0f);
			enemy.actionTime = (wAttack ? enemy.landAttackRecover : enemy.landRecover);
			return typeof(EnemyActionState);
		}
		return null;
	}
}
