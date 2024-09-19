using UnityEngine;

[SelectionBase]
public class SwingRope : SetupableMonobehavior, IPlatformable
{
	public RopeRenderer ropeMesh;

	public Vector3 closestPoint;

	public int closestPointIndex;

	private Vector3 playerPos;

	private Vector3 dir;

	private Vector3 gravity = new Vector3(0f, -10f, 0f);

	private RaycastHit hit;

	private float distance;

	public AudioClip sfxGrab;

	public AudioClip sfxDrop;

	public Transform t { get; private set; }

	public Transform tJoint { get; private set; }

	public SpringJoint joint { get; private set; }

	public LineRenderer lineRend { get; private set; }

	public CapsuleCollider clldr { get; private set; }

	private void Awake()
	{
		t = base.transform;
		tJoint = new GameObject("Joint").transform;
		tJoint.SetParent(t);
		tJoint.SetPositionAndRotation(t.position, Quaternion.identity);
		joint = tJoint.gameObject.AddComponent<SpringJoint>();
		joint.GetComponent<Rigidbody>().isKinematic = true;
		joint.autoConfigureConnectedAnchor = false;
		joint.connectedAnchor = Vector3.zero;
		joint.spring = 100f;
		base.enabled = false;
	}

	public void Setup()
	{
		lineRend = GetComponentInChildren<LineRenderer>();
		lineRend.useWorldSpace = false;
		lineRend.SetPosition(0, Vector3.zero);
		LineRenderer lineRenderer = lineRend;
		float startWidth = (lineRend.endWidth = 0.35f);
		lineRenderer.startWidth = startWidth;
		lineRend.textureMode = LineTextureMode.Tile;
		clldr = GetComponentInChildren<CapsuleCollider>();
		clldr.isTrigger = true;
		base.gameObject.layer = 12;
		UpdateChain();
	}

	public override void SetTargetPosition(Vector3 worldPos)
	{
		worldPos.x = t.position.x;
		worldPos.z = t.position.z;
		localTarget = t.InverseTransformPoint(worldPos);
		UpdateChain();
	}

	public void UpdateChain()
	{
		if (!lineRend)
		{
			Setup();
		}
		t = base.transform;
		float num = Vector3.Distance(t.position, t.position + localTarget);
		lineRend.SetPosition(1, localTarget);
		clldr.center = new Vector3(0f, (0f - num) / 2f, 0f);
		clldr.radius = 1f;
		clldr.height = num;
		if (!QuickmapScene.instance)
		{
			ropeMesh.GenerateRopeMesh(t.position, t.position + localTarget);
		}
		else
		{
			lineRend.enabled = true;
		}
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
		if ((Game.player.JumpHolded() || PlayerController.gamepad) && Game.player.Grab(this, kinematic: false))
		{
			base.enabled = true;
			distance = Vector3.Distance(Game.player.t.position, t.position);
			closestPoint = Game.player.t.position.ClosestPointOnLine(t.position, t.position + localTarget);
			closestPointIndex = Mathf.RoundToInt(Vector3.Distance(closestPoint, t.position));
			joint.minDistance = distance / 2f;
			joint.maxDistance = distance;
			joint.connectedAnchor = closestPoint - Game.player.t.position;
			joint.connectedBody = Game.player.rb;
			float num = Mathf.Lerp(1.5f, 1f, (Game.player.rb.velocity.magnitude - 10f) / 30f);
			Game.player.rb.velocity = Game.player.rb.velocity.With(null, 0f) * num;
			Game.player.sway.Sway(5f, 0f, 0f, 2f);
			CameraController.shake.Shake(1);
			QuickEffectsPool.Get("Poof", closestPoint).Play();
			Game.sounds.PlayClipAtPosition(sfxGrab, 1f, Game.player.t.position);
		}
	}

	public void Tick()
	{
		if (Game.player.JumpReleased())
		{
			Drop();
			return;
		}
		Game.player.camController.Angle(Game.player.rb.velocity.magnitude / 2f);
		playerPos = t.InverseTransformPoint(Game.player.t.position);
		playerPos.y += 1f;
		if ((bool)lineRend)
		{
			lineRend.SetPosition(1, playerPos + joint.connectedAnchor);
		}
	}

	private void FixedUpdate()
	{
		dir = t.position.DirTo(joint.connectedBody.position + joint.connectedAnchor);
		dir = Vector3.Cross(dir, Vector3.up);
		float num = Vector3.Dot(dir, Game.player.tHead.forward.With(null, 0f));
		Game.player.rb.AddForce(dir * (num * 10f));
		Game.player.rb.AddForce(gravity);
		if (Physics.Linecast(t.position + Vector3.down, Game.player.t.position, 1))
		{
			Debug.DrawLine(t.position + Vector3.down, Game.player.t.position, Color.red, 2f);
			Drop();
		}
		else
		{
			Debug.DrawLine(t.position, Game.player.t.position, Color.blue);
		}
	}

	public void Drop()
	{
		base.enabled = false;
		joint.connectedBody = null;
		if ((bool)lineRend)
		{
			lineRend.SetPosition(1, localTarget);
		}
		Game.player.Drop();
		Game.player.rb.AddForce(Vector3.up * 12f, ForceMode.Impulse);
		Game.player.sway.Sway(5f, 0f, 2f, 3.5f);
		Game.sounds.PlayClipAtPosition(sfxDrop, 1f, Game.player.t.position);
	}
}
