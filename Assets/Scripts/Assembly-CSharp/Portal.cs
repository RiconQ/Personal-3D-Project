using System;
using UnityEngine;
using UnityEngine.Events;

public class Portal : MonoBehaviour, ITriggerable
{
	public Vector3 portalPoint;

	public Vector3 portalTarget;

	private Vector3 dir;

	private Vector3 force;

	private bool isActive;

	public UnityEvent onTeleported;

	public void Trigger()
	{
		isActive = true;
	}

	private void Awake()
	{
		isActive = true;
		dir = portalPoint.DirTo(portalTarget);
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		isActive = false;
		isActive = true;
	}

	private void OnTriggerStay()
	{
		if (isActive && !Game.player.rb.isKinematic)
		{
			Teleport();
		}
	}

	private void Teleport()
	{
		Game.player.t.position = portalPoint;
		Game.player.grounder.Ungrounded();
		Game.player.rb.velocity = Vector3.zero;
		force = Game.player.rb.AddBallisticForce(portalTarget, 1.5f, -40f);
		Game.player.mouseLook.LookInDir(dir.With(null, 0f));
		Game.player.airControlBlock = 0.25f;
		Game.player.gTimer = 0f;
		Game.player.weapons.gameObject.SetActive(value: false);
		Game.player.weapons.gameObject.SetActive(value: true);
		onTeleported.Invoke();
		Game.player.fov.AddToFOV(30f);
		CameraController.shake.Shake(1);
		QuickEffectsPool.Get("EnemyPortalOut", portalPoint + force.normalized * 3f, Quaternion.LookRotation(force)).Play();
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(portalTarget, 0.5f);
		Gizmos.DrawSphere(portalPoint, 0.5f);
		Gizmos.DrawLine(portalPoint, portalTarget);
	}
}
