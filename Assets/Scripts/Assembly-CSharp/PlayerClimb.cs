using System;
using UnityEngine;

public class PlayerClimb : MonoBehaviour
{
	public PlayerController p;

	private int checkCount = -1;

	private float checkTimer;

	private RaycastHit hit;

	private Vector3 startPos;

	private Vector3 startDir;

	private Vector3 targetPos;

	public int state { get; private set; }

	public float timer { get; private set; }

	public bool TryToClimb()
	{
		if (state > 0 || p.weapons.IsAttacking() || !p.jumpHolded)
		{
			return false;
		}
		checkTimer += Time.deltaTime;
		if (Mathf.FloorToInt(checkTimer * 20f) != checkCount)
		{
			checkCount++;
			if (p.rb.isKinematic)
			{
				p.jumpHolded = false;
			}
			Debug.DrawRay(p.t.position, p.tHead.forward.With(null, 0f).normalized, Color.green, 2f);
			Vector3 position = p.t.position;
			position.y += 1.5f;
			Vector3 vector = position + p.tHead.forward.With(null, 0f).normalized * 2f;
			if (Physics.Linecast(p.t.position, position, 16385))
			{
				Debug.DrawLine(p.t.position, position, Color.red, 2f);
				return false;
			}
			if (Physics.Linecast(position, vector, 16385))
			{
				Debug.DrawLine(position, vector, Color.red, 2f);
				return false;
			}
			if (Physics.Raycast(vector, Vector3.down, out hit, 4f, 1) && hit.normal.y.InDegrees() < 30f && hit.point.y + 1f > p.t.position.y && !Physics.Raycast(p.t.position.With(null, hit.point.y + 1f), p.tHead.forward.With(null, 0f).normalized, 2f, 16385))
			{
				if (Physics.Raycast(p.t.position, Vector3.down, 1.5f, 1))
				{
					return false;
				}
				targetPos = hit.point + hit.normal;
				state = 3;
				checkTimer = 0f;
				checkCount = -1;
				p.jumpHolded = false;
				return true;
			}
			return false;
		}
		return false;
	}

	public void Reset()
	{
		if (state != 0)
		{
			state = 0;
			p.grounder.Reset();
			p.rb.isKinematic = false;
			p.rb.collisionDetectionMode = ((!p.grounder.grounded) ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete);
			p.weapons.gameObject.SetActive(value: true);
		}
		checkTimer = 0f;
		checkCount = -1;
	}

	public void ClimbingUpdate()
	{
		switch (state)
		{
		case 3:
		{
			p.rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			p.rb.isKinematic = true;
			p.rb.velocity = Vector3.zero;
			p.sway.Sway(10f, 0f, -5f, 2f);
			p.fov.kinematicFOV = 0f;
			p.weapons.gameObject.SetActive(value: false);
			startPos = p.t.position;
			float num = targetPos.y - startPos.y;
			startDir = Vector3.Lerp((startPos.DirTo(targetPos).With(null, 0f) + Vector3.up).normalized, Vector3.up * num, 0.7f);
			timer = 0f;
			state = 2;
			Game.sounds.PlayClip(p.climbSound, 0.5f);
			QuickEffectsPool.Get("Poof", targetPos - Vector3.up).Play();
			break;
		}
		case 2:
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 3f);
			p.t.position = MegaHelp.CalculateCubicBezierPoint(timer, startPos, startPos + startDir, targetPos, targetPos);
			p.fov.kinematicFOV = Mathf.Sin(timer * (float)Math.PI) * 5f;
			Game.player.camController.Angle(Mathf.Sin(timer * (float)Math.PI) * 7.5f);
			Debug.DrawRay(p.t.position, p.t.up * 0.2f, Color.grey, 2f);
			if (p.JumpPressed() && timer > 0.5f)
			{
				Reset();
				p.BasicJump(1.1f);
			}
			if (timer == 1f)
			{
				state = 1;
			}
			break;
		case 1:
			p.grounder.Reset();
			p.rb.isKinematic = false;
			p.rb.collisionDetectionMode = ((!p.grounder.grounded) ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete);
			p.rb.velocity = p.gDir * 10f;
			p.weapons.gameObject.SetActive(value: true);
			state = 0;
			break;
		}
	}
}
