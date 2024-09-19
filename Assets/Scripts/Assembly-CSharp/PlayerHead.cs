using System;
using UnityEngine;

public class PlayerHead : MonoBehaviour, IDamageable<Vector4>
{
	public static Action OnGameQuickReset = delegate
	{
	};

	public PlayerController p;

	public Transform t;

	public Rigidbody rb;

	public CanBeDrowned canBeDrowned;

	public Camera cam;

	public PerlinShake shake;

	public AudioListener audioListener;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private bool instaRespawn;

	private bool readyToRespawn;

	private float startFov;

	private float lifetime;

	public void Damage(Vector4 dir)
	{
		rb.AddForce(Vector3.one * 20f, ForceMode.Impulse);
	}

	private void Awake()
	{
		canBeDrowned = GetComponent<CanBeDrowned>();
		CanBeDrowned obj = canBeDrowned;
		obj.OnDrowned = (Action)Delegate.Combine(obj.OnDrowned, new Action(RestorePlayer));
	}

	private void OnDestroy()
	{
		CanBeDrowned obj = canBeDrowned;
		obj.OnDrowned = (Action)Delegate.Remove(obj.OnDrowned, new Action(RestorePlayer));
	}

	private void Start()
	{
		cam.enabled = false;
		audioListener.enabled = false;
		rb.isKinematic = true;
		if ((bool)t.parent)
		{
			t.SetParent(null);
			t.position = SavePoint.lastSavepoint.t.position;
		}
	}

	public void Chop(Transform tCam, Vector3 dir, bool insta)
	{
		lifetime = 0f;
		readyToRespawn = false;
		instaRespawn = insta;
		canBeDrowned.Reset();
		t.SetPositionAndRotation(tCam.position, tCam.rotation);
		cam.fieldOfView = (startFov = Game.player.camController.worldCam.fieldOfView);
		shake.Shake();
		p.camController.EnableCameraAndListener(value: false);
		cam.enabled = true;
		audioListener.enabled = true;
		LastActiveCamera.Set(cam);
		rb.isKinematic = false;
		rb.AddForce(dir * 5f, ForceMode.Impulse);
		rb.AddTorque(0f, 0f, 0.1f, ForceMode.Impulse);
	}

	public void RestorePlayer()
	{
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		rb.isKinematic = true;
		cam.enabled = false;
		audioListener.enabled = false;
		p.camController.EnableCameraAndListener(value: true);
		p.Reset();
		Game.time.StopSlowmo();
		Game.fading.InstantFade(1f);
		Game.fading.Fade(0f);
		if (OnGameQuickReset != null)
		{
			OnGameQuickReset();
		}
	}

	private void Update()
	{
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 110f, Time.deltaTime * 4f);
		lifetime += Time.unscaledDeltaTime;
		if (Game.loading != null || !p.isDead || !(lifetime > 0.3f))
		{
			return;
		}
		if (!readyToRespawn)
		{
			if (!instaRespawn)
			{
				Game.message.Show($"Press {((!PlayerController.gamepad) ? p.inputs.playerKeys[7].key.ToString() : p.inputs.playerKeys[7].joy)} to Restart");
			}
			else
			{
				RestorePlayer();
			}
			readyToRespawn = true;
		}
		if (p.RestartPressed())
		{
			RestorePlayer();
		}
	}
}
