using System;
using UnityEngine;

public class QuickmapTransformHandle : MonoBehaviour
{
	private Transform t;

	public Transform[] tHandles;

	public QuickmapHandle[] handles;

	public Transform tFreeMoveHandle;

	public Transform tTargetHandle;

	public Transform tPosHandles;

	public Transform tRotHandles;

	public Transform tBounds;

	private Vector3 pos;

	private Vector3 finalPos;

	private Vector3 offset;

	private SetupableMonobehavior setupableObject;

	private Ray ray;

	private Plane plane;

	private RaycastHit hit;

	public Camera cam;

	private bool isSelectiong;

	private bool inPickableRange;

	private int activeHandleIndex;

	private Vector3 lastMousePos;

	private Vector3 angles;

	private int mode;

	public bool somethingSelected => tSelected;

	public Transform tSelected { get; private set; }

	public bool isDragging { get; private set; }

	private void Awake()
	{
		t = base.transform;
		handles = GetComponentsInChildren<QuickmapHandle>();
		tTargetHandle.SetParent(null);
		QuickmapScene.OnPlayMode = (Action)Delegate.Combine(QuickmapScene.OnPlayMode, new Action(Deselect));
		base.gameObject.SetActive(value: false);
	}

	public void OnDisable()
	{
		QuickmapScene.OnPlayMode = (Action)Delegate.Remove(QuickmapScene.OnPlayMode, new Action(Deselect));
		tTargetHandle.gameObject.SetActive(value: false);
	}

	public void TryToSelect()
	{
		ray = cam.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray, out hit, 1000f, 685089);
		if (hit.distance != 0f)
		{
			if (hit.collider.gameObject.layer != 5 && !hit.collider.CompareTag("Unselectable"))
			{
				Select(hit.transform);
			}
			else
			{
				Select(null);
			}
		}
		else
		{
			Select(null);
		}
	}

	public void Deselect()
	{
		Select(null);
	}

	public void Select(Transform newSelected)
	{
		if ((bool)newSelected)
		{
			SwitchMode(0);
			tSelected = newSelected.root;
			t.position = tSelected.position;
			setupableObject = tSelected.GetComponentInChildren<SetupableMonobehavior>();
			if ((bool)setupableObject)
			{
				tTargetHandle.gameObject.SetActive(value: true);
				tTargetHandle.position = setupableObject.GetWorldTargetPosition();
			}
			else
			{
				tTargetHandle.gameObject.SetActive(value: false);
			}
			base.gameObject.SetActive(value: true);
		}
		else
		{
			tSelected = null;
			newSelected = null;
			setupableObject = null;
			base.gameObject.SetActive(value: false);
		}
	}

	private void SwitchMode(int value)
	{
		mode = value;
		switch (mode)
		{
		case 0:
			tFreeMoveHandle.gameObject.SetActive(value: false);
			tPosHandles.gameObject.SetActive(value: false);
			tRotHandles.gameObject.SetActive(value: false);
			break;
		case 1:
			tFreeMoveHandle.gameObject.SetActive(value: false);
			tPosHandles.gameObject.SetActive(value: true);
			tRotHandles.gameObject.SetActive(value: false);
			break;
		case 2:
			tFreeMoveHandle.gameObject.SetActive(value: false);
			tPosHandles.gameObject.SetActive(value: false);
			tRotHandles.gameObject.SetActive(value: true);
			break;
		}
	}

	private void Update()
	{
		if (!tSelected)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			SwitchMode(mode.Next(3));
		}
		if (!Input.GetMouseButton(1))
		{
			if (!isDragging)
			{
				activeHandleIndex = -1;
				float num = 0.0625f * ((float)Screen.width / cam.aspect);
				for (int i = 0; i < tHandles.Length; i++)
				{
					if (tHandles[i].gameObject.activeInHierarchy)
					{
						Vector2 b = cam.WorldToScreenPoint(tHandles[i].position);
						float num2 = Vector2.Distance(Input.mousePosition, b);
						if (num2 < num)
						{
							activeHandleIndex = i;
							num = num2;
						}
					}
				}
				for (int j = 0; j < tHandles.Length; j++)
				{
					handles[j].Select(j == activeHandleIndex);
				}
			}
		}
		else if (activeHandleIndex != -1)
		{
			activeHandleIndex = -1;
			for (int k = 0; k < tHandles.Length; k++)
			{
				handles[k].Select(k == activeHandleIndex);
			}
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			tSelected.Rotate(Vector3.up * 90f, Space.World);
		}
		if (Input.GetKeyDown(KeyCode.X) && tSelected.TryGetComponent<QuickmapObject>(out var component) && (component.Deleteable || !component))
		{
			QuickmapScene.instance.DeletePrefab(tSelected.gameObject);
			Select(null);
		}
		if ((bool)setupableObject && Input.GetKeyDown(KeyCode.E))
		{
			ray = cam.ScreenPointToRay(Input.mousePosition);
			Physics.Raycast(ray, out hit, 1000f, 1);
			if (hit.distance != 0f)
			{
				setupableObject.SetTargetPosition(hit.point.Snap());
				tTargetHandle.position = setupableObject.GetWorldTargetPosition();
			}
		}
		if (!isDragging)
		{
			if (Input.GetMouseButtonDown(0) && QuickmapScene.instance.editBox.SelectedHovered)
			{
				isDragging = true;
				Collider[] componentsInChildren = tSelected.GetComponentsInChildren<Collider>();
				for (int l = 0; l < componentsInChildren.Length; l++)
				{
					componentsInChildren[l].enabled = false;
				}
				ray = QuickmapScene.instance.editorCamera.cam.ScreenPointToRay(Input.mousePosition);
				Physics.Raycast(ray, out hit, 1000f, 4097);
				if (hit.distance != 0f)
				{
					Vector3 v = hit.point + hit.normal * 3f;
					QuickmapScene.instance.megaCubeWorld.Snap(ref v);
					pos = v;
				}
			}
			return;
		}
		if (Input.GetMouseButton(0))
		{
			ray = QuickmapScene.instance.editorCamera.cam.ScreenPointToRay(Input.mousePosition);
			Physics.Raycast(ray, out hit, 1000f, 4097);
			if (hit.distance != 0f)
			{
				Vector3 v2 = hit.point + hit.normal * 3f;
				QuickmapScene.instance.megaCubeWorld.Snap(ref v2);
				pos = v2;
				t.position = pos;
			}
		}
		if (Input.GetMouseButtonUp(0))
		{
			finalPos.x = Mathf.RoundToInt(pos.x / 6f) * 6;
			finalPos.z = Mathf.RoundToInt(pos.z / 6f) * 6;
			finalPos.y = pos.y;
			isDragging = false;
			Collider[] componentsInChildren = tSelected.GetComponentsInChildren<Collider>();
			for (int l = 0; l < componentsInChildren.Length; l++)
			{
				componentsInChildren[l].enabled = true;
			}
		}
	}

	private void LateUpdate()
	{
		if (!tSelected)
		{
			return;
		}
		float num = Vector3.Distance(t.position, QuickmapScene.instance.editorCamera.t.position);
		switch (mode)
		{
		case 1:
			tPosHandles.localScale = Vector3.one * (num / 9f);
			break;
		case 2:
			tRotHandles.localScale = Vector3.one * (num / 9f);
			break;
		}
		if (tSelected.position != tHandles[0].position)
		{
			tSelected.position = tHandles[0].position;
			if ((bool)setupableObject)
			{
				setupableObject.SetTargetPosition(tTargetHandle.position);
			}
		}
		tRotHandles.rotation = tSelected.rotation;
	}
}
