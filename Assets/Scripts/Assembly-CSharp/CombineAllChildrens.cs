using System.Collections.Generic;
using UnityEngine;

public class CombineAllChildrens : MonoBehaviour
{
	private void Start()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		List<GameObject> list = new List<GameObject>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			list.Add(componentsInChildren[i].gameObject);
		}
		if (list.Count > 0)
		{
			StaticBatchingUtility.Combine(list.ToArray(), base.gameObject);
		}
	}

	public void GroupUpAllStaticObjects()
	{
		List<GameObject> list = new List<GameObject>();
		list.AddRange(Object.FindObjectsOfType<GameObject>());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!list[num].isStatic || list[num].transform.root == base.transform || list[num].transform.parent != null)
			{
				list.RemoveAt(num);
			}
		}
		Debug.Log(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].transform.SetParent(base.transform);
		}
	}
}
