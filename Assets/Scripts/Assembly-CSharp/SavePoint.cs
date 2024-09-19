using System;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
	public static SavePoint lastSavepoint;

	public bool SimpleSpawn;

	public Transform t { get; protected set; }

	protected virtual void Start()
	{
		t = base.transform;
		if (CompareTag("Entrance"))
		{
			lastSavepoint = this;
		}
		Grounder grounder = Game.player.grounder;
		grounder.OnGrounded = (Action)Delegate.Combine(grounder.OnGrounded, new Action(Check));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
	}

	protected virtual void OnDestroy()
	{
		Grounder grounder = Game.player.grounder;
		grounder.OnGrounded = (Action)Delegate.Remove(grounder.OnGrounded, new Action(Check));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
	}

	protected virtual void Reset()
	{
		if (CompareTag("Entrance"))
		{
			lastSavepoint = this;
			if ((bool)QuickmapScene.instance)
			{
				Place();
			}
		}
	}

	public virtual void Place()
	{
		Game.player.SetKinematic(value: false);
		Game.player.Activate();
		Game.player.grounder.Ungrounded(forced: true);
		Game.player.t.position = t.position;
		Game.player.grounder.Reset();
		Game.player.mouseLook.LookInDir(t.forward);
		Game.player.weapons.gameObject.SetActive(value: true);
		Game.fading.Fade(0f);
	}

	public virtual void Launch()
	{
		if (!SimpleSpawn)
		{
			if (!t)
			{
				t = base.transform;
			}
			if ((bool)QuickmapScene.instance)
			{
				Place();
				return;
			}
			Game.player.SetKinematic(value: false);
			Game.player.Activate();
			Game.player.t.position = t.position - t.forward * 6f;
			Game.player.grounder.Ungrounded(forced: true);
			Game.player.grounder.Reset();
			Game.player.mouseLook.LookAt(t.position);
			Game.player.rb.AddBallisticForce(t.position, 1f, -40f, resetVelocity: true);
			Game.player.weapons.gameObject.SetActive(value: true);
			Game.fading.Fade(0f);
			Game.player.sway.Sway(-5f, 0f, 0f, 3f);
		}
	}

	public virtual void Spawn()
	{
	}

	private void Check()
	{
		if (!(lastSavepoint == this) && (t.position.y - (Game.player.t.position.y - 1f)).Abs() < 1f && (lastSavepoint == null || t.position.y > lastSavepoint.t.position.y))
		{
			lastSavepoint = this;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = ((lastSavepoint == this) ? Color.green : Color.grey);
		Gizmos.DrawIcon(base.transform.position, "SavePoint.png");
		Gizmos.DrawRay(base.transform.position, base.transform.forward * 2f);
	}
}
