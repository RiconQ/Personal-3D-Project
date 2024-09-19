using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public static CameraBob bob;

	public static CameraSway sway;

	public static PerlinShake shake;

	public Camera worldCam;

	public Camera playerCam;

	public static AudioListener audioListener;

	public static Transform tCam;

	private Transform t;

	private Vector3 angles;

	private void Awake()
	{
		bob = GetComponentInChildren<CameraBob>();
		sway = GetComponentInChildren<CameraSway>();
		shake = GetComponentInChildren<PerlinShake>();
		audioListener = GetComponentInChildren<AudioListener>();
		t = base.transform;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
	}

	public void EnableCameraAndListener(bool value)
	{
		worldCam.enabled = value;
		playerCam.enabled = value;
		audioListener.enabled = value;
		if (value)
		{
			LastActiveCamera.Set(worldCam);
		}
	}

	public void Angle(float z, float speed = 6f)
	{
		angles.y = 0f;
		if (speed != 0f)
		{
			angles.z = Mathf.LerpAngle(angles.z, z, Time.deltaTime * 6f);
		}
		else
		{
			angles.z = z;
		}
	}

	private void Reset()
	{
		angles.x = (angles.y = (angles.z = 0f));
		shake.Reset();
	}

	private void LateUpdate()
	{
		angles.x = Mathf.LerpAngle(angles.x, Mathf.Clamp(Game.player.v, -1f, 0f) * 5f, Time.deltaTime * 4f);
		t.localEulerAngles = angles;
		t.localEulerAngles += shake.finalShake;
	}
}
