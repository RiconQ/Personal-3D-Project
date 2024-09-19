using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	private Transform t;

	private void Awake()
	{
		t = base.transform;
	}

	private void Update()
	{
		t.LookAt(LastActiveCamera.tCam.position.With(null, t.position.y));
	}
}
