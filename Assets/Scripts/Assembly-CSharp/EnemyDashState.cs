using System;
using UnityEngine;

public class EnemyDashState : BaseState
{
	private BaseEnemy enemy;

	private Transform tParticle;

	private float timer;

	private float speed;

	private int state;

	private Vector3 posA;

	private Vector3 posB;

	private Vector3 nextPos;

	public EnemyDashState(BaseEnemy e)
		: base(e.gameObject)
	{
		enemy = e;
		tParticle = enemy.particleDash.transform;
	}

	public override void ExternalCall()
	{
	}

	public override void FirstCall()
	{
		enemy.agent.enabled = false;
		enemy.clldr.enabled = false;
		if (enemy.t.up.y.Abs() < 0.5f)
		{
			enemy.t.position += enemy.t.up;
		}
		tParticle.position = enemy.t.position;
		enemy.tMesh.gameObject.SetActive(value: false);
		enemy.particleDash.Play();
		enemy.PlaySound(enemy.sounds.Dash);
		timer = 0f;
		speed = 4f;
		state = 0;
		posA = enemy.t.position;
		posB = enemy.targetPosition;
	}

	public override void LastCall()
	{
		enemy.agent.enabled = true;
		enemy.clldr.enabled = true;
		enemy.t.rotation = Quaternion.LookRotation(posA.DirTo(posB).With(null, 0f));
		enemy.tMesh.localEulerAngles = new Vector3(-90f, 0f, 0f);
		enemy.tMesh.gameObject.SetActive(value: true);
		enemy.particleDash.Stop();
		if (enemy.GetType() == typeof(Shieldman))
		{
			enemy.animator.Play("Slash", -1, 0f);
		}
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
				state = 1;
				timer = 0f;
			}
			break;
		case 1:
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
			nextPos = Vector3.Lerp(posA, posB, timer);
			enemy.t.position = nextPos;
			tParticle.position = nextPos;
			if (timer == 1f)
			{
				enemy.actionTime = 0.5f;
				enemy.OnDashed();
				return typeof(EnemyActionState);
			}
			break;
		}
		return null;
	}
}
