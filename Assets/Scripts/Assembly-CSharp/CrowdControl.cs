using System;
using System.Collections.Generic;
using UnityEngine;

public class CrowdControl : MonoBehaviour
{
	public static Action OnAllEnemiesDead = delegate
	{
	};

	public static Action OnLastActiveDied = delegate
	{
	};

	public static CrowdControl instance;

	public static BaseEnemy lastAttacked;

	public static BaseEnemy current;

	public static List<BaseEnemy> allEnemies = new List<BaseEnemy>(50);

	public List<CrowdToken> tokens = new List<CrowdToken>(0);

	public static float curTimer;

	public int activeEnemies { get; private set; }

	public int deadEnemies { get; private set; }

	private void Awake()
	{
		instance = this;
		tokens.Add(new CrowdToken());
		Game.OnAnyLevelUnloaded = (Action)Delegate.Combine(Game.OnAnyLevelUnloaded, new Action(OnSceneLoaded));
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Combine(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(OnEnemyDie));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		Game.OnAnyLevelUnloaded = (Action)Delegate.Remove(Game.OnAnyLevelUnloaded, new Action(OnSceneLoaded));
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Remove(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(OnEnemyDie));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		activeEnemies = 0;
		deadEnemies = 0;
	}

	public bool GetToken(BaseEnemy e)
	{
		if (Game.player.rb.isKinematic)
		{
			return false;
		}
		foreach (CrowdToken token in tokens)
		{
			if (token.actor == e)
			{
				return false;
			}
		}
		foreach (CrowdToken token2 in tokens)
		{
			if (token2.actor == null)
			{
				token2.Setup(e);
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		activeEnemies = 0;
		foreach (CrowdToken token in tokens)
		{
			if ((bool)token.actor)
			{
				token.Tick();
			}
		}
		if (curTimer != 0f)
		{
			curTimer = Mathf.MoveTowards(curTimer, 0f, Time.deltaTime);
		}
		else if ((bool)current && (current.dead || !current.tTarget || !current.isActiveAndEnabled))
		{
			current = null;
		}
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if ((bool)allEnemy.tTarget && !allEnemy.dead)
			{
				activeEnemies++;
				if (allEnemy.isActiveAndEnabled && !current)
				{
					current = allEnemy;
					curTimer = 0.8f;
				}
			}
		}
	}

	private void OnEnemyDie(BaseEnemy e)
	{
		deadEnemies++;
		if (deadEnemies == allEnemies.Count)
		{
			if (Game.player.ViewAngle(e.GetActualPosition()) < 45f)
			{
				Physics.Raycast(Game.player.tHead.position, Game.player.tHead.position.DirTo(e.GetActualPosition()), out var hitInfo, 20f, 1025);
				if (hitInfo.distance != 0f && hitInfo.collider.gameObject.layer == 10)
				{
					Game.time.SlowMotion(0.25f, 0.5f, 0.1f);
				}
			}
			if (OnAllEnemiesDead != null)
			{
				OnAllEnemiesDead();
			}
		}
		if (activeEnemies <= 0)
		{
			return;
		}
		activeEnemies = 0;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if ((bool)allEnemy.tTarget && !allEnemy.dead)
			{
				activeEnemies++;
				if (allEnemy.isActiveAndEnabled && !current)
				{
					current = allEnemy;
					curTimer = 0.8f;
				}
			}
		}
		if (activeEnemies == 0 && OnLastActiveDied != null)
		{
			OnLastActiveDied();
		}
	}

	private void OnSceneLoaded()
	{
		if (allEnemies.Count > 0)
		{
			allEnemies.Clear();
		}
	}

