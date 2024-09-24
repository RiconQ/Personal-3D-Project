using System;
using System.Collections.Generic;
using UnityEngine;

public class PullableControl : MonoBehaviour
{
	public static PullableControl instance;

	public static List<Transform> pullables = new List<Transform>(50);

	private const int _MAX_DIST = 19;

	private const int _MAX_ANGLE = 25;

	public static Transform target;

	private RaycastHit hit;

	private float camAspect;

	private float scrDist;

	private float maxScrDist = 0.15f;

	private float maxDist = 8f;

	private float closestAngle;

	private float currentAngle;

	private Transform temp;

	private Vector2 scrDir;

	private Vector2 scrPos;

	private Vector2 scrCenter = new Vector2(0.5f, 0.5f);

	private SimpleFlare flare;

	private bool targetUnreachable;

	private void Awake()
	{
		instance = this;
		flare = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Pull Flare"), null, instantiateInWorldSpace: true) as GameObject).GetComponent<SimpleFlare>();
		Game.OnAnyLevelUnloaded = (Action)Delegate.Combine(Game.OnAnyLevelUnloaded, new Action(OnSceneUnloaded));
	}

	private void Start()
	{
		if ((bool)Game.player)
		{
			camAspect = Game.player.camController.worldCam.aspect;
		}
	}

	private void OnDestroy()
	{
		Game.OnAnyLevelUnloaded = (Action)Delegate.Remove(Game.OnAnyLevelUnloaded, new Action(OnSceneUnloaded));
	}

	private void OnSceneUnloaded()
	{
		if (pullables.Count > 0)
		{
			pullables.Clear();
		}
	}

	public static bool GetCurrent(out Transform t)
	{
		t = target;
		return t;
	}

	private void FixedUpdate()
	{
		if (!Game.player)
		{
			return;
		}
		target = null;
		maxScrDist = 0.15f;
		if (Game.player.weapons.daggerController.state != 0)
		{
			return;
		}
		foreach (Transform pullable in pullables)
		{
			if (!pullable.gameObject.activeInHierarchy || 
				pullable.position.y - Game.player.t.position.y < 0f || 
				Vector3.Dot(Game.player.tHead.forward, Game.player.tHead.position.DirTo(pullable.position)) < 0f)
			{
				continue;
			}
			Physics.Raycast(Game.player.tHead.position, Game.player.tHead.position.DirTo(pullable.position), out hit, 19f, 540673);
			if (hit.distance != 0f && hit.collider.gameObject.layer == 19)
			{
				scrPos = Game.player.camController.worldCam.WorldToScreenPoint(pullable.position);
				scrPos.x /= Screen.width;
				scrPos.y /= Screen.height;
				scrDir = scrCenter - scrPos;
				scrDir.x *= camAspect;
				scrDist = scrDir.magnitude;
				if (scrDist < maxScrDist)
				{
					maxScrDist = scrDist;
					target = pullable;
				}
			}
		}
	}

	private void LateUpdate()
	{
		if ((bool)target && !Game.player.weapons.daggerController.dagger.hoockedClldr)
		{
			if (!flare.gameObject.activeInHierarchy)
			{
				flare.gameObject.SetActive(value: true);
			}
			flare.t.position = target.position;
			Debug.DrawLine(Game.player.t.position, target.position, Color.magenta);
		}
		else if (flare.gameObject.activeInHierarchy)
		{
			flare.gameObject.SetActive(value: false);
		}
	}
}
