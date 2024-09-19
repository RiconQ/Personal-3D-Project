using UnityEngine;

public class LastActiveCamera : MonoBehaviour
{
	private Camera myCamera;

	public static Camera cam { get; private set; }

	public static Transform tCam { get; private set; }

	private void Awake()
	{
		myCamera = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		Set(myCamera);
	}

	private void LateUpdate()
	{
		Shader.SetGlobalVector("_PlayerPos", tCam.position);
	}

	public static void Set(Camera lastCam)
	{
		cam = lastCam;
		tCam = lastCam.transform;
	}
}
