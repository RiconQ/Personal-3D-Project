using System;
using UnityEngine;

public class Shelves : MonoBehaviour
{
	public GameObject prefab;

	public Transform[] positions;

	private BreakableB[] breakables;

	private bool breaked;

	private void Awake()
	{
		breakables = new BreakableB[positions.Length];
		for (int i = 0; i < positions.Length; i++)
		{
			breakables[i] = UnityEngine.Object.Instantiate(prefab, positions[i].position, Quaternion.identity).GetComponent<BreakableB>();
			breakables[i].gameObject.SetActive(value: false);
		}
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Combine(BreakableB.OnBreak, new Action<GameObject>(Check));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		BreakableB.OnBreak = (Action<GameObject>)Delegate.Remove(BreakableB.OnBreak, new Action<GameObject>(Check));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		for (int i = 0; i < breakables.Length; i++)
		{
			breakables[i].gameObject.SetActive(value: false);
		}
		breaked = false;
	}

	private void Check(GameObject obj)
	{
		if (!breaked && obj == base.gameObject)
		{
			Break();
		}
	}

	private void Break()
	{
		for (int i = 0; i < breakables.Length; i++)
		{
			Vector3 forward = positions[i].forward;
			forward = CrowdControl.instance.GetClosestDirectionToNormal(base.transform.position, forward, 30f);
			breakables[i].gameObject.SetActive(value: true);
			breakables[i].transform.SetPositionAndRotation(positions[i].position, positions[i].parent.rotation);
			breakables[i].Kick(forward * UnityEngine.Random.Range(5f, 15f));
		}
		breaked = true;
	}

	private void OnDrawGizmos()
	{
		for (int i = 0; i < positions.Length; i++)
		{
			Gizmos.DrawRay(positions[i].position, positions[i].forward);
		}
	}
}
