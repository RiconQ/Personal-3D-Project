using UnityEngine;

public class Jumper : SetupableMonobehavior
{
	public float timeToTarget = 1.25f;

	public Vector3 target;

	public Transform tParticleDir;

	public Transform tParticleDir2;

	public DamageData dmg = new DamageData();

	private Transform t;

	private Animation anim;

	private void Awake()
	{
		t = base.transform;
		anim = GetComponentInChildren<Animation>();
		dmg.amount = 10f;
		dmg.knockdown = true;
		base.gameObject.layer = 17;
	}

	public void SetParticleDir(Vector3 dir)
	{
		tParticleDir.rotation = Quaternion.LookRotation(dir, base.transform.up);
		tParticleDir2.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(dir, Vector3.up), base.transform.up);
	}

	public override Vector3 GetWorldTargetPosition()
	{
		return target;
	}

	public override void SetTargetPosition(Vector3 worldPos)
	{
		target = worldPos;
	}

	private void OnTriggerEnter(Collider other)
	{
		switch (other.gameObject.layer)
		{
		case 9:
		{
			Game.player.airControlBlock = timeToTarget / 2f;
			Game.player.grounder.Ungrounded();
			Game.player.rb.velocity = Vector3.zero;
			Game.player.sway.Sway(7f, 0f, 0f, 2.5f);
			Vector3 forward = Game.player.rb.AddBallisticForce(target + Vector3.up, timeToTarget, -40f);
			anim.Play();
			QuickEffectsPool.Get("Goo Splash", t.position + forward.normalized * 2f, Quaternion.LookRotation(forward)).Play();
			CameraController.shake.Shake(1);
			break;
		}
		case 10:
			dmg.dir = (Vector3.up + other.attachedRigidbody.velocity).normalized;
			dmg.newType = Game.style.basicBluntHit;
			other.GetComponent<IDamageable<DamageData>>().Damage(dmg);
			anim.Play();
			QuickEffectsPool.Get("Goo Splash", t.position, Quaternion.LookRotation(Vector3.up)).Play();
			break;
		case 14:
			other.attachedRigidbody.velocity = Vector3.zero;
			other.attachedRigidbody.AddBallisticForce(target + Vector3.up, timeToTarget, Physics.gravity.y);
			anim.Play();
			QuickEffectsPool.Get("Goo Splash", t.position, Quaternion.LookRotation(Vector3.up)).Play();
			break;
		}
	}

	private void OnDrawGizmosSelected()
	{
		int num = 20;
		Vector3 to = default(Vector3);
		Vector3 vector = MegaHelp.BallisticTrajectory3D(base.transform.position, target, timeToTarget, -40f);
		Gizmos.color = Color.green;
		Vector3 vector2 = default(Vector3);
		for (int i = 0; i < num; i++)
		{
			float num2 = (float)i * (timeToTarget / (float)num);
			vector2.x = vector.x * num2;
			vector2.y = vector.y * num2 - 20f * (num2 * num2);
			vector2.z = vector.z * num2;
			if (i > 1)
			{
				Gizmos.DrawLine(base.transform.position + vector2, to);
			}
			to = base.transform.position + vector2;
		}
	}
}
