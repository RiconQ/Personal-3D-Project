using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuickPoolObject
{
	public GameObject prefab;

	public string name;

	public Queue<PooledMonobehaviour> available = new Queue<PooledMonobehaviour>();

	public PooledMonobehaviour firstSpawned;
}
