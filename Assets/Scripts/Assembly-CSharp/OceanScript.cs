using System;
using UnityEngine;

public class OceanScript : MonoBehaviour
{
	public static Action OnFall = delegate
	{
	};

	public static float yPosition = -1000f;

	public static float amp;

	public float amplitude = 2f;

	public bool trackPlayerPosition = true;

	private Transform t;

	private AudioClip splashSound;

	private int state;

	private float timer;

	private PlayerController p;

	private float dist;

	private Vector3 temp;

	private MeshFilter mf;

	private Vector3[] vertices;

	public static float WaterHeightAtPoint(Vector3 point, bool global = false)
	{
		return Mathf.Sin(Game.time.sinTime * 2f + point.x / 20f) * Mathf.Sin(Game.time.sinTime + point.z / 10f) * amp + (global ? yPosition : 0f);
	}

	public static float WaterHeightAtPoint(float x, float z, bool global = false)
	{
		return Mathf.Sin(Game.time.sinTime * 2f + x / 20f) * Mathf.Sin(Game.time.sinTime + z / 10f) * amp + (global ? yPosition : 0f);
	}

	private void Awake()
	{
		t = base.transform;
		yPosition = t.position.y;
		amp = amplitude;
		mf = GetComponent<MeshFilter>();
		vertices = mf.sharedMesh.vertices;
		splashSound = Resources.Load("Sounds/Water Splash") as AudioClip;
	}

	private void Start()
	{
		p = PlayerController.instance;
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			temp = vertices[i];
			temp.y = WaterHeightAtPoint(t.position.x + temp.x, t.position.z + temp.z);
			vertices[i].y = temp.y;
		}
		mf.sharedMesh.vertices = vertices;
	}

	private void LateUpdate()
	{
		if (trackPlayerPosition && (bool)p)
		{
			temp.x = LastActiveCamera.tCam.position.x;
			temp.y = yPosition;
			temp.z = LastActiveCamera.tCam.position.z;
			if (t.position != temp)
			{
				t.position = temp;
			}
		}
		if (!p || !p.isActiveAndEnabled)
		{
			return;
		}
		switch (state)
		{
		case 0:
		{
			float num = p.tHead.position.y - WaterHeightAtPoint(p.tHead.position) - t.position.y;
			Debug.DrawRay(p.tHead.position, Vector3.down * num);
			if (num < 0.5f && Game.player.inputActive && !p.rb.isKinematic && p.rb.velocity.y < 0f)
			{
				Game.fading.InstantFade(1f);
				Game.sounds.PlayClip(splashSound);
				p.Deactivate();
				p.rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
				p.rb.isKinematic = true;
				p.weapons.gameObject.SetActive(value: false);
				timer = 0f;
				state++;
				if (OnFall != null)
				{
					OnFall();
				}
			}
			break;
		}
		case 1:
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 2f);
			if (timer == 1f)
			{
				state++;
			}
			break;
		case 2:
			if ((bool)SavePoint.lastSavepoint)
			{
				if (SavePoint.lastSavepoint.GetType() == typeof(SpawnPoint))
				{
					Game.player.head.RestorePlayer();
				}
				else
				{
					SavePoint.lastSavepoint.Launch();
				}
			}
			else
			{
				p.BackToLastGrounded();
				p.rb.isKinematic = false;
				p.Activate();
				p.grounder.Grounded(forced: true);
				p.rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
				p.weapons.gameObject.SetActive(value: true);
				Game.fading.InstantFade(0f);
			}
			state = 0;
			break;
		}
	}
}
