using UnityEngine;

[SelectionBase]
public class ClimbChain : SetupableMonobehavior, IPlatformable
{
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public BoxCollider clldr;

	public ChainRenderer rend;

	public Transform t;

	private int sign;

	private float timer;

	private float speed;

	private Vector3 off;

	private Vector3 offset;

	private Vector3 pivotOffset;

	private Vector3 pos;

	private Vector3 targetPos;

	private Vector3 target;

	private void Awake()
	{
		target = t.TransformPoint(localTarget);
	}

	public void Setup()
	{
		t = base.transform;
		localTarget = new Vector3(0f, -10f, 0f);
		clldr = GetComponent<BoxCollider>();
		clldr.isTrigger = true;
		target = t.position - Vector3.up * 10f;
		curve = new AnimationCurve();
		curve.AddKey(new Keyframe(0f, 0f, 0f, 8f));
		curve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
		base.gameObject.layer = 12;
		UpdateLine();
	}

	public override void SetTargetPosition(Vector3 worldPos)
	{
		worldPos.x = t.position.x;
		worldPos.z = t.position.z;
		target = worldPos;
		localTarget = t.InverseTransformPoint(worldPos);
		UpdateLine();
	}

	public void UpdateLine()
	{
		if (!t || !clldr)
		{
			Setup();
		}
		float num = Vector3.Distance(t.position, t.position + localTarget);
		rend.GenerateChainMesh(t.position, t.position + localTarget);
		clldr.center = new Vector3(0f, (0f - num) / 2f, 0f);
		clldr.size = new Vector3(0.8f, num, 0.8f);
	}

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
		float magnitude = Game.player.rb.velocity.magnitude;
		if ((Game.player.JumpHolded() || PlayerController.gamepad) && Game.player.Grab(this))
		{
			pos = Game.player.t.position;
			Vector3 vector = Game.player.t.position.ClosestPointOnLine(t.position, target);
			pivotOffset = pos - vector;
			offset = -pivotOffset + vector.DirTo(Game.player.t.position).With(null, 0f) * 0.45f;
			sign = Vector3.Dot(Game.player.tHead.forward, t.forward).Sign();
			targetPos = ((sign == 1) ? target : (t.position - Vector3.up)) + pivotOffset;
			timer = 0f;
			if (Game.player.tHead.forward.y.Abs() > 0.25f)
			{
				speed = magnitude * 1.5f;
			}
			else
			{
				speed = 0f;
			}
			speed = -6f;
			CameraController.shake.Shake(1);
			Game.sounds.PlayClipAtPosition(Game.player.sfxChainGrab, 1f, Game.player.t.position);
		}
	}

	public void Tick()
	{
		if (Game.player.JumpReleased())
		{
			Drop();
			Game.player.sway.Sway(5f, 0f, 0f, 4f);
			Game.player.rb.AddForce(Vector3.up * 22f, ForceMode.Impulse);
		}
		targetPos = t.position + pivotOffset;
		speed = Mathf.Lerp(speed, 16f, Time.deltaTime * 4f);
		pos = Vector3.MoveTowards(pos, targetPos, Time.deltaTime * speed);
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 0.5f);
			off = Vector3.LerpUnclamped(Vector3.zero, offset, curve.Evaluate(timer));
		}
		Game.player.t.position = pos + off;
		Game.player.camController.Angle(0f);
	}

	public void Drop()
	{
		Game.player.Drop();
		Game.sounds.PlayClipAtPosition(Game.player.sfxChainOff, 1f, Game.player.t.position);
	}
}
