using System.Collections.Generic;
using UnityEngine;

public class QuickPool : MonoBehaviour
{
	public static QuickPool instance;

	[HideInInspector]
	public List<QuickPoolObject> prefabs = new List<QuickPoolObject>();

	public PrefabsCollection[] collections = new PrefabsCollection[0];

	private PooledMonobehaviour obj;

	private void Awake()
	{
		instance = this;
		for (int i = 0; i < collections.Length; i++)
		{
			for (int j = 0; j < collections[i].prefabs.Length; j++)
			{
				QuickPoolObject quickPoolObject = new QuickPoolObject();
				quickPoolObject.prefab = collections[i].prefabs[j];
				quickPoolObject.name = quickPoolObject.prefab.name;
				prefabs.Add(quickPoolObject);
			}
		}
		for (int k = 0; k < prefabs.Count; k++)
		{
			int num = 1;
			for (int l = 0; l < num; l++)
			{
				GameObject gameObject = Object.Instantiate(prefabs[k].prefab);
				gameObject.GetComponent<PooledMonobehaviour>().SetPoolIndex(k);
				if (l == 0)
				{
					num = gameObject.GetComponent<PooledMonobehaviour>().maxCount;
				}
				gameObject.SetActive(value: false);
			}
			if (prefabs[k].name == "")
			{
				prefabs[k].name = prefabs[k].prefab.name;
			}
		}
	}

	public int GetIndexByName(string name)
	{
		for (int i = 0; i < prefabs.Count; i++)
		{
			if (prefabs[i].name == name)
			{
				return i;
			}
		}
		AddNewPool(name);
		Debug.Log($"Don't forget to add {name} to the pool");
		return prefabs.Count - 1;
	}

	public void AddNewPool(string name)
	{
		GameObject gameObject = Resources.Load("Pooled/" + name) as GameObject;
		if (gameObject == null)
		{
			return;
		}
		QuickPoolObject quickPoolObject = new QuickPoolObject();
		quickPoolObject.prefab = gameObject;
		quickPoolObject.name = gameObject.name;
		prefabs.Add(quickPoolObject);
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject2 = Object.Instantiate(quickPoolObject.prefab);
			gameObject2.GetComponent<PooledMonobehaviour>().SetPoolIndex(prefabs.Count - 1);
			if (i == 0)
			{
				num = gameObject2.GetComponent<PooledMonobehaviour>().maxCount;
			}
			gameObject2.SetActive(value: false);
		}
	}

	public void ReturnToPool(int index, PooledMonobehaviour obj)
	{
		prefabs[index].available.Enqueue(obj);
	}

	public PooledMonobehaviour Get(string name, Transform t)
	{
		int indexByName = GetIndexByName(name);
		if (prefabs[indexByName].available.Count == 0)
		{
			prefabs[indexByName].firstSpawned.gameObject.SetActive(value: false);
			prefabs[indexByName].firstSpawned = null;
		}
		obj = prefabs[indexByName].available.Dequeue();
		if ((bool)obj.t.parent)
		{
			obj.t.SetParent(null);
		}
		obj.t.SetPositionAndRotation(t.position, t.rotation);
		obj.gameObject.SetActive(value: true);
		if (!prefabs[indexByName].firstSpawned)
		{
			prefabs[indexByName].firstSpawned = obj;
		}
		return obj;
	}

	public PooledMonobehaviour Get(string name, Vector3 pos, Quaternion rot = default(Quaternion))
	{
		int indexByName = GetIndexByName(name);
		if (prefabs[indexByName].available.Count == 0)
		{
			prefabs[indexByName].firstSpawned.gameObject.SetActive(value: false);
			prefabs[indexByName].firstSpawned = null;
		}
		obj = prefabs[indexByName].available.Dequeue();
		if ((bool)obj.t.parent)
		{
			obj.t.SetParent(null);
		}
		obj.t.SetPositionAndRotation(pos, rot);
		obj.gameObject.SetActive(value: true);
		if (!prefabs[indexByName].firstSpawned)
		{
			prefabs[indexByName].firstSpawned = obj;
		}
		return obj;
	}

	public PooledMonobehaviour Get(GameObject prefab, Transform t)
	{
		for (int i = 0; i < prefabs.Count; i++)
		{
			if (prefabs[i].prefab == prefab)
			{
				if (prefabs[i].available.Count == 0)
				{
					prefabs[i].firstSpawned.gameObject.SetActive(value: false);
					prefabs[i].firstSpawned = null;
				}
				obj = prefabs[i].available.Dequeue();
				if ((bool)obj.t.parent)
				{
					obj.t.SetParent(null);
				}
				obj.t.SetPositionAndRotation(t.position, t.rotation);
				obj.gameObject.SetActive(value: true);
				if (!prefabs[i].firstSpawned)
				{
					prefabs[i].firstSpawned = obj;
				}
				return obj;
			}
		}
		return null;
	}

	public PooledMonobehaviour Get(GameObject prefab, Vector3 pos, Quaternion rot = default(Quaternion))
	{
		for (int i = 0; i < prefabs.Count; i++)
		{
			if (prefabs[i].prefab == prefab)
			{
				if (prefabs[i].available.Count == 0)
				{
					prefabs[i].firstSpawned.gameObject.SetActive(value: false);
					prefabs[i].firstSpawned = null;
				}
				obj = prefabs[i].available.Dequeue();
				if ((bool)obj.t.parent)
				{
					obj.t.SetParent(null);
				}
				obj.t.SetPositionAndRotation(pos, rot);
				obj.gameObject.SetActive(value: true);
				if (!prefabs[i].firstSpawned)
				{
					prefabs[i].firstSpawned = obj;
				}
				return obj;
			}
		}
		return null;
	}
}
