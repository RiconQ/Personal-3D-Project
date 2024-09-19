using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class MegaArk : MeshCreator
{
	[Range(1f, 12f)]
	public int count = 4;

	[Range(1f, 4f)]
	public int corners = 2;

	public float depth = 6f;

	public float radius = 2f;

	private List<Vector3> points = new List<Vector3>();

	private Vector3[] cornerPoints;

	private List<Mesh> meshes = new List<Mesh>();

	public MeshFilter meshFilter { get; private set; }

	public MeshCollider meshCollider { get; private set; }

	public override void CreateMesh()
	{
		base.CreateMesh();
		points.Clear();
		cornerPoints = new Vector3[4];
		cornerPoints[0] = new Vector3(radius, radius, 0f);
		cornerPoints[1] = new Vector3(0f - radius, radius, 0f);
		cornerPoints[2] = new Vector3(0f - radius, 0f - radius, 0f);
		cornerPoints[3] = new Vector3(radius, 0f - radius, 0f);
		Vector3 vector = Vector3.right;
		for (int i = 0; i < corners * count + 1; i++)
		{
			points.Add(vector * radius);
			vector = Quaternion.Euler(0f, 0f, 90f / (float)count) * vector;
		}
		for (int j = 0; j < corners * count + 1; j++)
		{
			vector = points[j] - Vector3.forward * depth;
			points.Add(vector);
		}
		meshes.Clear();
		meshes.Add(CreateArkFace());
		for (int k = 0; k < corners * count; k++)
		{
			meshes.Add(CreateCapMesh(k, Mathf.FloorToInt((float)k / (float)count), forward: true));
			meshes.Add(CreateCapMesh(k + corners * count + 1, Mathf.FloorToInt((float)k / (float)count), forward: false));
		}
		if (!meshFilter)
		{
			meshFilter = GetComponent<MeshFilter>();
		}
		Mesh mesh = CombineMeshes(meshes.ToArray());
		mesh.name = "MegaArk-mesh";
		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;
		meshCollider = GetComponent<MeshCollider>();
		if (!meshCollider)
		{
			meshCollider = base.gameObject.AddComponent<MeshCollider>();
		}
		meshCollider.sharedMesh = mesh;
	}

	private Mesh CreateArkFace()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = points.ToArray();
		List<int> list = new List<int>();
		for (int i = 0; i < corners * count; i++)
		{
			int item = i;
			int num = i.Next(points.Count / 2);
			int item2 = i + points.Count / 2;
			int item3 = num + points.Count / 2;
			list.Add(item);
			list.Add(num);
			list.Add(item2);
			list.Add(item2);
			list.Add(num);
			list.Add(item3);
		}
		mesh.triangles = list.ToArray();
		Vector2[] array = new Vector2[points.Count];
		for (int j = 0; j < points.Count; j++)
		{
			array[j] = new Vector2(points[j].x / radius, points[j].z / depth);
		}
		mesh.uv = array;
		return mesh;
	}

	private Mesh CreateCapMesh(int index, int segmentIndex, bool forward)
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[3]
		{
			cornerPoints[segmentIndex].With(null, null, forward ? 0f : (0f - depth)),
			points[index],
			points[index + 1]
		};
		mesh.vertices = array;
		if (!forward)
		{
			mesh.triangles = new int[3] { 0, 1, 2 };
		}
		else
		{
			mesh.triangles = new int[3] { 2, 1, 0 };
		}
		Vector2[] array2 = new Vector2[3];
		for (int i = 0; i < 3; i++)
		{
			array2[i] = array[i] / radius;
		}
		mesh.uv = array2;
		return mesh;
	}

	private Mesh CombineMeshes(Mesh[] meshes)
	{
		List<CombineInstance> list = new List<CombineInstance>();
		for (int i = 0; i < meshes.Length; i++)
		{
			CombineInstance item = default(CombineInstance);
			item.mesh = meshes[i];
			list.Add(item);
		}
		Mesh mesh = new Mesh();
		mesh.CombineMeshes(list.ToArray(), mergeSubMeshes: true, useMatrices: false);
		return mesh;
	}
}
