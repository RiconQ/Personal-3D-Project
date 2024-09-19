using UnityEngine;

public class NPC : MonoBehaviour, IDamageable<DamageData>, IKickable<Vector3>
{
	[TextArea]
	public string[] lines;

	public int index;

	private Transform t;

	private Transform tHead;

	private Quaternion startRot;

	private PlayerController p;

	private Vector3 dir;

	private Quaternion rot;

	public void Damage(DamageData dmg)
	{
	}

	public void Kick(Vector3 dir)
	{
		if (index < lines.Length)
		{
			TextBlob.instance.Show(lines[index]);
			index = index.Next(lines.Length + 1);
		}
		else
		{
			GetComponent<Rigidbody>().isKinematic = false;
			GetComponent<Rigidbody>().AddForce((dir + Vector3.up) / 2f * 10f, ForceMode.Impulse);
		}
	}

	private void Awake()
	{
		t = base.transform;
		tHead = t.Find("Head").transform;
		startRot = t.rotation;
	}

	private void Start()
	{
		p = PlayerController.instance;
	}

	private void Update()
	{
		if (Vector3.Distance(p.t.position, t.position) < 6f)
		{
			dir = tHead.position.DirTo(p.tHead.position);
			rot = Quaternion.LookRotation(dir);
			rot = new Quaternion(0f, rot.y, 0f, rot.w);
			t.rotation = Quaternion.Slerp(t.rotation, rot, Time.deltaTime * 6f);
			rot = Quaternion.LookRotation(dir);
			rot = new Quaternion(rot.x, 0f, 0f, rot.w);
			tHead.localRotation = Quaternion.Slerp(tHead.localRotation, rot, Time.deltaTime * 6f);
		}
		else
		{
			t.rotation = Quaternion.Slerp(t.rotation, startRot, Time.deltaTime);
			tHead.localRotation = Quaternion.Slerp(tHead.localRotation, Quaternion.identity, Time.deltaTime);
		}
	}
}
