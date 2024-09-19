using System;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
	private Vector3 angles;

	private Camera cam;

	private Transform tCam;

	private float hSpeed;

	private float h;

	private bool active;

	public QUI_Title title;

	private void Awake()
	{
		cam = GetComponentInChildren<Camera>();
		tCam = cam.transform;
		hSpeed = -30f;
		QUI_Title qUI_Title = title;
		qUI_Title.OnActivate = (Action<bool>)Delegate.Combine(qUI_Title.OnActivate, new Action<bool>(Activate));
		base.transform.position = new Vector3(1f, 0.5f, 0f);
	}

	private void Activate(bool value)
	{
		if (active != !value)
		{
			active = !value;
		}
	}

	private void Update()
	{
		if (active)
		{
			if (Input.GetButton("Fire1"))
			{
				h = Mathf.Clamp(Input.GetAxis("Mouse X") / 4f, -1f, 1f);
			}
			base.transform.position = Vector3.Lerp(base.transform.position, Vector3.zero, Time.deltaTime * 4f);
		}
		else
		{
			h = 0f;
			base.transform.position = Vector3.Lerp(base.transform.position, new Vector3(1f, 0.5f, 0f), Time.deltaTime * 2f);
		}
		if (h.Abs() > 0.25f)
		{
			hSpeed = Mathf.Lerp(hSpeed, 90f * h, Time.deltaTime * 10f);
		}
		else
		{
			hSpeed = Mathf.Lerp(hSpeed, 0f, Time.deltaTime * 2f);
		}
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 55 - (Input.GetButton("Fire1") ? 5 : 0), Time.deltaTime * 4f);
		angles.x = Mathf.PerlinNoise(tCam.position.x / 4f, tCam.position.z / 4f);
		angles.y = Mathf.PerlinNoise(tCam.position.y / 4f, tCam.position.x / 4f);
		angles.z = Mathf.LerpAngle(tCam.localEulerAngles.z, h * 2.5f, Time.deltaTime * 4f);
		tCam.localEulerAngles = angles;
		base.transform.Rotate(Vector3.up * hSpeed * Time.deltaTime);
	}
}
