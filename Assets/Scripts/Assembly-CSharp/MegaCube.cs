using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class MegaCube : MonoBehaviour
{
	[HideInInspector]
	public List<Vector4> points = new List<Vector4>(512);

	[HideInInspector]
	public int xSize = 2;

	[HideInInspector]
	public int ySize = 2;

	[HideInInspector]
	public int zSize = 2;

	[HideInInspector]
	public float side = 2f;

	[HideInInspector]
	public float step = 1f;

	[HideInInspector]
	public bool[] excludeSides = new bool[6];

	public Bounds bounds;

	public float uvStep = 6f;

	public bool flipped;

	public bool runtimeEditing;

	public bool randomTile;

	public Material floorMat;

	public MegaCube[] connectedCubes = new MegaCube[6];

	private Vector3[] vertices = new Vector3[4];

	private Vector2[] uv = new Vector2[4];

	private Vector3 gizmo_offset = Vector3.one;

	private Vector4 temp;

	private Mesh finalMesh;

	private List<Mesh> meshes = new List<Mesh>(512);

	private List<Vector3> vertsBuffer;

	private List<int> trisBuffer;

	private List<Vector2> uvsBuffer;

	private Color[] colors = new Color[0];

	private List<int> cubesToCheck = new List<int>(6);

	private int bufferIndex;

	public Transform t { get; private set; }

	public MeshFilter meshFilter { get; private set; }

	public MeshCollider meshCollider { get; private set; }

	public MeshRenderer meshRenderer { get; private set; }

	private void Awake()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		meshRenderer = GetComponent<MeshRenderer>();
		if (runtimeEditing)
		{
			SetupBuffersAndFinalMesh();
		}
	}

	public void UpdateBounds()
	{
		bounds.size = new Vector3((float)xSize * side, (float)ySize * side, (float)zSize * side);
		bounds.center = base.transform.TransformPoint(bounds.size / 2f - Vector3.one * (side / 2f));
	}

	public bool PositionInBounds(Vector3 pos)
	{
		UpdateBounds();
		return bounds.Contains(pos);
	}

	public void SetupBuffersAndFinalMesh()
	{
		finalMesh = new Mesh();
		int num = xSize * ySize * zSize / 2 * 8;
		vertsBuffer = new List<Vector3>(num * 4);
		trisBuffer = new List<int>(num * 6);
		uvsBuffer = new List<Vector2>(num * 4);
	}

	public void UpdatePosition(Vector3 pos_offset)
	{
		if (!t)
		{
			t = base.transform;
		}
		t.localPosition += pos_offset;
	}

	public void SetSize(int x, int y, int z, float s)
	{
		xSize = x;
		ySize = y;
		zSize = z;
		side = s;
	}

	public void UpdatePoint(int i, int wValue)
	{
		points[i] = points[i].With(null, null, null, wValue);
	}

	public void InitialSetup(Vector4 size, Material mat)
	{
		t = base.transform;
		xSize = (int)size.x;
		ySize = (int)size.y;
		zSize = (int)size.z;
		side = size.w;
		uvStep = side;
		CreateArray(1);
		SetupBuffersAndFinalMesh();
		GetComponent<MeshRenderer>().material = (mat ? mat : (Resources.Load("MegaCube-default", typeof(Material)) as Material));
		CreateMesh();
	}

	public void ExpandMegacube(int faceToExclude, int i)
	{
		if (!connectedCubes[faceToExclude])
		{
			GameObject gameObject = new GameObject("MegaCube");
			gameObject.isStatic = t.gameObject.isStatic;
			MegaCube megaCube = gameObject.AddComponent<MegaCube>();
			connectedCubes[faceToExclude] = megaCube;
			float num = 1f;
			switch (faceToExclude)
			{
			case 0:
			case 1:
				megaCube.connectedCubes[(faceToExclude == 0) ? 1u : 0u] = this;
				num = (float)xSize * side;
				break;
			case 2:
			case 3:
				megaCube.connectedCubes[(faceToExclude == 2) ? 3 : 2] = this;
				num = (float)zSize * side;
				break;
			case 4:
			case 5:
				megaCube.connectedCubes[(faceToExclude == 4) ? 5 : 4] = this;
				num = (float)ySize * side;
				break;
			}
			gameObject.transform.position = t.transform.position + (Vector3)CubeMesh.faceDirections[i] * num;
			gameObject.transform.rotation = t.transform.rotation;
			megaCube.flipped = flipped;
			megaCube.runtimeEditing = runtimeEditing;
			megaCube.randomTile = randomTile;
			megaCube.InitialSetup(new Vector4(xSize, ySize, zSize, side), GetComponent<MeshRenderer>().sharedMaterial);
			megaCube.CreateMesh();
			CreateArray();
			CreateMesh();
		}
	}

	public void CreateArray(int resetValue = -1)
	{
		int num = xSize * ySize * zSize;
		if (resetValue > -1)
		{
			points.Clear();
		}
		for (int i = 0; i < num; i++)
		{
			temp.x = i - Mathf.FloorToInt(i / xSize) * xSize;
			temp.y = Mathf.FloorToInt(i / (xSize * zSize));
			temp.z = (float)Mathf.FloorToInt(i / xSize) - temp.y * (float)zSize;
			temp *= side;
			if (resetValue > -1)
			{
				temp.w = resetValue;
				points.Add(temp);
			}
			else
			{
				temp.w = points[i].w;
				points[i] = temp;
			}
		}
	}

	public int IndexOfClosestPointToPosition(Vector3 pos)
	{
		int result = 0;
		float num = float.PositiveInfinity;
		for (int i = 0; i < points.Count; i++)
		{
			float sqrMagnitude = ((Vector3)points[i] - pos).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = i;
			}
		}
		return result;
	}

	public void ClearArea(Vector3 aPos, Vector3 bPos, int w = 0)
	{
		int a = IndexOfClosestPointToPosition(base.transform.InverseTransformPoint(aPos));
		int b = IndexOfClosestPointToPosition(base.transform.InverseTransformPoint(bPos));
		ClearArea(a, b, w);
	}

	public void ClearArea(int a, int b, int w = 0)
	{
		Vector3Int vector3Int = default(Vector3Int);
		Vector3Int vector3Int2 = default(Vector3Int);
		vector3Int.x = (int)((points[b].x - points[a].x) / side);
		vector3Int.y = (int)((points[b].y - points[a].y) / side);
		vector3Int.z = (int)((points[b].z - points[a].z) / side);
		vector3Int2.x = vector3Int.x.Sign();
		vector3Int2.y = vector3Int.y.Sign();
		vector3Int2.z = vector3Int.z.Sign();
		_ = vector3Int.x;
		_ = vector3Int.z;
		_ = vector3Int.y;
		List<int> list = new List<int>();
		for (int i = 0; i < vector3Int.x.Abs() + 1; i++)
		{
			list.Add(a + i * vector3Int2.x);
		}
		int count = list.Count;
		for (int j = 0; j < count; j++)
		{
			for (int k = 1; k < vector3Int.z.Abs() + 1; k++)
			{
				list.Add(list[j] + xSize * k * vector3Int2.z);
			}
		}
		count = list.Count;
		for (int l = 0; l < vector3Int.y.Abs(); l++)
		{
			for (int m = 0; m < count; m++)
			{
				list.Add(list[m] + (l + 1) * (xSize * zSize) * vector3Int2.y);
			}
		}
		for (int n = 0; n < list.Count; n++)
		{
			if (points[list[n]].w != (float)w)
			{
				temp = points[list[n]];
				temp.w = w;
				points[list[n]] = temp;
			}
		}
		if (points[a].y == 0f || points[b].y == 0f)
		{
			cubesToCheck.Add(4);
		}
		if (points[a].y == side * (float)ySize - side || points[b].y == side * (float)ySize - side)
		{
			cubesToCheck.Add(5);
		}
		CreateMesh();
	}

	private void CreateMeshRuntime(int changedIndex = -1, bool extarnalCall = false)
	{
		CheckComponents();
		if (vertsBuffer == null)
		{
			SetupBuffersAndFinalMesh();
		}
		vertsBuffer.Clear();
		trisBuffer.Clear();
		uvsBuffer.Clear();
		bufferIndex = 0;
		for (int i = 0; i < points.Count; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				temp = points[i];
				if (temp.w == 2f)
				{
					continue;
				}
				if ((excludeSides[j] || (bool)connectedCubes[j]) && CheckExternal(i, j))
				{
					if (!connectedCubes[j])
					{
						continue;
					}
					if (temp.w == 1f)
					{
						if (connectedCubes[j].CheckOpposite(i, j))
						{
							UpdateBuffers(temp, j);
						}
						else if (changedIndex == i && !cubesToCheck.Contains(j))
						{
							cubesToCheck.Add(j);
						}
					}
					else if (changedIndex == i && !connectedCubes[j].CheckOpposite(i, j) && !cubesToCheck.Contains(j))
					{
						cubesToCheck.Add(j);
					}
				}
				else if (flipped)
				{
					if (CheckExternal(i, j))
					{
						if (temp.w == 0f)
						{
							UpdateBuffers(temp, j, flipped: true);
						}
					}
					else if (temp.w == 1f && !CheckNeighbor(i, j))
					{
						UpdateBuffers(temp, j);
					}
				}
				else if (temp.w == 1f && !CheckNeighbor(i, j))
				{
					UpdateBuffers(temp, j);
				}
			}
		}
		if (cubesToCheck.Count != 0 && !extarnalCall)
		{
			foreach (int item in cubesToCheck)
			{
				if ((bool)connectedCubes[item])
				{
					connectedCubes[item].CreateArray();
					connectedCubes[item].CreateMesh(-1, extarnalCall: true);
				}
			}
		}
		if (colors.Length != vertsBuffer.Count)
		{
			colors = new Color[vertsBuffer.Count];
		}
		Color color = new Color(1f, 1f, 1f, 1f);
		for (int k = 0; k < colors.Length; k++)
		{
			temp = base.transform.TransformPoint(vertsBuffer[k]);
			color.r = Mathf.PerlinNoise((0f - temp.x + temp.y) / 12f, (temp.x + temp.z) / 12f);
			color.b = Mathf.PerlinNoise((0f - temp.x + temp.y) / 32f, (temp.x + temp.z) / 32f);
			color.g = 0f;
			color.a = 1f;
			colors[k] = color;
		}
		finalMesh.Clear();
		finalMesh.SetVertices(vertsBuffer);
		finalMesh.SetTriangles(trisBuffer, 0);
		finalMesh.SetUVs(0, uvsBuffer);
		finalMesh.SetColors(colors);
		finalMesh.RecalculateNormals();
		finalMesh.Optimize();
		meshFilter.sharedMesh = finalMesh;
		meshCollider.sharedMesh = finalMesh;
		cubesToCheck.Clear();
	}

	public void CreateMesh(int changedIndex = -1, bool extarnalCall = false)
	{
		CheckComponents();
		if (runtimeEditing)
		{
			CreateMeshRuntime(changedIndex, extarnalCall);
			return;
		}
		if (vertsBuffer == null)
		{
			SetupBuffersAndFinalMesh();
		}
		meshes.Clear();
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			Object.DestroyImmediate(componentsInChildren[i].gameObject);
		}
		for (int j = 0; j < 6; j++)
		{
			for (int k = 0; k < points.Count; k++)
			{
				temp = points[k];
				if (temp.w == 2f)
				{
					continue;
				}
				if ((excludeSides[j] || (bool)connectedCubes[j]) && CheckExternal(k, j))
				{
					if (!connectedCubes[j])
					{
						continue;
					}
					if (temp.w == 1f)
					{
						if (connectedCubes[j].CheckOpposite(k, j))
						{
							meshes.Add(CreateFace(temp, j));
						}
						else if (changedIndex == k && !cubesToCheck.Contains(j))
						{
							cubesToCheck.Add(j);
						}
					}
					else if (changedIndex == k && !connectedCubes[j].CheckOpposite(k, j) && !cubesToCheck.Contains(j))
					{
						cubesToCheck.Add(j);
					}
				}
				else if (flipped)
				{
					if (CheckExternal(k, j))
					{
						if (temp.w == 0f)
						{
							meshes.Add(CreateFace(temp, j, flipped: true));
						}
					}
					else if (temp.w == 1f && !CheckNeighbor(k, j))
					{
						meshes.Add(CreateFace(temp, j));
					}
				}
				else if (temp.w == 1f && !CheckNeighbor(k, j))
				{
					meshes.Add(CreateFace(temp, j));
				}
			}
		}
		if (cubesToCheck.Count != 0 && !extarnalCall)
		{
			foreach (int item in cubesToCheck)
			{
				if ((bool)connectedCubes[item])
				{
					connectedCubes[item].CreateArray();
					connectedCubes[item].CreateMesh(-1, extarnalCall: true);
				}
			}
		}
		finalMesh = new Mesh();
		CombineInstance[] array = new CombineInstance[meshes.Count];
		for (int l = 0; l < array.Length; l++)
		{
			array[l].mesh = meshes[l];
		}
		finalMesh.CombineMeshes(array, mergeSubMeshes: true, useMatrices: false);
		finalMesh.RecalculateNormals();
		finalMesh.Optimize();
		meshFilter.sharedMesh = finalMesh;
		if (meshCollider.enabled)
		{
			meshRenderer.enabled = false;
		}
		meshCollider.sharedMesh = finalMesh;
		cubesToCheck.Clear();
	}

	public void CreateMesh2(int changedIndex = -1, bool extarnalCall = false)
	{
		CheckComponents();
		if (runtimeEditing)
		{
			CreateMeshRuntime(changedIndex, extarnalCall);
			return;
		}
		if (vertsBuffer == null)
		{
			SetupBuffersAndFinalMesh();
		}
		meshes.Clear();
		for (int i = 0; i < points.Count; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				temp = points[i];
				if (temp.w == 2f)
				{
					continue;
				}
				if ((excludeSides[j] || (bool)connectedCubes[j]) && CheckExternal(i, j))
				{
					if (!connectedCubes[j])
					{
						continue;
					}
					if (temp.w == 1f)
					{
						if (connectedCubes[j].CheckOpposite(i, j))
						{
							meshes.Add(CreateFace(temp, j));
						}
						else if (changedIndex == i && !cubesToCheck.Contains(j))
						{
							cubesToCheck.Add(j);
						}
					}
					else if (changedIndex == i && !connectedCubes[j].CheckOpposite(i, j) && !cubesToCheck.Contains(j))
					{
						cubesToCheck.Add(j);
					}
				}
				else if (flipped)
				{
					if (CheckExternal(i, j))
					{
						if (temp.w == 0f)
						{
							meshes.Add(CreateFace(temp, j, flipped: true));
						}
					}
					else if (temp.w == 1f && !CheckNeighbor(i, j))
					{
						meshes.Add(CreateFace(temp, j));
					}
				}
				else if (temp.w == 1f && !CheckNeighbor(i, j))
				{
					meshes.Add(CreateFace(temp, j));
				}
			}
		}
		if (cubesToCheck.Count != 0 && !extarnalCall)
		{
			foreach (int item in cubesToCheck)
			{
				if ((bool)connectedCubes[item])
				{
					connectedCubes[item].CreateArray();
					connectedCubes[item].CreateMesh(-1, extarnalCall: true);
				}
			}
		}
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int k = 1; k < componentsInChildren.Length; k++)
		{
			Object.DestroyImmediate(componentsInChildren[k].gameObject);
		}
		for (int l = 0; l < meshes.Count; l++)
		{
			GameObject obj = new GameObject(l.ToString());
			obj.transform.SetParent(t, worldPositionStays: false);
			meshes[l].RecalculateNormals();
			obj.AddComponent<MeshFilter>().mesh = meshes[l];
			obj.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().sharedMaterial;
			obj.isStatic = true;
		}
		finalMesh = new Mesh();
		CombineInstance[] array = new CombineInstance[meshes.Count];
		for (int m = 0; m < array.Length; m++)
		{
			array[m].mesh = meshes[m];
		}
		finalMesh.CombineMeshes(array, mergeSubMeshes: true, useMatrices: false);
		finalMesh.RecalculateNormals();
		finalMesh.Optimize();
		meshFilter.sharedMesh = finalMesh;
		if (meshCollider.enabled)
		{
			meshRenderer.enabled = false;
		}
		meshCollider.sharedMesh = finalMesh;
		cubesToCheck.Clear();
	}

	private void CheckComponents()
	{
		if (!t)
		{
			t = base.transform;
		}
		if (!meshFilter)
		{
			meshFilter = GetComponent<MeshFilter>();
		}
		if (!meshRenderer)
		{
			meshRenderer = GetComponent<MeshRenderer>();
		}
		if (!meshCollider)
		{
			meshCollider = GetComponent<MeshCollider>();
		}
	}

	private void GenerateLightmapUVset()
	{
	}

	public void RefreshConnectedCubes()
	{
		for (int i = 0; i < connectedCubes.Length; i++)
		{
			if ((bool)connectedCubes[i])
			{
				connectedCubes[i].CreateArray();
				connectedCubes[i].CreateMesh(-1, extarnalCall: true);
			}
		}
	}

	public void ResetDependences()
	{
		for (int i = 0; i < connectedCubes.Length; i++)
		{
			if ((bool)connectedCubes[i])
			{
				connectedCubes[i] = null;
			}
		}
	}

	private bool CheckOpposite(int i, int dir)
	{
		int num = i;
		switch (dir)
		{
		case 0:
			num += (xSize - 1) * zSize;
			break;
		case 1:
			num -= (xSize - 1) * zSize;
			break;
		case 2:
			num += xSize - 1;
			break;
		case 3:
			num -= xSize - 1;
			break;
		case 4:
			num += xSize * zSize * (ySize - 1);
			break;
		case 5:
			num -= xSize * zSize * (ySize - 1);
			break;
		}
		return points[num].w != 1f;
	}

	private bool CheckExternal(int i, int dir)
	{
		switch (dir)
		{
		case 0:
			if (points[i].z != 0f)
			{
				return false;
			}
			return true;
		case 1:
			if (points[i].z != (float)(zSize - 1) * side)
			{
				return false;
			}
			return true;
		case 2:
			if (points[i].x != 0f)
			{
				return false;
			}
			return true;
		case 3:
			if (points[i].x != (float)(xSize - 1) * side)
			{
				return false;
			}
			return true;
		case 4:
			if (points[i].y != 0f)
			{
				return false;
			}
			return true;
		case 5:
			if (points[i].y != (float)(ySize - 1) * side)
			{
				return false;
			}
			return true;
		default:
			return false;
		}
	}

	public bool CheckNeighbor(int i, int dir)
	{
		int num = i;
		num = dir switch
		{
			0 => num - xSize, 
			1 => num + xSize, 
			2 => num - 1, 
			3 => num + 1, 
			4 => num - xSize * zSize, 
			_ => num + xSize * zSize, 
		};
		if (num < 0 || num >= points.Count)
		{
			return false;
		}
		if (points[num].w == 0f)
		{
			return false;
		}
		if (dir < 4 && points[num].y != points[i].y)
		{
			return false;
		}
		if (dir > 1 && points[num].z != points[i].z)
		{
			return false;
		}
		return true;
	}

	private void UpdateBuffers(Vector3 point, int dir, bool flipped = false)
	{
		for (int i = 0; i < 4; i++)
		{
			temp = CubeMesh.vertices[CubeMesh.indices[dir][i]] * (side / 2f) + point;
			Vector2 item = default(Vector2);
			item.x = CubeMesh.uv[i].x;
			item.y = CubeMesh.uv[i].y;
			uvsBuffer.Add(item);
			vertsBuffer.Add(temp);
		}
		for (int j = 0; j < 6; j++)
		{
			trisBuffer.Add(bufferIndex * 4 + (flipped ? CubeMesh.faceFlippedTriangles[j] : CubeMesh.faceTriangles[j]));
		}
		bufferIndex++;
	}

	private Mesh CreateFace(Vector3 point, int dir, bool flipped = false)
	{
		Mesh mesh = new Mesh();
		for (int i = 0; i < 4; i++)
		{
			vertices[i] = CubeMesh.vertices[CubeMesh.indices[dir][i]] * (side / 2f) + point;
		}
		mesh.vertices = vertices;
		mesh.SetTriangles(flipped ? CubeMesh.faceFlippedTriangles : CubeMesh.faceTriangles, 0);
		if (!randomTile)
		{
			for (int j = 0; j < 4; j++)
			{
				uv[j].x = CubeMesh.uv[j].x;
				uv[j].y = CubeMesh.uv[j].y;
			}
		}
		else
		{
			int num = Random.Range(0, 4);
			for (int k = 0; k < 4; k++)
			{
				uv[k].x = CubeMesh.uv[k].x / 2f + CubeMesh.uv[num].x / 2f;
				uv[k].y = CubeMesh.uv[k].y / 2f + CubeMesh.uv[num].y / 2f;
			}
		}
		mesh.uv = uv;
		mesh.uv2 = uv;
		mesh.RecalculateNormals();
		GameObject gameObject = new GameObject(point.ToString());
		gameObject.transform.SetParent(t, worldPositionStays: false);
		gameObject.AddComponent<MeshFilter>().mesh = mesh;
		if (dir == 5 && (bool)floorMat)
		{
			gameObject.AddComponent<MeshRenderer>().material = floorMat;
		}
		else
		{
			gameObject.AddComponent<MeshRenderer>().material = meshRenderer.sharedMaterial;
		}
		gameObject.isStatic = true;
		return mesh;
	}

	private void OnDrawGizmosSelected()
	{
		if (!t)
		{
			t = base.transform;
		}
		Gizmos.color = Color.green;
		for (int i = 0; i < 6; i++)
		{
			if ((bool)connectedCubes[i])
			{
				if (!connectedCubes[i].t)
				{
					connectedCubes[i].t = connectedCubes[i].transform;
				}
				Gizmos.DrawLine(t.position, connectedCubes[i].t.position);
			}
		}
		Vector3 vector = new Vector3(xSize, ySize, zSize);
		Vector3 vector2 = vector * (side / 2f) - gizmo_offset * (side / 2f);
		Gizmos.matrix = t.localToWorldMatrix;
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(vector2, vector * side);
		for (int j = 0; j < 6; j++)
		{
			if (excludeSides[j])
			{
				switch (j)
				{
				case 0:
				case 1:
					Gizmos.DrawRay(vector2 + (Vector3)CubeMesh.faceDirections[j] * ((float)zSize * side / 2f), CubeMesh.faceDirections[j]);
					break;
				case 2:
				case 3:
					Gizmos.DrawRay(vector2 + (Vector3)CubeMesh.faceDirections[j] * ((float)xSize * side / 2f), CubeMesh.faceDirections[j]);
					break;
				case 4:
				case 5:
					Gizmos.DrawRay(vector2 + (Vector3)CubeMesh.faceDirections[j] * ((float)ySize * side / 2f), CubeMesh.faceDirections[j]);
					break;
				}
			}
		}
	}
}
