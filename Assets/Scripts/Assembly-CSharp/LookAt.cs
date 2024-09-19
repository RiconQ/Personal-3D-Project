using UnityEngine;

[ExecuteInEditMode]
public class LookAt : MonoBehaviour
{
	public bool active;

	public Transform t;

	public Transform lookAtTarget;

	private void LateUpdate()
	{
		if (active && (bool)lookAtTarget && (bool)t)
		{
			t.LookAt(lookAtTarget.position, Vector3.up);
			t.Rotate(-90f, 0f, 0f);
			Debug.DrawLine(t.position, lookAtTarget.position, Color.blue);
		}
	}
}
