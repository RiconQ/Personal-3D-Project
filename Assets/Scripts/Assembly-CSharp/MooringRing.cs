using UnityEngine;

public class MooringRing : MonoBehaviour, IPlatformable
{
	public AudioClip sound;

	public Transform t;

	public Transform tRing;

	public SpringJoint joint;

	private Vector3 angles;

	private void OnTriggerStay()
	{
		Grab();
	}

	private void OnTriggerEnter()
	{
		Grab();
	}

	public void Grab()
	{
		if (!Physics.Raycast(t.position, -t.right, 1f, 16384) && !(Game.player.airControlBlock > 0f) && Game.player.Grab(this, kinematic: false))
		{
			if ((Game.player.ringTimer != 0f || Game.player.parkourActionsCount > 0 || Game.player.jumpBuffer > 0f) && Game.player.JumpHolded())
			{
				Drop();
				Game.player.ParkourMove();
				Game.player.airControlBlock = 0.1f;
				Game.player.ForceInDir((Vector3.up + Game.player.tHead.forward.With(null, 0f).normalized / 6f).normalized * 32f);
				Game.player.sway.Sway(5f, 0f, 3f, 4f);
				CameraController.shake.Shake(1);
				QuickEffectsPool.Get("Poof", t.position, Quaternion.LookRotation(t.forward)).Play();
				Game.sounds.PlayClipAtPosition(sound, 0.6f, t.position);
			}
			else
			{
				Game.player.ringTimer = 0.5f;
				joint.connectedBody = Game.player.rb;
				Game.player.rb.drag = 6f;
				Game.player.sway.Sway(0f, 0f, 10f, 3f);
				QuickEffectsPool.Get("Poof", t.position, Quaternion.LookRotation(t.forward)).Play();
				Game.sounds.PlayClipAtPosition(sound, 0.6f, t.position);
			}
		}
	}

	public void Tick()
	{
		if (Game.player.JumpPressed())
		{
			Drop();
			Game.player.airControlBlock = 0.05f;
			Game.player.ForceInDir(Vector3.up.normalized * 22f);
		}
		else
		{
			angles.x = Mathf.LerpAngle(angles.x, -20f, Time.deltaTime * 4f);
			tRing.localEulerAngles = angles;
			Game.player.camController.Angle(0f);
		}
	}

	public void Drop()
	{
		joint.connectedBody = null;
		angles.x = (angles.y = (angles.z = 0f));
		tRing.localEulerAngles = angles;
		Game.player.Drop();
		Game.player.rb.drag = 0f;
	}
}
