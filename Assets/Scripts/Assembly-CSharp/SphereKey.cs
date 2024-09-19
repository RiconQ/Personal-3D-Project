using System;
using UnityEngine;

public class SphereKey : BaseBreakable
{
	private Vector3 startPosition;

	private Quaternion startRotation;

	public override void Awake()
	{
		base.Awake();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Start()
	{
		startPosition = base.t.position;
		startRotation = base.t.rotation;
	}

	private void Reset()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: true);
		}
		Rigidbody rigidbody = base.rb;
		Vector3 velocity = (base.rb.angularVelocity = Vector3.zero);
		rigidbody.velocity = velocity;
		base.t.SetPositionAndRotation(startPosition, startRotation);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Target"))
		{
			CameraController.shake.Shake(2);
			base.gameObject.SetActive(value: false);
		}
		else if (collision.gameObject.layer == 0)
		{
			Reset();
		}
	}
}
