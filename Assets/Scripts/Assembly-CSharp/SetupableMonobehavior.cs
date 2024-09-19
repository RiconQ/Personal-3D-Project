using UnityEngine;

public class SetupableMonobehavior : MonoBehaviour
{
	public Vector3 localTarget;

	public virtual Vector3 GetWorldTargetPosition()
	{
		return base.transform.TransformPoint(localTarget);
	}

	public virtual void SetTargetPosition(Vector3 worldPos)
	{
		localTarget = base.transform.InverseTransformPoint(worldPos);
	}
}