	public void KillTheRest()
	{
		DamageData damageData = new DamageData();
		damageData.amount = 999f;
		damageData.dir = Vector3.up;
		damageData.newType = Game.style.basicBluntHit;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (!allEnemy.dead)
			{
				allEnemy.Damage(damageData);
			}
		}
	}

	public Vector3 GetClosestDirectionToNormal(Vector3 pos, Vector3 normal, float maxAngle = 90f, float maxDist = 18f)
	{
		Vector3 vector = normal;
		float num = maxAngle;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (allEnemy.dead)
			{
				continue;
			}
			Vector3 actualPosition = allEnemy.GetActualPosition();
			Vector3 direction = pos.DirTo(actualPosition);
			float num2 = Vector3.Distance(pos, actualPosition);
			if (!(num2 > maxDist) && !Physics.Raycast(pos, direction, num2, 1))
			{
				direction = pos.DirTo(actualPosition);
				float num3 = Vector3.Angle(direction, normal);
				if (num3 < num)
				{
					num = num3;
					vector = direction;
				}
			}
		}
		return vector.normalized;
	}

	public void GetClosestDirectionsToNormal(Vector3[] results, Vector3 pos, Vector3 normal, float maxAngle, float maxDist)
	{
		int num = 0;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (allEnemy.dead || num >= results.Length)
			{
				continue;
			}
			Vector3 actualPosition = allEnemy.GetActualPosition();
			Vector3 direction = pos.DirTo(actualPosition);
			float num2 = Vector3.Distance(pos, actualPosition);
			if (!(num2 > maxDist) && !Physics.Raycast(pos, direction, num2, 1))
			{
				direction = pos.DirTo(actualPosition);
				if (Vector3.Angle(direction, normal) < maxAngle)
				{
					results[num] = direction.normalized;
					num = num.Next(results.Length);
					Debug.DrawLine(pos, actualPosition, Color.blue, 1f);
				}
			}
		}
	}

	public bool GetClosestEnemyToNormal(Vector3 pos, Vector3 normal, float maxAngle, float maxDist, out BaseEnemy enemy, bool onlyKnocked = false)
	{
		enemy = null;
		float num = maxAngle;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (allEnemy.dead || (onlyKnocked && allEnemy.isActiveAndEnabled))
			{
				continue;
			}
			Vector3 actualPosition = allEnemy.GetActualPosition();
			Vector3 vector = pos.DirTo(actualPosition);
			float num2 = Vector3.Distance(pos, actualPosition);
			if (!(num2 > maxDist) && !Physics.Raycast(pos, vector, num2, 1))
			{
				float num3 = Vector3.Angle(vector, normal);
				if (num3 < num)
				{
					num = num3;
					enemy = allEnemy;
				}
			}
		}
		return enemy != null;
	}

	public bool GetClosestEnemyToNormal(Vector3 pos, Vector3 normal, float maxAngle, float maxDist, out BaseEnemy enemy, List<BaseEnemy> exclude)
	{
		enemy = null;
		float num = maxAngle;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (allEnemy.dead)
			{
				continue;
			}
			bool flag = false;
			foreach (BaseEnemy item in exclude)
			{
				if (item == allEnemy)
				{
					flag = true;
				}
			}
			if (flag)
			{
				continue;
			}
			Vector3 actualPosition = allEnemy.GetActualPosition();
			Vector3 vector = pos.DirTo(actualPosition);
			float num2 = Vector3.Distance(pos, actualPosition);
			if (!(num2 > maxDist) && !Physics.Raycast(pos, vector, num2, 1))
			{
				float num3 = Vector3.Angle(vector, normal);
				if (num3 < num)
				{
					num = num3;
					enemy = allEnemy;
				}
			}
		}
		return enemy != null;
	}

	public bool GetEnemiesInRange(List<BaseEnemy> enemies, Vector3 pos, float radius, int count = 0)
	{
		enemies.Clear();
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (!allEnemy.dead && Vector3.Distance(pos, allEnemy.GetActualPosition()) < radius)
			{
				enemies.Add(allEnemy);
				Debug.DrawLine(pos, allEnemy.GetActualPosition(), Color.green, 1f);
				if (count > 0 && enemies.Count == count)
				{
					break;
				}
			}
		}
		return enemies.Count > 0;
	}

	public void GetClosestEnemies(List<BaseEnemy> enemies, Vector3 pos, Vector3 normal, float maxDist, float cutAngle = 90f, bool onlyActive = true)
	{
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (allEnemy.dead || (onlyActive && !allEnemy.isActiveAndEnabled))
			{
				continue;
			}
			Vector3 actualPosition = allEnemy.GetActualPosition();
			float num = Vector3.Distance(pos, actualPosition);
			if (!(num > maxDist))
			{
				Vector3 vector = pos.DirTo(actualPosition);
				if (!(Vector3.Angle(vector, normal) > cutAngle) && !Physics.Raycast(pos, vector, num, 1))
				{
					enemies.Add(allEnemy);
				}
			}
		}
		enemies.Sort((BaseEnemy a, BaseEnemy b) => Vector2.Distance(pos, a.t.position).CompareTo(Vector2.Distance(pos, b.t.position)));
	}

	public void GetClosestEnemiesToNormal(List<BaseEnemy> enemies, Vector3 pos, Vector3 normal, float maxAngle, float maxDist, int count)
	{
		int num = 0;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (allEnemy.dead)
			{
				continue;
			}
			Vector3 actualPosition = allEnemy.GetActualPosition();
			Vector3 vector = pos.DirTo(actualPosition);
			float num2 = Vector3.Distance(pos, actualPosition);
			if (!(num2 > maxDist) && !Physics.Raycast(pos, vector, num2, 1) && Vector3.Angle(vector, normal) < maxAngle)
			{
				enemies.Add(allEnemy);
				Debug.DrawLine(allEnemy.GetActualPosition(), pos, Color.yellow, 3f);
				num++;
				if (num >= count)
				{
					break;
				}
			}
		}
	}

	public Transform GetClosestEnemyTransform(Vector3 pos)
	{
		Transform result = null;
		float num = float.PositiveInfinity;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (!allEnemy.dead)
			{
				float sqrMagnitude = ((allEnemy.isActiveAndEnabled ? allEnemy.t.position : allEnemy.body.rb.position) - pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = (allEnemy.isActiveAndEnabled ? allEnemy.t : allEnemy.body.rb.transform);
				}
			}
		}
		return result;
	}

	public BaseEnemy GetClosestEnemy(Vector3 pos, float maxDist = float.PositiveInfinity)
	{
		BaseEnemy result = null;
		float num = maxDist * maxDist;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (!allEnemy.dead)
			{
				float sqrMagnitude = (allEnemy.GetActualPosition() - pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = allEnemy;
				}
			}
		}
		return result;
	}

	public void GetClosestEnemy(Vector3 pos, out BaseEnemy enemy, List<BaseEnemy> exclude, float maxDist = 16f)
	{
		enemy = null;
		float num = maxDist * maxDist;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (allEnemy.dead || (!allEnemy.isActiveAndEnabled && !allEnemy.body.isActiveAndEnabled))
			{
				continue;
			}
			bool flag = false;
			foreach (BaseEnemy item in exclude)
			{
				if (item == allEnemy)
				{
					flag = true;
					break;
				}
			}
			if (!flag && !(Game.player.ViewAngle(allEnemy.GetActualPosition()) > 60f))
			{
				float sqrMagnitude = ((allEnemy.isActiveAndEnabled ? allEnemy.t.position : allEnemy.body.rb.position) - pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					enemy = allEnemy;
				}
			}
		}
	}

	public void GetClosestEnemy(Vector3 pos, out BaseEnemy enemy, BaseEnemy exlude, float maxDist = 16f)
	{
		enemy = null;
		float num = maxDist * maxDist;
		foreach (BaseEnemy allEnemy in allEnemies)
		{
			if (!allEnemy.dead && (allEnemy.isActiveAndEnabled || allEnemy.body.isActiveAndEnabled) && !(allEnemy == exlude) && !(Game.player.ViewAngle(allEnemy.GetActualPosition()) > 75f))
			{
				float sqrMagnitude = ((allEnemy.isActiveAndEnabled ? allEnemy.t.position : allEnemy.body.rb.position) - pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					enemy = allEnemy;
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		for (int i = 0; i < allEnemies.Count; i++)
		{
			Gizmos.color = (allEnemies[i].tTarget ? (Color.green / 2f) : Color.black);
			Gizmos.DrawSphere(allEnemies[i].t.position, 0.5f);
		}
		for (int j = 0; j < tokens.Count; j++)
		{
			if ((bool)tokens[j].actor)
			{
				Gizmos.DrawIcon(tokens[j].actor.GetActualPosition(), "EnemyToken.png");
			}
		}
	}
}
