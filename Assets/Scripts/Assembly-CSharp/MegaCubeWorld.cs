using System;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class MegaCubeWorld : MonoBehaviour
{
	public Action OnChange = delegate
	{
	};

	private static readonly List<Vector3Int> PreallocationList = new List<Vector3Int>(512);

	private Transform t;

	private Vector3 tempVector;

	private Vector3Int temp;

	private Vector3[] vertices = new Vector3[4];

	private Vector2[] uv = new Vector2[4];

	private Vector3[] faceVertices = new Vector3[8];

	private Mesh tempMesh;

	private Color color;

	private List<Vector3> vertsBuffer = new List<Vector3>();

	private List<int> trisBuffer = new List<int>();

	private List<Vector2> uvsBuffer = new List<Vector2>();

	private List<Color> colorsBuffer = new List<Color>();

	private int bufferIndex;

	public bool runtimeEditing;

	public int side = 6;

	public Material mat;

	[HideInInspector]
	public Vector3Int origin;

	public List<Vector3Int> pointsToExclude = new List<Vector3Int>();

	public List<MegaCubeRegion> regions = new List<MegaCubeRegion>(27);

	[HideInInspector]
	public List<Vector3Int> areaPoints = new List<Vector3Int>(512);

	[HideInInspector]
	public List<int> regionsToCheck = new List<int>(64);

	[HideInInspector]
	public Vector3Int posA;

	[HideInInspector]
	public Vector3Int posB;

	private Vector3[] verts = new Vector3[4];

	private int[] tris = new int[6];

	private void Awake()
	{
		if (runtimeEditing)
		{
			t = base.transform;
			SetupBuffers();
		}
	}

	public void SetupBuffers()
	{
		if (vertsBuffer.Capacity != 1536)
		{
			vertsBuffer = new List<Vector3>(1536);
		}
		if (uvsBuffer.Capacity != 1536)
		{
			uvsBuffer = new List<Vector2>(1536);
		}
		if (colorsBuffer.Capacity != 1536)
		{
			colorsBuffer = new List<Color>(1536);
		}
		if (trisBuffer.Capacity != 2304)
		{
			trisBuffer = new List<int>(2304);
		}
		tempMesh = new Mesh();
		for (int i = 0; i < faceVertices.Length; i++)
		{
			faceVertices[i] = CubeMesh.vertices[i] * side / 2f;
		}
	}

	public void ClearExlusions()
	{
		pointsToExclude.Clear();
		Rebuild();
	}

	public void Rebuild()
	{
		foreach (MegaCubeRegion region in regions)
		{
			Transform[] componentsInChildren = region.tRoot.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Vector3Int point in region.points)
			{
				for (int i = 1; i < componentsInChildren.Length; i++)
				{
					if ((bool)componentsInChildren[i] && componentsInChildren[i].position == point)
					{
						UnityEngine.Object.DestroyImmediate(componentsInChildren[i].gameObject);
					}
				}
				int num = RegionContains(point);
				if (num == -1 || pointsToExclude.Contains(point))
				{
					continue;
				}
				for (int j = 0; j < 6; j++)
				{
					if (!CheckNeighbour(point, j, num))
					{
						CreateFace(point, j, num);
					}
				}
			}
		}
		GenerateColliderForEachRegion();
	}

	public void RebuildSingle(Vector3Int point, int sign)
	{
		if (sign == 1)
		{
			AddToRegion(point);
		}
		else
		{
			RemoveFromRegion(point);
		}
		areaPoints.Clear();
		areaPoints.Add(point);
		for (int i = 1; i < 7; i++)
		{
			areaPoints.Add(point + CubeMesh.faceDirections[i - 1] * side);
		}
		RebuildArea();
		GenerateColliderForEachRegion();
	}

	private void RebuildArea()
	{
		foreach (MegaCubeRegion region in regions)
		{
			Transform[] componentsInChildren = region.tRoot.GetComponentsInChildren<Transform>(includeInactive: true);
			for (int i = 0; i < areaPoints.Count; i++)
			{
				for (int j = 1; j < componentsInChildren.Length; j++)
				{
					if ((bool)componentsInChildren[j] && componentsInChildren[j].position == areaPoints[i])
					{
						UnityEngine.Object.DestroyImmediate(componentsInChildren[j].gameObject);
					}
				}
			}
		}
		for (int k = 0; k < areaPoints.Count; k++)
		{
			int num = RegionContains(areaPoints[k]);
			if (num == -1 || pointsToExclude.Contains(areaPoints[k]))
			{
				continue;
			}
			for (int l = 0; l < 6; l++)
			{
				if (!CheckNeighbour(areaPoints[k], l, num))
				{
					CreateFace(areaPoints[k], l, num);
				}
			}
		}
		areaPoints.Clear();
	}

	public void GenerateColliderForEachRegion()
	{
		for (int i = 0; i < regions.Count; i++)
		{
			vertsBuffer.Clear();
			trisBuffer.Clear();
			int num = 0;
			MeshFilter[] componentsInChildren = regions[i].tRoot.GetComponentsInChildren<MeshFilter>(includeInactive: true);
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				if (!meshFilter.CompareTag("Dirt"))
				{
					verts = meshFilter.sharedMesh.vertices;
					tempVector = meshFilter.transform.position;
					for (int k = 0; k < 4; k++)
					{
						verts[k] += tempVector;
					}
					tris = meshFilter.sharedMesh.triangles;
					for (int l = 0; l < 6; l++)
					{
						tris[l] += num * 4;
					}
					vertsBuffer.AddRange(verts);
					trisBuffer.AddRange(tris);
					num++;
				}
			}
			tempMesh = new Mesh();
			tempMesh.SetVertices(vertsBuffer);
			tempMesh.SetTriangles(trisBuffer, 0);
			tempMesh.RecalculateNormals();
			tempMesh.RecalculateTangents();
			regions[i].clldr.sharedMesh = tempMesh;
		}
		if (OnChange != null)
		{
			OnChange();
		}
	}

	public void CreateMeshRuntime()
	{
		if (!t)
		{
			SetupBuffers();
		}
		int num = 0;
		if (regionsToCheck.Count == 0)
		{
			foreach (MegaCubeRegion region in regions)
			{
				UpdateRegionMesh(region, num);
				num++;
			}
			return;
		}
		for (int i = 0; i < regionsToCheck.Count; i++)
		{
			num = regionsToCheck[i];
			UpdateRegionMesh(regions[num], num);
		}
		regionsToCheck.Clear();
	}

	public void SimplifyCollider()
	{
		foreach (MegaCubeRegion region in regions)
		{
			Mesh sharedMesh = region.clldr.sharedMesh;
			sharedMesh.Weld(0.1f, 0.1f);
			sharedMesh.Simplify();
			region.clldr.sharedMesh = sharedMesh;
		}
	}

	private void UpdateRegionMesh(MegaCubeRegion r, int index)
	{
		vertsBuffer.Clear();
		uvsBuffer.Clear();
		trisBuffer.Clear();
		colorsBuffer.Clear();
		bufferIndex = 0;
		foreach (Vector3Int point in r.points)
		{
			for (int i = 0; i < 6; i++)
			{
				if (!CheckNeighbour(point, i, index))
				{
					AddFaceToBuffers(point, i);
				}
			}
		}
		if (!r.mesh)
		{
			r.mesh = new Mesh();
		}
		else
		{
			r.mesh.Clear();
		}
		r.mesh.SetVertices(vertsBuffer);
		r.mesh.SetUVs(0, uvsBuffer);
		r.mesh.SetTriangles(trisBuffer, 0);
		r.mesh.SetColors(colorsBuffer);
		r.mesh.RecalculateNormals();
		r.filter.sharedMesh = r.mesh;
		r.clldr.sharedMesh = r.mesh;
		r.rend.sharedMaterial = mat;
	}

	private void AddFaceToBuffers(Vector3 point, int dir, bool flipped = false)
	{
		for (int i = 0; i < 6; i++)
		{
			if (i < 4)
			{
				tempVector = point + faceVertices[CubeMesh.indices[dir][i]];
				vertsBuffer.Add(tempVector);
				color.r = Mathf.PerlinNoise((0f - tempVector.x + tempVector.y) / 8f, (tempVector.x + tempVector.z) / 8f);
				color.b = Mathf.PerlinNoise((0f - tempVector.x + tempVector.y) / 16f, (tempVector.x + tempVector.z) / 16f);
				color.g = 0f;
				color.a = 1f;
				colorsBuffer.Add(color);
			}
			trisBuffer.Add(bufferIndex * 4 + (flipped ? CubeMesh.faceFlippedTriangles[i] : CubeMesh.faceTriangles[i]));
		}
		uvsBuffer.AddRange(CubeMesh.uv);
		bufferIndex++;
	}

	private Mesh CreateFace(Vector3 point, int dir, int region = -1, bool flipped = false)
	{
		Mesh mesh = new Mesh();
		for (int i = 0; i < 4; i++)
		{
			vertices[i] = CubeMesh.vertices[CubeMesh.indices[dir][i]] * ((float)side / 2f);
		}
		mesh.vertices = vertices;
		mesh.SetTriangles(flipped ? CubeMesh.faceFlippedTriangles : CubeMesh.faceTriangles, 0);
		Vector2 vector = default(Vector2);
		vector.x = ((Mathf.PerlinNoise((point.x + point.z) / 10f, (point.y - point.x) / 20f) > 0.5f) ? 0f : 0.5f);
		vector.y = ((Mathf.PerlinNoise((point.y - point.z) / 10f, (point.z + point.x) / 20f) > 0.5f) ? 0f : 0.5f);
		Vector2[] array = new Vector2[4];
		for (int j = 0; j < 4; j++)
		{
			array[j] = CubeMesh.uv[j] / 2f + vector;
		}
		mesh.uv = array;
		mesh.SetUVs(0, array);
		mesh.SetUVs(1, CubeMesh.uv);
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		GameObject gameObject = new GameObject(dir.ToString());
		gameObject.transform.SetParent((region == -1) ? t : regions[region].tRoot, worldPositionStays: false);
		gameObject.transform.position = point;
		gameObject.AddComponent<MeshFilter>().mesh = mesh;
		gameObject.AddComponent<MeshRenderer>().material = mat;
		if (!runtimeEditing)
		{
			gameObject.isStatic = true;
		}
		return mesh;
	}

	public void TranslateWorldPosition(Vector3 wordlPos, out Vector3Int v)
	{
		v = default(Vector3Int);
		v.x = Mathf.RoundToInt(wordlPos.x / (float)side) * side;
		v.y = Mathf.RoundToInt(wordlPos.y / (float)side) * side;
		v.z = Mathf.RoundToInt(wordlPos.z / (float)side) * side;
	}

	public void ClearAll()
	{
		for (int i = 0; i < regions.Count; i++)
		{
			UnityEngine.Object.DestroyImmediate(regions[i].root);
		}
		regions.Clear();
	}

	public void Setup()
	{
		t = base.transform;
		ClearAll();
		FillRegionAtPoint(new Vector3Int(0, 0, 0));
		if (!mat)
		{
			mat = Resources.Load("Materials/Quickmap", typeof(Material)) as Material;
		}
		if (!runtimeEditing)
		{
			Rebuild();
			return;
		}
		SetupBuffers();
		CreateMeshRuntime();
	}

	public void Clear()
	{
		ClearAll();
	}

	public void AddSingle(Vector3Int pos, bool add = false)
	{
	}

	public void AddArea(Vector3Int posA, Vector3Int posB, bool add = false)
	{
		areaPoints.Clear();
		regionsToCheck.Clear();
		if (posA != posB)
		{
			int i = posB.x - posA.x;
			int i2 = posB.y - posA.y;
			int i3 = posB.z - posA.z;
			int num = 1 + i.Abs() / side;
			int num2 = 1 + i2.Abs() / side;
			int num3 = 1 + i3.Abs() / side;
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num3; k++)
				{
					for (int l = 0; l < num2; l++)
					{
						temp.x = posA.x + j * side * i.Sign();
						temp.y = posA.y + l * side * i2.Sign();
						temp.z = posA.z + k * side * i3.Sign();
						areaPoints.Add(temp);
						if (add)
						{
							AddToRegion(temp);
						}
						else
						{
							RemoveFromRegion(temp);
						}
						if (j == 0)
						{
							areaPoints.Add(temp - CubeMesh.right * (side * i.Sign()));
						}
						if (j == num - 1)
						{
							areaPoints.Add(temp + CubeMesh.right * (side * i.Sign()));
						}
						if (l == 0)
						{
							areaPoints.Add(temp - CubeMesh.up * (side * i2.Sign()));
						}
						if (l == num2 - 1)
						{
							areaPoints.Add(temp + CubeMesh.up * (side * i2.Sign()));
						}
						if (k == 0)
						{
							areaPoints.Add(temp - CubeMesh.forward * (side * i3.Sign()));
						}
						if (k == num3 - 1)
						{
							areaPoints.Add(temp + CubeMesh.forward * (side * i3.Sign()));
						}
					}
				}
			}
		}
		else
		{
			areaPoints.Add(posA);
			if (add)
			{
				AddToRegion(posA);
			}
			else
			{
				RemoveFromRegion(posA);
			}
			for (int m = 1; m < 7; m++)
			{
				areaPoints.Add(posA + CubeMesh.faceDirections[m - 1] * side);
			}
		}
		if (runtimeEditing)
		{
			CreateMeshRuntime();
			return;
		}
		RebuildArea();
		GenerateColliderForEachRegion();
	}

	public void ExcludeArea(Vector3Int posA, Vector3Int posB)
	{
		areaPoints.Clear();
		regionsToCheck.Clear();
		if (posA != posB)
		{
			int i = posB.x - posA.x;
			int i2 = posB.y - posA.y;
			int i3 = posB.z - posA.z;
			int num = 1 + i.Abs() / side;
			int num2 = 1 + i2.Abs() / side;
			int num3 = 1 + i3.Abs() / side;
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num3; k++)
				{
					for (int l = 0; l < num2; l++)
					{
						temp.x = posA.x + j * side * i.Sign();
						temp.y = posA.y + l * side * i2.Sign();
						temp.z = posA.z + k * side * i3.Sign();
						areaPoints.Add(temp);
						ExcludePoint(temp);
						if (j == 0)
						{
							areaPoints.Add(temp - CubeMesh.right * (side * i.Sign()));
						}
						if (j == num - 1)
						{
							areaPoints.Add(temp + CubeMesh.right * (side * i.Sign()));
						}
						if (l == 0)
						{
							areaPoints.Add(temp - CubeMesh.up * (side * i2.Sign()));
						}
						if (l == num2 - 1)
						{
							areaPoints.Add(temp + CubeMesh.up * (side * i2.Sign()));
						}
						if (k == 0)
						{
							areaPoints.Add(temp - CubeMesh.forward * (side * i3.Sign()));
						}
						if (k == num3 - 1)
						{
							areaPoints.Add(temp + CubeMesh.forward * (side * i3.Sign()));
						}
					}
				}
			}
		}
		else
		{
			areaPoints.Add(posA);
			ExcludePoint(posA);
			for (int m = 1; m < 7; m++)
			{
				areaPoints.Add(posA + CubeMesh.faceDirections[m - 1] * side);
			}
		}
		if (runtimeEditing)
		{
			CreateMeshRuntime();
			return;
		}
		RebuildArea();
		GenerateColliderForEachRegion();
	}

	public bool CheckNeighbour(Vector3Int pos, int dir, int region = -1)
	{
		temp = pos + CubeMesh.faceDirections[dir] * side;
		return RegionContains(temp) != -1;
	}

	public bool CheckCertainNeighbour(Vector3Int pos, int region = -1)
	{
		return RegionContains(pos) != -1;
	}

	public void AddToRegion(Vector3Int v)
	{
		UpdateOrigin(v);
		int num = FindRegion();
		if (num == -1)
		{
			num = AddNewRegion();
		}
		if (!regions[num].points.Contains(v))
		{
			regions[num].points.Add(v);
		}
	}

	public void ExcludePoint(Vector3Int v)
	{
		if (!pointsToExclude.Contains(v))
		{
			pointsToExclude.Add(v);
		}
	}

	public void RemoveFromRegion(Vector3Int v)
	{
		UpdateOrigin(v);
		int num = FindRegion();
		if (num != -1 && regions[num].points.Contains(v))
		{
			regions[num].points.Remove(v);
			if (regions[num].points.Count == 0)
			{
				RemoveRegion(num);
			}
		}
	}

	public void RemoveRegionAtPoint(Vector3Int v)
	{
		UpdateOrigin(v);
		int i = FindRegion();
		RemoveRegion(i);
	}

	public void FillRegionAtPoint(Vector3Int v)
	{
		UpdateOrigin(v);
		int num = FindRegion();
		if (num == -1)
		{
			num = AddNewRegion();
		}
		if (!runtimeEditing)
		{
			Transform[] componentsInChildren = regions[num].tRoot.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Vector3Int point in regions[num].points)
			{
				for (int i = 1; i < componentsInChildren.Length; i++)
				{
					if ((bool)componentsInChildren[i] && componentsInChildren[i].position == point)
					{
						UnityEngine.Object.DestroyImmediate(componentsInChildren[i].gameObject);
					}
				}
			}
		}
		regions[num].points.Clear();
		ref Vector3Int reference = ref temp;
		ref Vector3Int reference2 = ref temp;
		int num3 = (temp.z = 0);
		int x = (reference2.y = num3);
		reference.x = x;
		for (int j = 0; j < 8; j++)
		{
			for (int k = 0; k < 8; k++)
			{
				for (int l = 0; l < 8; l++)
				{
					temp.x = origin.x * (side * 8) + j * side;
					temp.y = origin.y * (side * 8) + l * side;
					temp.z = origin.z * (side * 8) + k * side;
					regions[num].points.Add(temp);
				}
			}
		}
	}

	public void UpdateOrigin(Vector3Int v)
	{
		origin.x = Mathf.FloorToInt((float)v.x / ((float)side * 8f));
		origin.y = Mathf.FloorToInt((float)v.y / ((float)side * 8f));
		origin.z = Mathf.FloorToInt((float)v.z / ((float)side * 8f));
	}

	public void Snap(ref Vector3 v)
	{
		v.x = Mathf.RoundToInt(v.x / (float)side) * side;
		v.y = Mathf.RoundToInt(v.y / (float)side) * side;
		v.z = Mathf.RoundToInt(v.z / (float)side) * side;
	}

	public int RegionContains(Vector3Int v)
	{
		UpdateOrigin(v);
		int num = FindRegion();
		if (num == -1)
		{
			return -1;
		}
		if (!regions[num].points.Contains(v))
		{
			return -1;
		}
		return num;
	}

	public int FindRegion()
	{
		for (int i = 0; i < regions.Count; i++)
		{
			if (regions[i].origin == origin)
			{
				return i;
			}
		}
		return -1;
	}

	private void RemoveRegion(int i)
	{
		UnityEngine.Object.DestroyImmediate(regions[i].root);
		regions.RemoveAt(i);
	}

	public int AddNewRegion()
	{
		MegaCubeRegion megaCubeRegion = new MegaCubeRegion();
		megaCubeRegion.points = new HashSet<Vector3Int>(PreallocationList);
		megaCubeRegion.points.Clear();
		megaCubeRegion.origin = origin;
		megaCubeRegion.root = new GameObject();
		if (runtimeEditing)
		{
			megaCubeRegion.root.tag = t.tag;
			megaCubeRegion.filter = megaCubeRegion.root.AddComponent<MeshFilter>();
			megaCubeRegion.rend = megaCubeRegion.root.AddComponent<MeshRenderer>();
		}
		megaCubeRegion.clldr = megaCubeRegion.root.AddComponent<MeshCollider>();
		megaCubeRegion.tRoot = megaCubeRegion.root.transform;
		megaCubeRegion.tRoot.SetParent(base.transform);
		megaCubeRegion.center = megaCubeRegion.origin * side * 8 + Vector3.one * (side * 4 - side / 2);
		megaCubeRegion.size = Vector3.one * (side * 8);
		megaCubeRegion.mesh = new Mesh();
		regions.Add(megaCubeRegion);
		return regions.Count - 1;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.black / 2f;
		foreach (MegaCubeRegion region in regions)
		{
			Gizmos.DrawWireCube(region.center, region.size);
			Gizmos.DrawWireCube(region.clldr.bounds.center, region.clldr.bounds.size);
		}
		Gizmos.color = Color.red;
		foreach (Vector3Int item in pointsToExclude)
		{
			Gizmos.DrawWireCube(item, Vector3.one * side);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
		if (posA.sqrMagnitude > 0)
		{
			if (posB.sqrMagnitude == 0)
			{
				Gizmos.DrawCube(posA, Vector3.one * side);
				return;
			}
			Vector3 vector = posB - posA;
			Gizmos.DrawCube((posA + posB) / 2, new Vector3(vector.x.Abs() + (float)side + 0.01f, vector.y.Abs() + (float)side + 0.01f, vector.z.Abs() + (float)side + 0.01f));
		}
	}
}
