using UnityEngine;

public class LockSpell : MonoBehaviour, IDamageable<Vector4>, IKickable<Vector3>
{
	private Rigidbody rb;

	private LineRenderer line;

	public HeavyDoor door;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		line = GetComponent<LineRenderer>();
		line.SetPosition(0, base.transform.TransformPoint(line.GetPosition(0)));
		line.SetPosition(2, base.transform.TransformPoint(line.GetPosition(2)));
		line.useWorldSpace = true;
	}

	public void Damage(Vector4 dir)
	{
		QuickEffectsPool.Get("Block", base.transform.position).Play();
		if (dir.w == 125f)
		{
			door.Open();
			base.gameObject.SetActive(value: false);
		}
		else
		{
			rb.AddForce(dir.normalized * 10f, ForceMode.Impulse);
		}
	}

	public void Kick(Vector3 dir)
	{
		rb.AddForce(dir.normalized * 5f, ForceMode.Impulse);
	}

	private void Update()
	{
		line.SetPosition(1, base.transform.position);
	}
}
