using System;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
	public PlayerController p;

	public DamageType dmg_Dash;

	public AudioClip sfxDash;

	public AudioClip sfxAirDash;

	public StylePoint brutalDash;

	private float timer;

	private float speed = 30f;

	private Vector3 dir;

	private Vector3 startPos;

	private Vector3 targetPos;

	private Transform tTarget;

	private RaycastHit hit;

	private DamageData damage = new DamageData();

	public bool isDashing { get; private set; }

	public int state { get; private set; }

	private void Awake()
	{
		damage.amount = 1000f;
	}

	public void Reset()
	{
		p.rb.isKinematic = false;
		p.headPosition.ChangeYPosition(0.75f);
		isDashing = false;
		state = 0;
	}

	public bool Dash()
	{
		if (state != 0 || !p.dashPossible)
		{
			return false;
		}
		tTarget = WeaponsControl.instance.GetClosestTarget2();
		if (tTarget == null || p.slide.slideState != 0)
		{
			return false;
		}
		if (!isDashing && state == 0)
		{
			Physics.Raycast(tTarget.position, Vector3.down, out hit, (!p.grounder.grounded) ? 1 : 4, 1);
			if (hit.distance != 0f)
			{
				targetPos = hit.point + Vector3.up;
			}
			else
			{
				targetPos = tTarget.position;
			}
			state = 3;
			isDashing = true;
			return true;
		}
		return false;
	}

	public void DashingUpdate()
	{
		switch (state)
		{
		case 3:
			Game.sounds.PlayClipAtPosition(p.grounder.grounded ? sfxDash : sfxAirDash, 1f, startPos);
			if (p.grounder.grounded)
			{
				p.grounder.Ungrounded();
			}
			p.SetKinematic(value: true);
			p.weapons.gameObject.SetActive(value: false);
			if (tTarget.gameObject.layer == 11 || tTarget.gameObject.layer == 10)
			{
				p.weapons.DropCurrentWeapon((Vector3.up + p.tHead.forward.With(null, 0f) / 2f).normalized);
			}
			else
			{
				tTarget.GetComponentInChildren<DashableObject>().PreDash();
			}
			startPos = p.t.position;
			dir = startPos.DirTo(targetPos);
			speed = (8f - Vector3.Distance(p.t.position, targetPos) / 2f).Abs();
			speed = Mathf.Clamp(speed, 2f, 12f);
			p.fov.kinematicFOV = 0f;
			p.headPosition.ChangeYPosition(0f);
			p.sway.Sway(0f, 0f, 6f, 3f);
			CameraController.shake.Shake(1);
			timer = 0f;
			state--;
			break;
		case 2:
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
			p.fov.kinematicFOV = Mathf.Sin(timer * (float)Math.PI) * 25f;
			p.t.position = Vector3.Lerp(startPos, targetPos, timer - 0.2f);
			p.camController.Angle(Mathf.Sin(timer * (float)Math.PI) * -5f);
			if (timer == 1f)
			{
				state--;
			}
			break;
		case 1:
			p.ParkourMove();
			p.dashPossible = false;
			p.airControlBlock = 0.2f;
			p.headPosition.ChangeYPosition(0.75f);
			p.weapons.gameObject.SetActive(value: true);
			p.SetKinematic(value: false);
			p.rb.AddForce(dir * 25f, ForceMode.VelocityChange);
			if (tTarget.gameObject.layer == 11 || tTarget.gameObject.layer == 10)
			{
				Body componentInParent = tTarget.GetComponentInParent<Body>();
				p.weapons.PickWeapon(componentInParent.enemy.weapon.index);
				componentInParent.rb.isKinematic = false;
				damage.dir = dir;
				damage.newType = dmg_Dash;
				componentInParent.Damage(damage);
				StyleRanking.instance.RegStylePoint(brutalDash);
				CameraController.shake.Shake();
				Game.time.SlowMotion(0.2f, 0.1f, 0.1f);
			}
			else
			{
				tTarget.GetComponentInChildren<DashableObject>().Dash();
			}
			isDashing = false;
			state--;
			break;
		}
	}
}
