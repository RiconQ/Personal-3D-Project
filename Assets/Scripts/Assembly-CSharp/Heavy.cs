using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heavy : BaseEnemy
{
	private bool playerGrabbed;

	private int attackType;

	private float timer;

	private float grabTimer;

	public Transform tPlayerPivot;

	public AnimationCurve curveGrab;

	public GameObject _pooledPrefab;

	public List<DamageType> playerMoves = new List<DamageType>(3);

	protected override void Awake()
	{
		base.Awake();
		base.agent.updateRotation = false;
		Dictionary<Type, BaseState> states = new Dictionary<Type, BaseState>
		{
			{
				typeof(EnemyIdleState),
				new EnemyIdleState(this)
			},
			{
				typeof(EnemyFollowState),
				new EnemyFollowState(this)
			},
			{
				typeof(EnemyActionState),
				new EnemyActionState(this)
			},
			{
				typeof(EnemyJumpState),
				new EnemyJumpState(this)
			},
			{
				typeof(EnemyAltJumpState),
				new EnemyAltJumpState(this)
			},
			{
				typeof(HeavyDashState),
				new HeavyDashState(this)
			}
		};
		base.stateMachine.SetStates(states);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		timer = UnityEngine.Random.Range(1.5f, 3f);
		grabTimer = MyRandom.Range(1f, 3f);
		actionTime = 0.75f;
		base.stateMachine.SwitchState(typeof(EnemyActionState));
	}

	public override void Reset()
	{
		base.Reset();
		playerMoves.Clear();
	}

	public override void Kick(Vector3 dir)
	{
	}

	public override void Die(DamageData damage)
	{
		base.Die(damage);
		CameraController.shake.Shake(1);
	}

	public override void AnimationEvent(int index)
	{
		base.AnimationEvent(index);
		switch (index)
		{
		case 0:
			if (Game.player.ViewAnglePlane(base.t.position) < 90f)
			{
				base.t.LookAt(tTarget.position.With(null, base.t.position.y));
			}
			break;
		case 1:
		{
			QuickEffectsPool.Get("HeavySlam", base.t.position, Quaternion.identity).Play();
			CameraController.shake.Shake(1);
			timer = 0.5f;
			attackType = 2;
			Collider[] array = new Collider[5];
			Physics.OverlapBoxNonAlloc(base.t.position, new Vector3(4f, 0.5f, 4f), array, base.t.rotation, 1536);
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] != null))
				{
					continue;
				}
				switch (array[i].gameObject.layer)
				{
				case 9:
					Game.player.Damage((base.t.position.DirTo(Game.player.t.position) + Vector3.up).normalized);
					break;
				case 10:
					if (array[i].gameObject != base.gameObject)
					{
						DamageData damageData = new DamageData();
						damageData.knockdown = true;
						damageData.amount = 40f;
						damageData.dir = base.t.position.DirTo(array[i].bounds.center + Vector3.up).normalized;
						damageData.newType = Game.style.basicBluntHit;
						array[i].GetComponent<IDamageable<DamageData>>().Damage(damageData);
					}
					break;
				}
				array[i] = null;
			}
			break;
		}
		case 2:
			CameraController.shake.Shake();
			QuickPool.instance.Get(_pooledPrefab, base.t);
			timer = UnityEngine.Random.Range(1f, 2f);
			break;
		case 4:
			if (playerGrabbed)
			{
				StopAllCoroutines();
				Game.player.Activate();
				Game.player.MakeInvinsible(value: false);
				Game.player.SetKinematic(value: false);
				Game.player.weapons.gameObject.SetActive(value: true);
				Game.player.Damage(base.t.forward);
				Game.player.airControlBlock = 0.2f;
				Game.player.rb.AddForce((base.t.forward + base.t.up).normalized * 15f, ForceMode.Impulse);
				playerGrabbed = false;
			}
			break;
		case 3:
		case 5:
			break;
		}
	}

	public override void RunningUpdate()
	{
		base.RunningUpdate();
		float sqrMagnitude = base.agent.velocity.sqrMagnitude;
		if (sqrMagnitude > 4f)
		{
			base.animator.speed = 0.1f + sqrMagnitude / (base.agent.speed * base.agent.speed);
			base.t.rotation = Quaternion.RotateTowards(base.t.rotation, Quaternion.LookRotation(base.agent.velocity.normalized), Time.deltaTime * (base.agent.velocity.magnitude / base.agent.speed) * 180f);
		}
		else
		{
			base.animator.speed = Mathf.MoveTowards(base.animator.speed, 1f, Time.deltaTime);
		}
	}

	private bool DirectJump()
	{
		Physics.Raycast(tHead.position, tHead.position.DirTo(tTarget.position), out hit, 32f, 513);
		if (hit.transform == tTarget)
		{
			if (Physics.Raycast(hit.point + hit.normal, Vector3.down, out hit, 2f, 1))
			{
				Debug.Log("DJ");
				targetPosition = hit.point;
				TryJump(targetPosition);
			}
			return true;
		}
		return false;
	}

	private IEnumerator GrabingThePlayer()
	{
		playerGrabbed = true;
		Game.player.weapons.DropCurrentWeapon(Game.player.tHead.forward);
		Game.player.weapons.gameObject.SetActive(value: false);
		Game.player.Deactivate();
		Game.player.SetKinematic(value: true);
		Game.player.MakeInvinsible(value: true);
		CameraController.shake.Shake(2);
		Vector3 startPos = Game.player.t.position;
		float timer = 0f;
		while (base.stateMachine.CurrentIs(typeof(EnemyActionState)))
		{
			if (timer != 1f)
			{
				timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 2f);
				Game.player.t.position = Vector3.LerpUnclamped(startPos, tPlayerPivot.position, curveGrab.Evaluate(timer));
			}
			else
			{
				Game.player.t.position = tPlayerPivot.position;
			}
			Game.player.camController.Angle(Mathf.Sin(timer * (float)Math.PI) * 12f);
			Game.player.mouseLook.LookAtSmooth(tHead.position);
			if (Game.player.KickPressed())
			{
				base.animator.Play("Damage", -1, 0f);
				Game.player.Activate();
				Game.player.SetKinematic(value: false);
				Game.player.MakeInvinsible(value: false);
				Game.player.weapons.gameObject.SetActive(value: true);
				Game.player.weapons.kickController.QuickKick();
				break;
			}
			yield return null;
		}
	}

	public override void ActualDamage(DamageData damage)
	{
		base.ActualDamage(damage);
		timer -= 0.3f;
	}

	private void Update()
	{
		if (!tTarget || !base.agent.enabled || base.stateMachine.CurrentIs(typeof(EnemyActionState)) || CheckForOffMeshLinks())
		{
			return;
		}
		if (base.distVertical.Abs() < 4f)
		{
			targetPosition = tTarget.position + base.t.right * (Mathf.Sin((float)base.randomSeed + Time.timeSinceLevelLoad / 2f) * (base.distGrounded / 2f));
		}
		else
		{
			targetPosition = base.t.position;
		}
		UpdateTargetDistances();
		if (base.distGrounded > 3f && Vector3.Angle(base.t.forward, base.t.position.DirTo(tTarget.position.With(null, base.t.position.y))).InRange(80f, 140f))
		{
			bool flag = Vector3.Dot(base.t.right, base.t.position.DirTo(tTarget.position.With(null, base.t.position.y))) < 0f;
			ActionStateWithAnim(flag ? "Turn Left" : "Turn Right");
			base.t.Rotate(0f, 90 * ((!flag) ? 1 : (-1)), 0f);
			actionTime = 0.5f;
		}
		else if (timer != 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime * (float)((!Game.player.isDamaged) ? 1 : 2));
		}
		else if (base.distVertical.Abs() < 4f)
		{
			if (base.distGrounded < 24f && CrowdControl.instance.GetToken(this))
			{
				Physics.Raycast(tHead.position, tHead.position.DirTo(Game.player.tHead.position), out hit, 24f, 513);
				if (hit.transform == tTarget)
				{
					Warning();
					base.t.LookAt(tTarget.position.With(null, base.t.position.y));
					base.animator.Play("Attack", -1, 0f);
					actionTime = 1.5f;
					base.stateMachine.SwitchState(typeof(EnemyActionState));
					timer = UnityEngine.Random.Range(2f, 4f);
				}
				else if (!DirectJump())
				{
					CheckViewPoints();
					timer = UnityEngine.Random.Range(1f, 2f);
				}
			}
		}
		else
		{
			if (CheckJumpPosAtPosition(ref targetPosition, tTarget.position, 2f, 20f, 20f))
			{
				base.stateMachine.SwitchState(typeof(EnemyAltJumpState));
			}
			timer = UnityEngine.Random.Range(1f, 2f);
		}
	}
}
