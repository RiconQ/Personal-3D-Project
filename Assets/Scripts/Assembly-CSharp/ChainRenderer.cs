using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChainRenderer : MonoBehaviour
{
	public Mesh chainSegment;

	public float segmentLength = 2f;

	private Transform t;

	private MeshFilter mf;

	private MeshRenderer mr;

	private Vector3 posA;

	private Vector3 posB;

	private Vector3 temp;

	private List<Vector3> segmentVerts = new List<Vector3>();

	private List<Vector2> segmentUVs = new List<Vector2>();

	private List<int> segmentTris = new List<int>();

	private List<Color> segmentColors = new List<Color>();

	private List<Vector3> vertices = new List<Vector3>();

	private List<Vector2> uvs = new List<Vector2>();

	private List<int> tris = new List<int>();

	private List<Color> colors = new List<Color>();

	private void Awake()
	{
		t = base.transform;
		mf = GetComponent<MeshFilter>();
		mr = GetComponent<MeshRenderer>();
	}

	[Button]
	public void Test()
	{
		posA = t.position;
		posB = t.position;
		posB.y -= 10f;
		GenerateChainMesh(posA, posB);
	}

	public void GenerateChainMesh(Vector3 a, Vector3 b)
	{
		t = base.transform;
		mf = GetComponent<MeshFilter>();
		mr = GetComponent<MeshRenderer>();
		chainSegment.GetVertices(segmentVerts);
		chainSegment.GetUVs(0, segmentUVs);
		chainSegment.GetTriangles(segmentTris, 0);
		chainSegment.GetColors(segmentColors);
		float num = Vector3.Distance(a, b);
		int num2 = Mathf.RoundToInt(num / segmentLength);
		float num3 = num / (float)num2;
		Debug.Log(num2);
		Mesh mesh = new Mesh();
		mesh.name = "ChainMesh";
		vertices.Clear();
		tris.Clear();
		uvs.Clear();
		colors.Clear();
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < segmentVerts.Count; j++)
			{
				temp = segmentVerts[j];
				temp.y -= (float)i * num3;
				vertices.Add(temp);
			}
			uvs.AddRange(segmentUVs);
			for (int k = 0; k < segmentTris.Count; k++)
			{
				tris.Add(segmentTris[k] + i * segmentVerts.Count);
			}
			colors.AddRange(segmentColors);
		}
		mesh.SetVertices(vertices);
		mesh.SetUVs(0, uvs);
		mesh.SetTriangles(tris, 0);
		mesh.SetColors(colors);
		mesh.RecalculateNormals();
		mesh.Optimize();
		mf.sharedMesh = mesh;
	}
}
