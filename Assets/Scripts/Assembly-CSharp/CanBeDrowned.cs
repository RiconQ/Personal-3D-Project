using System;
using UnityEngine;

public class CanBeDrowned : MonoBehaviour
{
	public Action OnDrowned = delegate
	{
	};

	private int state;

	private Transform t;

	private Quaternion upRotation = Quaternion.LookRotation(Vector3.up);

	private BodyCollider bodyCollider;

	private void Awake()
	{
		t = base.transform;
		bodyCollider = GetComponent<BodyCollider>();
	}

	public void Reset()
	{
		state = 0;
	}

	private void OnEnable()
	{
		Reset();
	}

	private void LateUpdate()
	{
		switch (state)
		{
		case 0:
			if (t.position.y < OceanScript.WaterHeightAtPoint(t.position, global: true))
			{
				state = 1;
				QuickEffectsPool.Get("Splash", t.position, upRotation).Play();
				if (OnDrowned != null)
				{
					OnDrowned();
				}
			}
			break;
		case 1:
			if (t.position.y < OceanScript.WaterHeightAtPoint(t.position, global: true) - 10f)
			{
				if ((bool)bodyCollider)
				{
					state = 0;
					bodyCollider.body.DeactivateBody();
				}
				else
				{
					t.root.gameObject.SetActive(value: false);
				}
			}
			break;
		}
	}
}
