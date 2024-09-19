using UnityEngine;

public class KeyartMockupCamera : MonoBehaviour
{
	private Transform t;

	private Quaternion startRotation;

	private MouseLook mouseLook;

	private void Awake()
	{
		t = base.transform;
		startRotation = t.rotation;
		mouseLook = GetComponent<MouseLook>();
	}

	private void Update()
	{
		if (Input.GetMouseButton(0))
		{
			mouseLook.enabled = true;
		}
		else
		{
			mouseLook.enabled = false;
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			t.rotation = startRotation;
		}
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		if (axis.Abs() > 0.1f)
		{
			t.Translate(t.right * axis * Time.deltaTime);
		}
		if (axis2.Abs() > 0.1f)
		{
			t.Translate(t.forward * axis2 * Time.deltaTime);
		}
	}
}
