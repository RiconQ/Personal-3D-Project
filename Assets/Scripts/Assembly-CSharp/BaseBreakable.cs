using UnityEngine;

public class BaseBreakable : MonoBehaviour, IDamageable<DamageData>, IKickable<Vector3>
{
	[HideInInspector]
	public StylePointTypes styleMove;

	public Transform t { get; private set; }

	public Rigidbody rb { get; private set; }

	public Collider clldr { get; private set; }

	public virtual void Awake()
	{
		t = base.transform;
		rb = GetComponent<Rigidbody>();
		clldr = GetComponent<Collider>();
		BreakablesControl.instance.Add(this);
		styleMove = StylePointTypes.Domino;
	}

	public virtual void Damage(DamageData dmg)
	{
	}

	public virtual void Kick(Vector3 dir)
	{
	}
}
