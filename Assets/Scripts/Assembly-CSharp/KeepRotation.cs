using UnityEngine;

public class KeepRotation : MonoBehaviour
{
	private Transform t;

	private Quaternion rot = Quaternion.Euler(0f, -1f, 0f);

	private void Awake()
	{
		t = base.transform;
	}

	private void LateUpdate()
	{
		if (t.rotation != rot)
		{
			t.rotation = rot;
		}
	}
}
