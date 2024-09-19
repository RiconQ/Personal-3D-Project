using UnityEngine;

public class QuickmapEditBox : MonoBehaviour
{
	public Transform tEditBox;

	public Transform tCursor;

	public MeshRenderer rendEditBox;

	public MegaCubeWorld megacube;

	public Texture2D addTex;

	public Texture2D subTex;

	private int sign;

	private int extrude;

	private float extrusion;

	private Vector3 temp;

	private Vector3Int posA;

	private Vector3Int posB;

	private Vector3 tempNormal;

	private MaterialPropertyBlock block;

	private Ray ray;

	private RaycastHit hit;

	private Camera cam;

	public Texture2D TexCursorDefault;

	public Texture2D TexCursorDrag;

	public CursorMode CursorMode;

	public Vector2 HotSpot = Vector2.zero;

	public bool SelectedHovered;

	public bool isEditing { get; private set; }

	private void Awake()
	{
		cam = GetComponentInChildren<Camera>();
		rendEditBox.enabled = false;
		block = new MaterialPropertyBlock();
		rendEditBox.GetPropertyBlock(block);
	}

	private bool MouseRaycast()
	{
		ray = cam.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray, out hit, 1000f, 4097);
		return hit.distance != 0f;
	}

	private void OnDisable()
	{
		CursorUpdate();
	}

	private void Update()
	{
		CursorUpdate();
		if (!isEditing)
		{
			if (!Input.GetKeyDown(KeyCode.Minus) && !Input.GetKeyDown(KeyCode.Equals))
			{
				return;
			}
			sign = ((!Input.GetKey(KeyCode.Minus)) ? 1 : (-1));
			if (MouseRaycast())
			{
				if (QuickmapScene.instance.selectionHandle.somethingSelected)
				{
					QuickmapScene.instance.selectionHandle.Select(null);
				}
				megacube.TranslateWorldPosition(hit.point + hit.normal * (sign * megacube.side / 2), out posA);
				megacube.posA = posA;
				tEditBox.position = posA;
				temp.x = (temp.y = (temp.z = megacube.side));
				tEditBox.localScale = temp;
				bool flag2 = (rendEditBox.enabled = true);
				isEditing = flag2;
				tempNormal = hit.normal;
				extrude = 0;
				extrusion = 0f;
				block.SetTexture("_MainTex", (sign == 1) ? addTex : subTex);
				rendEditBox.SetPropertyBlock(block);
			}
			return;
		}
		float num = (0f - Input.GetAxisRaw("Mouse Wheel")) * 10f;
		if (num.Abs() != 0f)
		{
			extrusion += num;
		}
		if (sign == 1)
		{
			extrusion = Mathf.Clamp(extrusion, 0f, float.PositiveInfinity);
		}
		if (sign == -1)
		{
			extrusion = Mathf.Clamp(extrusion, float.NegativeInfinity, 0f);
		}
		if ((float)extrude != Mathf.Round(extrusion) * (float)megacube.side)
		{
			extrude = Mathf.RoundToInt(extrusion) * megacube.side;
		}
		Debug.DrawRay(hit.point, hit.normal * (1 + extrude), Color.yellow);
		if (MouseRaycast())
		{
			megacube.TranslateWorldPosition(hit.point + hit.normal * (sign * megacube.side / 2), out posB);
			posB += new Vector3Int((int)tempNormal.x * extrude, (int)tempNormal.y * extrude, (int)tempNormal.z * extrude);
			if (posA != posB)
			{
				tEditBox.position = (posA + posB) / 2;
				temp = posB - posA;
				temp.x = Mathf.Abs(temp.x) + (float)megacube.side;
				temp.y = Mathf.Abs(temp.y) + (float)megacube.side;
				temp.z = Mathf.Abs(temp.z) + (float)megacube.side;
				tEditBox.localScale = temp;
			}
		}
		if ((sign == -1 && !Input.GetKey(KeyCode.Minus)) || (sign == 1 && !Input.GetKey(KeyCode.Equals)))
		{
			megacube.AddArea(posA, posB, sign == 1);
			if (sign == -1)
			{
				rendEditBox.GetComponent<ParticleSystem>().Play();
			}
			bool flag2 = (rendEditBox.enabled = false);
			isEditing = flag2;
		}
	}

	private void CursorUpdate()
	{
		if (QuickmapScene.instance.editorCamera.enabled && !QuickmapScene.instance.editorCamera.IsLookingAround && !QuickmapScene.instance.isPlaymode && MouseRaycast())
		{
			if (!tCursor.gameObject.activeInHierarchy)
			{
				tCursor.gameObject.SetActive(value: true);
			}
			Vector3 position = hit.point + hit.normal * 3f;
			position.x = Mathf.RoundToInt(position.x / 6f) * 6;
			position.y = Mathf.RoundToInt(position.y / 6f) * 6;
			position.z = Mathf.RoundToInt(position.z / 6f) * 6;
			position -= hit.normal * 3f;
			Quaternion rotation = Quaternion.LookRotation(-hit.normal);
			tCursor.SetPositionAndRotation(position, rotation);
			SelectedHovered = false;
			if ((bool)QuickmapScene.instance.selectionHandle.tSelected)
			{
				SelectedHovered = hit.transform.root == QuickmapScene.instance.selectionHandle.tSelected;
				QuickmapScene.instance.selectionHandle.tBounds.GetComponent<MeshRenderer>().material.color = (SelectedHovered ? Color.green : Color.white);
				Cursor.SetCursor(SelectedHovered ? TexCursorDrag : TexCursorDefault, HotSpot, CursorMode);
			}
			else
			{
				QuickmapScene.instance.selectionHandle.tBounds.GetComponent<MeshRenderer>().material.color = Color.white;
				Cursor.SetCursor(TexCursorDefault, HotSpot, CursorMode);
			}
		}
		else if (tCursor.gameObject.activeInHierarchy)
		{
			tCursor.gameObject.SetActive(value: false);
			SelectedHovered = false;
			QuickmapScene.instance.selectionHandle.tBounds.GetComponent<MeshRenderer>().material.color = Color.white;
		}
	}
}
