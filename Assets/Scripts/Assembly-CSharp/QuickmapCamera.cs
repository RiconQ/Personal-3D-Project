using UnityEngine;

public class QuickmapCamera : MonoBehaviour
{
	public KeyboardInputs inputs;

	private Transform tCam;

	private AudioListener listener;

	private bool isEditing;

	private int firstSelected;

	private int lastSelected;

	private int sign = -1;

	private float timer;

	private float x;

	private float xTemp;

	private float z;

	private float zTemp;

	private float y;

	private float yTemp;

	private Vector3 aPos;

	private Vector3 bPos;

	private Vector3 scale;

	private Vector3 temp;

	private Ray ray;

	private RaycastHit hit;

	public KeyCode jumpKey { get; private set; }

	public KeyCode upKey { get; private set; }

	public KeyCode leftKey { get; private set; }

	public KeyCode downKey { get; private set; }

	public KeyCode rightKey { get; private set; }

	public KeyCode slideKey { get; private set; }

	public Camera cam { get; private set; }

	public Transform t { get; private set; }

	public MouseLook mouseLook { get; private set; }

	public PerlinShake shake { get; private set; }

	public bool IsLookingAround { get; private set; }

	private void Awake()
	{
		t = base.transform;
		tCam = t.Find("Camera").transform;
		cam = tCam.GetComponent<Camera>();
		mouseLook = GetComponent<MouseLook>();
		shake = GetComponentInChildren<PerlinShake>();
		listener = GetComponentInChildren<AudioListener>();
		mouseLook.enabled = false;
		Cursor.lockState = CursorLockMode.None;
		upKey = inputs.playerKeys[0].key;
		leftKey = inputs.playerKeys[1].key;
		downKey = inputs.playerKeys[2].key;
		rightKey = inputs.playerKeys[3].key;
		slideKey = inputs.playerKeys[5].key;
		jumpKey = inputs.playerKeys[6].key;
	}

	public void Activate()
	{
		base.enabled = true;
		listener.enabled = true;
	}

	public void Deactivate()
	{
		base.enabled = false;
		listener.enabled = false;
	}

	private bool MouseRaycast()
	{
		ray = cam.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray, out hit, 1000f, 1);
		return hit.distance != 0f;
	}

	private void Update()
	{
		if (!PlayerController.instance.gameObject.activeInHierarchy)
		{
			zTemp = 0f;
			zTemp += (Input.GetKey(upKey) ? 1 : 0);
			zTemp += (Input.GetKey(downKey) ? (-1) : 0);
			xTemp = 0f;
			xTemp += (Input.GetKey(leftKey) ? (-1) : 0);
			xTemp += (Input.GetKey(rightKey) ? 1 : 0);
			yTemp = 0f;
			yTemp += (Input.GetKey(KeyCode.Space) ? 1 : 0);
			yTemp += (Input.GetKey(KeyCode.LeftAlt) ? (-1) : 0);
			z = Mathf.Lerp(z, zTemp * (Input.GetKey(KeyCode.LeftShift) ? 2.5f : 1f), Time.deltaTime * 6f);
			x = Mathf.Lerp(x, xTemp * (Input.GetKey(KeyCode.LeftShift) ? 2.5f : 1f), Time.deltaTime * 6f);
			y = Mathf.Lerp(y, yTemp * (Input.GetKey(KeyCode.LeftShift) ? 2.5f : 1f), Time.deltaTime * 6f);
			if (z.Abs() > 0f)
			{
				t.Translate(Vector3.forward * (z * Time.deltaTime * 20f));
			}
			if (x.Abs() > 0f)
			{
				t.Translate(Vector3.right * (x * Time.deltaTime * 20f));
			}
			if (y.Abs() > 0f)
			{
				t.Translate(Vector3.up * (y * Time.deltaTime * 20f), Space.World);
			}
			cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, IsLookingAround ? 85 : 90, Time.deltaTime * 8f);
			if (IsLookingAround != Input.GetMouseButton(1))
			{
				IsLookingAround = !IsLookingAround;
				mouseLook.enabled = IsLookingAround;
				Cursor.lockState = (IsLookingAround ? CursorLockMode.Locked : CursorLockMode.None);
			}
		}
	}
}
