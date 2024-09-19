using System;
using UnityEngine;

public class QuickmapObject : MonoBehaviour
{
	public bool Deleteable = true;

	public string PublicName = "In-editor Name";

	private BoxCollider clldr;

	private void Awake()
	{
		clldr = base.gameObject.AddComponent<BoxCollider>();
		clldr.isTrigger = true;
		clldr.size = new Vector3(6f, 6f, 6f);
		base.gameObject.layer = 12;
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(OnEditMode));
		QuickmapScene.OnPlayMode = (Action)Delegate.Combine(QuickmapScene.OnPlayMode, new Action(OnPlayMode));
	}

	private void OnDestroy()
	{
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(OnEditMode));
		QuickmapScene.OnPlayMode = (Action)Delegate.Remove(QuickmapScene.OnPlayMode, new Action(OnPlayMode));
	}

	private void OnEditMode()
	{
		if ((bool)clldr)
		{
			clldr.enabled = true;
		}
	}

	private void OnPlayMode()
	{
		if ((bool)clldr)
		{
			clldr.enabled = false;
		}
	}
}
