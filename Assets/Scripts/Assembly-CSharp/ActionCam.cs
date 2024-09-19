using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ActionCam : MonoBehaviour
{
	public static ActionCam instance;

	public CanvasGroup cg;

	public Color colorA;

	public Color colorB;

	public Text lockA;

	public Text lockB;

	public Text orbitalA;

	public Text orbitalB;

	private Vector3 inputDir;

	private Vector3 inputDir2;

	private float translateSpeed;

	private float orbitalSpeedX;

	private float orbitalSpeedZ;

	private float rotateSpeed;

	private float fovSpeed;

	private float triggers;

	private Vector3 pos;

	private Vector3 rot;

	private Vector3 shake;

	private Camera cam;

	private MouseLook mouseLook;

	private Vector3 target;

	private Transform tCam;

	private Transform t;

	private bool keepSpeed;

	private RaycastHit hit;

	private void Awake()
	{
		instance = this;
		cam = GetComponentInChildren<Camera>();
		tCam = cam.transform;
		mouseLook = GetComponent<MouseLook>();
		t = base.transform;
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Combine(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(OnEnemyDied));
	}

	private void OnEnable()
	{
		t.position = Game.player.tHead.position;
		mouseLook.LookInDir(Game.player.tHead.forward);
		translateSpeed = (rotateSpeed = (fovSpeed = 0f));
		orbitalSpeedX = 0f;
		orbitalSpeedZ = 0f;
		cam.fieldOfView = Camera.main.fieldOfView;
		fovSpeed = 10f;
		keepSpeed = false;
		Text text = lockA;
		Color color2 = (lockB.color = colorA);
		text.color = color2;
		target = Vector3.zero;
		Text text2 = orbitalA;
		color2 = (orbitalB.color = colorA);
		text2.color = color2;
	}

	private void OnDisable()
	{
		Game.fading.InstantFade(1f);
		Game.fading.Fade(0f);
	}

	private void OnDestroy()
	{
		keepSpeed = true;
		Text text = lockA;
		Color color2 = (lockB.color = (keepSpeed ? colorB : colorA));
		text.color = color2;
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Remove(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(OnEnemyDied));
	}

	private void OnEnemyDied(BaseEnemy enemy)
	{
	}

	private void UpdatePosition()
	{
		if (!keepSpeed)
		{
			inputDir.x = Input.GetAxis("Horizontal");
			inputDir.z = Input.GetAxis("Vertical");
			if (inputDir.sqrMagnitude > 0.25f)
			{
				pos = inputDir;
				translateSpeed = Mathf.Lerp(translateSpeed, 10 * ((!Input.GetKey(KeyCode.LeftShift)) ? 1 : 4), Time.unscaledDeltaTime);
			}
			else
			{
				translateSpeed = Mathf.Lerp(translateSpeed, 0f, Time.unscaledDeltaTime);
			}
		}
		t.Translate(pos * translateSpeed * Time.unscaledDeltaTime, Space.Self);
	}

	private void Capture()
	{
		int pixelHeight = cam.pixelHeight;
		int pixelWidth = cam.pixelWidth;
		RenderTexture renderTexture = new RenderTexture(pixelWidth, pixelHeight, 24);
		cam.targetTexture = renderTexture;
		cam.clearFlags = CameraClearFlags.Skybox;
		cam.Render();
		RenderTexture.active = renderTexture;
		Texture2D texture2D = new Texture2D(pixelWidth, pixelHeight, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, pixelWidth, pixelHeight), 0, 0);
		texture2D.Apply();
		cam.targetTexture = null;
		cam.clearFlags = CameraClearFlags.Depth;
		RenderTexture.active = null;
		byte[] bytes = texture2D.EncodeToPNG();
		if (!Directory.Exists(Application.persistentDataPath + "\\Screenshots"))
		{
			Directory.CreateDirectory(Application.persistentDataPath + "\\Screenshots");
		}
		File.WriteAllBytes(Application.persistentDataPath + "\\Screenshots\\" + DateTime.UtcNow.ToString().Replace(':', '.') + ".png", bytes);
	}

	private void UpdateRotation()
	{
		if (!keepSpeed)
		{
			inputDir2.y = Input.GetAxis("Horizontal2");
			inputDir2.x = Input.GetAxis("Vertical2");
			if (inputDir2.sqrMagnitude > 0.25f)
			{
				rot = inputDir2;
				rotateSpeed = Mathf.Lerp(rotateSpeed, 90f, Time.unscaledDeltaTime);
			}
			else
			{
				rotateSpeed = Mathf.Lerp(rotateSpeed, 0f, Time.unscaledDeltaTime);
			}
			t.Rotate(rot * rotateSpeed * Time.unscaledDeltaTime, Space.Self);
		}
	}

	private void UpdateFOV()
	{
		if (!keepSpeed)
		{
			triggers = Input.GetAxis("Mouse Wheel");
			if (triggers > 0.05f || triggers < -0.05f)
			{
				fovSpeed = Mathf.Lerp(fovSpeed, 100f * Mathf.Sign(triggers), Time.unscaledDeltaTime * 2f);
			}
			else
			{
				fovSpeed = Mathf.Lerp(fovSpeed, 0f, Time.unscaledDeltaTime * 2f);
			}
		}
		if (fovSpeed != 0f)
		{
			cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + fovSpeed * Time.unscaledDeltaTime, 10f, 170f);
		}
	}

	private void UpdateShake()
	{
		if (translateSpeed != 0f)
		{
			float num = 4f;
			shake.x = Mathf.PerlinNoise((0f - t.position.x) / num, t.position.y / num);
			shake.y = Mathf.PerlinNoise(t.position.y / num, (0f - t.position.z) / num);
			shake.z = Mathf.PerlinNoise((0f - t.position.z) / num, t.position.x / num);
			tCam.localEulerAngles = shake * Mathf.Abs(translateSpeed) / 20f;
		}
	}

	private void ClampPosition()
	{
		Vector3 position = default(Vector3);
		position.x = t.position.x;
		position.z = t.position.z;
		position.y = Mathf.Clamp(t.position.y, OceanScript.WaterHeightAtPoint(t.position, global: true) + 1f, 200f);
		t.position = position;
	}

	private void LateUpdate()
	{
		ClampPosition();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			cg.alpha = ((cg.alpha != 1f) ? 1 : 0);
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			Physics.Raycast(t.position, t.forward, out hit, 50f, 1);
			if (hit.distance != 0f && hit.normal.y == 1f)
			{
				PlayerController.instance.gameObject.SetActive(value: true);
				PlayerController.instance.t.position = hit.point + Vector3.up;
				PlayerController.instance.mouseLook.LookInDir(Vector3.ProjectOnPlane(t.forward, Vector3.up));
				Game.fading.InstantFade(1f);
				Game.fading.Fade(0f);
				base.gameObject.SetActive(value: false);
			}
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			if (target.sqrMagnitude == 0f)
			{
				Physics.Raycast(t.position, t.forward, out var hitInfo, 1000f, 1);
				if (hitInfo.distance != 0f)
				{
					target = hitInfo.point;
				}
				else
				{
					target = t.position + t.forward * 20f;
				}
				orbitalSpeedX = 2f;
				orbitalSpeedZ = 0f;
				keepSpeed = false;
				Text text = lockA;
				Color color2 = (lockB.color = (keepSpeed ? colorB : colorA));
				text.color = color2;
				Text text2 = orbitalA;
				color2 = (orbitalB.color = colorB);
				text2.color = color2;
			}
			else
			{
				mouseLook.LookInDir(t.forward);
				target = Vector3.zero;
				translateSpeed = 0f;
				rotateSpeed = 0f;
				fovSpeed = 0f;
				Text text3 = orbitalA;
				Color color2 = (orbitalB.color = colorA);
				text3.color = color2;
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			Game.wideMode.Set(-1f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			shake = Vector3.zero;
			t.position = t.position.Snap();
			mouseLook.LookInDir(t.forward.Snap());
			translateSpeed = 0f;
			rotateSpeed = 0f;
			fovSpeed = 0f;
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			keepSpeed = !keepSpeed;
			Text text4 = lockA;
			Color color2 = (lockB.color = (keepSpeed ? colorB : colorA));
			text4.color = color2;
		}
		if (target.sqrMagnitude > 0f)
		{
			inputDir.x = Input.GetAxis("Horizontal");
			inputDir.z = Input.GetAxis("Vertical");
			if (!keepSpeed)
			{
				orbitalSpeedX = Mathf.Lerp(orbitalSpeedX, inputDir.x * 20f, Time.unscaledDeltaTime);
				orbitalSpeedZ = Mathf.Lerp(orbitalSpeedZ, inputDir.z * 20f, Time.unscaledDeltaTime);
			}
			mouseLook.enabled = false;
			UpdateFOV();
			t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(t.position.DirTo(target)), Time.unscaledDeltaTime * 10f);
			t.Translate(Vector3.right * orbitalSpeedX * Time.unscaledDeltaTime, Space.Self);
			t.Translate(Vector3.forward * orbitalSpeedZ * Time.unscaledDeltaTime, Space.Self);
		}
		else
		{
			mouseLook.enabled = true;
			UpdatePosition();
			UpdateRotation();
			UpdateFOV();
			UpdateShake();
		}
	}
}
