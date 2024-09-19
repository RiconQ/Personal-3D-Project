using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class ChainSkinnedRenderer : MonoBehaviour
{
	public Mesh chainSegment;

	public float segmentLength = 2f;

	public Transform[] tBones;

	public bool zForward;

	private Transform t;

	private SkinnedMeshRenderer mr;

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
		mr = GetComponent<SkinnedMeshRenderer>();
	}

	public void GenerateChainMesh(Vector3[] poses)
	{
		if (!t)
		{
			t = base.transform;
		}
		if (!mr)
		{
			mr = GetComponent<SkinnedMeshRenderer>();
		}
		chainSegment.GetVertices(segmentVerts);
		chainSegment.GetUVs(0, segmentUVs);
		chainSegment.GetTriangles(segmentTris, 0);
		chainSegment.GetColors(segmentColors);
		int num = poses.Length;
		Debug.Log(num);
		Mesh mesh = new Mesh();
		mesh.name = "ChainMesh";
		vertices.Clear();
		tris.Clear();
		uvs.Clear();
		colors.Clear();
		tBones = new Transform[num];
		BoneWeight[] array = new BoneWeight[num * segmentVerts.Count];
		Matrix4x4[] array2 = new Matrix4x4[num];
		for (int i = 0; i < num; i++)
		{
			tBones[i] = new GameObject("Bone").transform;
			tBones[i].SetParent(t);
			tBones[i].localPosition = poses[i];
			for (int j = 0; j < segmentVerts.Count; j++)
			{
				temp = segmentVerts[j];
				if (!zForward)
				{
					temp.y -= (float)i * segmentLength;
				}
				else
				{
					temp.z += (float)i * segmentLength;
				}
				vertices.Add(temp);
				array[i * segmentVerts.Count + j].boneIndex0 = i;
				array[i * segmentVerts.Count + j].weight0 = 1f;
			}
			array2[i] = tBones[i].worldToLocalMatrix * t.root.localToWorldMatrix;
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
		mesh.boneWeights = array;
		mesh.bindposes = array2;
		mr.sharedMesh = mesh;
		mr.localBounds = mesh.bounds;
		mr.bones = tBones;
	}
}
