using System;
using System.Collections.Generic;
using UnityEngine;

public class ChainBarrierArena : MonoBehaviour
{
	public GameObject objOrbBarrier;

	public GameObject orbChainPrefab;

	public TheOrb orb;

	public Material mat;

	public Transform t;

	public BarrierChain[] chains;

	private int deadCount;

	private Vector3 startPos;

	private Vector3 temp;

	private List<int> indices;

	private void Start()
	{
		t = base.transform;
		startPos = t.position;
		t.SetParent(null);
		mat.SetVector("_Center", t.position);
		int count = CrowdControl.allEnemies.Count;
		chains = new BarrierChain[count];
		indices = new List<int>(count);
		for (int i = 0; i < count; i++)
		{
			indices.Add(i);
			chains[i] = UnityEngine.Object.Instantiate(orbChainPrefab, t).GetComponent<BarrierChain>();
			chains[i].t.localEulerAngles = new Vector3(0f, (float)i * 360f / (float)count + UnityEngine.Random.Range(-15f, 15f), 0f);
		}
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		BaseEnemy.OnAnyEnemyDie = (Action)Delegate.Combine(BaseEnemy.OnAnyEnemyDie, new Action(Check));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		BaseEnemy.OnAnyEnemyDie = (Action)Delegate.Remove(BaseEnemy.OnAnyEnemyDie, new Action(Check));
	}

	private void Check()
	{
		int index = UnityEngine.Random.Range(0, indices.Count);
		chains[indices[index]].Hide();
		indices.RemoveAt(index);
		deadCount++;
		if (deadCount == CrowdControl.allEnemies.Count)
		{
			objOrbBarrier.SetActive(value: false);
			orb.DestroyTheOrb();
		}
	}

	private void Reset()
	{
		indices.Clear();
		for (int i = 0; i < chains.Length; i++)
		{
			indices.Add(i);
		}
		deadCount = 0;
		objOrbBarrier.SetActive(value: true);
		BarrierChain[] array = chains;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Reset();
		}
	}

	private void LateUpdate()
	{
		float num = Mathf.Sin(Time.time);
		temp = startPos;
		temp.y += num * 0.5f;
		t.position = temp;
		BarrierChain[] array = chains;
		foreach (BarrierChain barrierChain in array)
		{
			temp = barrierChain.t.localEulerAngles;
			temp.z = -5f - num * 2f;
			barrierChain.t.localEulerAngles = temp;
		}
	}
}
