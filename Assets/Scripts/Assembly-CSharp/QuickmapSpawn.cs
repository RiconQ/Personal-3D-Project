using System;
using UnityEngine;

public class QuickmapSpawn : MonoBehaviour
{
	private Transform t;

	private Transform tMesh;

	private Transform tTarget;

	private void Awake()
	{
		t = base.transform;
		tMesh = t.Find("Mesh");
		tTarget = t.Find("Target");
		QuickmapScene.OnPlayMode = (Action)Delegate.Combine(QuickmapScene.OnPlayMode, new Action(OnPlayMode));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(OnEditMode));
	}

	private void OnDestroy()
	{
		QuickmapScene.OnPlayMode = (Action)Delegate.Remove(QuickmapScene.OnPlayMode, new Action(OnPlayMode));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(OnEditMode));
	}

	private void OnPlayMode()
	{
		tMesh.gameObject.SetActive(value: false);
	}

	private void OnEditMode()
	{
		tMesh.gameObject.SetActive(value: true);
	}

	private void OnTriggerEnter()
	{
		Game.mission.SetState(1);
		Game.player.t.position = tTarget.position;
		SavePoint.lastSavepoint = tTarget.GetComponent<SpawnPoint>();
	}
}
