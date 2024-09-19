using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	public List<SpawnerEntry> entries = new List<SpawnerEntry>();

	public PrefabsCollection enemiesCollection;

	public Door door;

	public GameObject objBarrier;

	public int maxAtOnce = 1;

	public bool arenaFight;

	private Transform t;

	private int count;

	private int deadCount;

	private float timer;

	private Collider clldr;

	private Coroutine spawning;

	private List<BaseEnemy> enemies = new List<BaseEnemy>(10);

	private void Awake()
	{
		t = base.transform;
		clldr = GetComponent<Collider>();
		for (int i = 0; i < entries.Count; i++)
		{
			BaseEnemy component = UnityEngine.Object.Instantiate(enemiesCollection.prefabs[entries[i].index], entries[i].pos, Quaternion.identity).GetComponent<BaseEnemy>();
			enemies.Add(component);
			component.ManualReset = true;
			if ((bool)entries[i].shieldDamageType)
			{
				component.shieldDamageType = entries[i].shieldDamageType;
				component.shield.gameObject.SetActive(value: true);
			}
			component.gameObject.SetActive(value: false);
		}
		if ((bool)door)
		{
			door.SetupDeathLock(entries.Count);
		}
		if ((bool)objBarrier)
		{
			objBarrier.SetActive(value: true);
		}
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Combine(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(CheckEnemy));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Remove(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(CheckEnemy));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].ResetPositionAndRotation();
			enemies[i].DeactivateEnemy();
		}
		if ((bool)door)
		{
			door.UpdateDeathLock(0);
		}
		if ((bool)objBarrier)
		{
			objBarrier.SetActive(value: true);
		}
		count = 0;
		deadCount = 0;
		if (!clldr.enabled)
		{
			clldr.enabled = true;
		}
		if (spawning != null)
		{
			StopCoroutine(spawning);
			spawning = null;
		}
	}

	private void CheckEnemy(BaseEnemy enemy)
	{
		if (spawning == null)
		{
			return;
		}
		for (int i = 0; i < enemies.Count; i++)
		{
			if (enemy == enemies[i])
			{
				deadCount++;
				if ((bool)door)
				{
					door.UpdateDeathLock(deadCount);
				}
				break;
			}
		}
	}

	private void OnTriggerExit2(Collider c)
	{
		if (!(t.InverseTransformPoint(c.transform.position).z < 0f) && spawning == null)
		{
			spawning = StartCoroutine(Spawning());
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (spawning == null)
		{
			spawning = StartCoroutine(Spawning());
		}
	}

	private IEnumerator Spawning()
	{
		count = 0;
		deadCount = 0;
		while (count < entries.Count)
		{
			if (maxAtOnce == -1 || count - deadCount < maxAtOnce)
			{
				QuickEffectsPool.Get("Summon FX", entries[count].pos).Play();
				timer = 0f;
				while (timer < 0.75f)
				{
					timer += Time.deltaTime;
					yield return null;
				}
				enemies[count].ActivateEnemy();
				QuickEffectsPool.Get("Summon FX END", entries[count].pos).Play();
				count++;
			}
			timer = 0f;
			while (timer < 0.5f)
			{
				timer += Time.deltaTime;
				yield return null;
			}
		}
		while (deadCount != entries.Count)
		{
			yield return null;
		}
		if ((bool)objBarrier)
		{
			Game.time.SlowMotion(0.1f, 0.4f, 0.2f);
		}
		if (arenaFight)
		{
			Game.mission.SetState(2);
			CameraController.shake.Shake(2);
		}
		yield return new WaitForSeconds(0.4f);
		if ((bool)door)
		{
			door.Open();
		}
		if ((bool)objBarrier)
		{
			objBarrier.SetActive(value: false);
			CameraController.shake.Shake(2);
			QuickEffectsPool.Get("Chain Gate Explosion", objBarrier.transform.position, objBarrier.transform.rotation).Play();
		}
		clldr.enabled = false;
		spawning = null;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.grey;
		Gizmos.DrawRay(base.transform.position, base.transform.forward * 4f);
		Gizmos.DrawSphere(base.transform.position, 0.75f);
	}
}
