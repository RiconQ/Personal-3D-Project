using System;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
public class ShelvesTest : MonoBehaviour, ITriggerable
{
	public float yForce = 1f;

	private Vector3 torque = new Vector3(90f, 45f, 0f);

	public Vector3 direction = new Vector3(0f, 1f, 1f);

	public NavMeshModifierVolume NavMeshModifier;

	private Transform t;

	private BreakableB[] breakables;

	private BaseEnemy enemy;

	private ShelvesTestCollider shelvesClldr;

	public bool isBreaked { get; private set; }

	public void Trigger()
	{
		if (!isBreaked)
		{
			isBreaked = true;
			UnityEngine.Random.Range(0, breakables.Length);
			for (int i = 0; i < breakables.Length; i++)
			{
				breakables[i].rb.isKinematic = false;
				breakables[i].t.position += t.position.DirTo(breakables[i].t.position);
				torque.x = UnityEngine.Random.Range(-90f, 90f);
				torque.z = UnityEngine.Random.Range(-45f, 45f);
				breakables[i].rb.AddForceAndTorque(t.TransformDirection(direction).normalized * 20f, torque);
			}
			if ((bool)NavMeshModifier)
			{
				NavMeshModifier.enabled = false;
				OffMeshLinkManager.instance.surface.BuildNavMesh();
			}
			if (shelvesClldr.isActiveAndEnabled)
			{
				shelvesClldr.Break();
			}
			CameraController.shake.Shake(1);
		}
	}

	private void Start()
	{
		t = base.transform;
		breakables = GetComponentsInChildren<BreakableB>();
		for (int i = 0; i < breakables.Length; i++)
		{
			breakables[i].shelves = this;
			breakables[i].t.SetParent(null);
		}
		shelvesClldr = GetComponentInChildren<ShelvesTestCollider>();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		if ((bool)NavMeshModifier)
		{
			NavMeshModifier.enabled = true;
		}
		isBreaked = false;
		for (int i = 0; i < breakables.Length; i++)
		{
			breakables[i].rb.isKinematic = true;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawRay(Vector3.zero, direction);
	}
}
