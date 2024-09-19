using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class QuickmapScene : MonoBehaviour
{
	public enum QuickmapState
	{
		nothing = 0,
		selection = 1,
		editing = 2
	}

	public static Action OnPlayMode = delegate
	{
	};

	public static Action OnEditMode = delegate
	{
	};

	public static Action<int> OnStateChanged = delegate
	{
	};

	public static QuickmapScene instance;

	public QuickmapState state;

	public PrefabsToolbar prefabsToolbar;

	public QuickmapTransformHandle selectionHandle;

	public QuickmapCamera editorCamera;

	public QuickmapEditBox editBox;

	public MegaCubeWorld megaCubeWorld;

	public NavMeshSurface surface;

	public Canvas canvas;

	public CanvasGroup cgEditorTools;

	public Transform tSpawn;

	public OffMeshLinkManager links;

	public PrefabsCollection collection;

	public List<GameObject> objectsToSave = new List<GameObject>(1024);

	public Dictionary<Vector3Int, GameObject> instantiatedObjects = new Dictionary<Vector3Int, GameObject>(1024);

	private RaycastHit hit;

	private Ray ray;

	private PlayerController player;

	public bool isPlaymode { get; private set; }

	private void Awake()
	{
		instance = this;
		links = UnityEngine.Object.FindObjectOfType<OffMeshLinkManager>();
		Game.fading.speed = 2f;
	}

	private void Start()
	{
		editorCamera.gameObject.SetActive(value: true);
		Quickmap.LoadQuickmap(this);
	}

	private void OnDestroy()
	{
		instance = null;
		Quickmap.pathMyDownloads = "";
	}

	private void Update()
	{
		cgEditorTools.alpha = Mathf.Lerp(cgEditorTools.alpha, editorCamera.isActiveAndEnabled ? 1f : 0.1f, Time.deltaTime * 10f);
		if (!editorCamera.isActiveAndEnabled && !isPlaymode)
		{
			return;
		}
		int num = (selectionHandle.somethingSelected ? 1 : (editBox.isEditing ? 2 : 0));
		if (state != (QuickmapState)num)
		{
			state = (QuickmapState)num;
			if (OnStateChanged != null)
			{
				OnStateChanged((int)state);
			}
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			Play();
		}
		if (Input.GetKeyDown(KeyCode.F1))
		{
			string text = Application.persistentDataPath.Replace("/", "\\");
			Process.Start("explorer.exe", "/select," + text);
		}
		if (isPlaymode)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Play();
			}
			return;
		}
		if (Input.GetKeyDown(KeyCode.Q) && prefabsToolbar.GetSelectedPrefab(out var prefab))
		{
			RaycastFromMousePosition();
			if (hit.distance != 0f)
			{
				Vector3 point = hit.point;
				Quaternion identity = Quaternion.identity;
				point = hit.point + hit.normal * megaCubeWorld.side / 2f;
				megaCubeWorld.Snap(ref point);
				identity = ((hit.normal.y.Abs() < 0.5f) ? Quaternion.LookRotation(hit.normal, Vector3.up) : Quaternion.LookRotation(-editorCamera.t.forward.CardinalDirection(), Vector3.up));
				InstantiatePrefab(prefab, point, identity, Vector3.zero);
			}
		}
		if (Input.GetMouseButtonUp(0) && !selectionHandle.isDragging)
		{
			selectionHandle.TryToSelect();
		}
	}

	private void ClearObjects()
	{
		if (objectsToSave.Count > 0)
		{
			for (int i = 0; i < objectsToSave.Count; i++)
			{
				UnityEngine.Object.Destroy(objectsToSave[i]);
			}
			objectsToSave.Clear();
		}
	}

	public void Reset()
	{
		ClearObjects();
		megaCubeWorld.ClearAll();
		megaCubeWorld.FillRegionAtPoint(Vector3Int.zero);
		tSpawn.position = new Vector3(24f, 48f, 0f);
		megaCubeWorld.CreateMeshRuntime();
	}

	public void Randomise(int value = -1)
	{
	}

	public void SaveCurrentMap()
	{
		Quickmap.SaveQuickmap(this);
	}

	public void DeleteQuickmap()
	{
		Quickmap.DeleteQuickmap();
		Quickmap.customMapName = "";
		Game.instance.LoadLevel("MainMenu");
	}

	public void Play()
	{
		if (Game.fading.cg.alpha != 0f)
		{
			return;
		}
		if (!player)
		{
			player = PlayerController.instance;
		}
		if (!isPlaymode)
		{
			isPlaymode = true;
			surface.BuildNavMesh();
			if ((bool)links)
			{
				links.CreateLinks();
			}
			GC.Collect();
			canvas.gameObject.SetActive(value: false);
			editorCamera.gameObject.SetActive(value: false);
			selectionHandle.Select(null);
			player.gameObject.SetActive(value: true);
			tSpawn.GetComponentInChildren<SavePoint>().Launch();
			Game.fading.InstantFade(1f);
			Game.fading.Fade(0f);
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			if (OnPlayMode != null)
			{
				OnPlayMode();
			}
		}
		else
		{
			isPlaymode = false;
			surface.RemoveData();
			if (player.head.isActiveAndEnabled)
			{
				player.head.RestorePlayer();
			}
			player.gameObject.SetActive(value: false);
			player.Deactivate();
			player.weapons.SetWeapon(-1);
			canvas.gameObject.SetActive(value: true);
			editorCamera.gameObject.SetActive(value: true);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			surface.RemoveData();
			if (OnEditMode != null)
			{
				OnEditMode();
			}
		}
	}

	public bool RaycastFromMousePosition()
	{
		ray = editorCamera.cam.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray, out hit, 1000f, 4097);
		return hit.distance != 0f;
	}

	public void InstantiatePrefab(GameObject prefab, Vector3 pos, Quaternion rot, Vector3 target)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, pos, rot);
		gameObject.name = prefab.name;
		SetupableMonobehavior component = gameObject.GetComponent<SetupableMonobehavior>();
		if ((bool)component)
		{
			component.SetTargetPosition(target);
		}
		if (!objectsToSave.Contains(gameObject))
		{
			objectsToSave.Add(gameObject);
		}
		selectionHandle.Select(gameObject.transform);
	}

	public GameObject InstantiatePrefab(string name, Vector3 pos, Quaternion rot, Vector3 target)
	{
		GameObject gameObject = null;
		for (int i = 0; i < collection.prefabs.Length; i++)
		{
			if (collection.prefabs[i].name == name)
			{
				gameObject = collection.prefabs[i];
				break;
			}
		}
		if ((bool)gameObject)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, pos, rot);
			gameObject2.name = name;
			SetupableMonobehavior component = gameObject2.GetComponent<SetupableMonobehavior>();
			if ((bool)component)
			{
				component.SetTargetPosition(target);
			}
			if (!objectsToSave.Contains(gameObject2))
			{
				objectsToSave.Add(gameObject2);
			}
			return gameObject2;
		}
		UnityEngine.Debug.Log($"Failed to find {name}");
		return null;
	}

	public void DeletePrefab(GameObject obj)
	{
		if (objectsToSave.Contains(obj))
		{
			objectsToSave.Remove(obj);
		}
		UnityEngine.Object.Destroy(obj);
	}
}
