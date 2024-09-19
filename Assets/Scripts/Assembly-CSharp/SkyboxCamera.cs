using UnityEngine;

public class SkyboxCamera : MonoBehaviour
{
	private Transform t;

	private Camera cam;

	private void Awake()
	{
		t = base.transform;
		cam = GetComponent<Camera>();
	}

	private void LateUpdate()
	{
		if ((bool)LastActiveCamera.cam)
		{
			t.localPosition = Game.player.t.position / 1000f;
			t.rotation = LastActiveCamera.tCam.rotation;
			cam.fieldOfView = LastActiveCamera.cam.fieldOfView;
		}
	}
}
