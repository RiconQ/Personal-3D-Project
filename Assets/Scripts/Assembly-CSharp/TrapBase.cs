using UnityEngine;

public abstract class TrapBase : MonoBehaviour
{
	public Transform t { get; protected set; }

	public abstract void Trigger();
}
