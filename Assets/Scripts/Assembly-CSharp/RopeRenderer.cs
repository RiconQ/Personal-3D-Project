using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class RopeRenderer : MonoBehaviour
{
	public float radius = 0.1f;

	public Transform[] tBones;

	private SkinnedMeshRenderer mr;

	private Vector3[] offsets = new Vector3[4]
	{
		new Vector3(0f, 0f, -1f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f)
	};

	public void GenerateRopeMesh(Vector3 posA, Vector3 posB)
	{
		int num = Mathf.RoundToInt(Vector3.Distance(posB, posA)) + 1;
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			if ((bool)componentsInChildren[i])
			{
				Object.DestroyImmediate(componentsInChildren[i].gameObject);
			}
		}
		tBones = new Transform[num];
		List<Vector3> list = new List<Vector3>();
		List<Vector2> list2 = new List<Vector2>();
		List<int> list3 = new List<int>();
		BoneWeight[] array = new BoneWeight[num * 5];
		Matrix4x4[] array2 = new Matrix4x4[num];
		Vector3 vector = default(Vector3);
		for (int j = 0; j < num; j++)
		{
			tBones[j] = new GameObject().transform;
			tBones[j].SetParent(base.transform);
			vector.x = (vector.z = 0f);
			vector.y = -j;
			tBones[j].localPosition = vector;
			tBones[j].localRotation = Quaternion.LookRotation(Vector3.down);
			for (int k = 0; k < offsets.Length + 1; k++)
			{
				int num2 = ((k != offsets.Length) ? k : 0);
				vector.x = offsets[num2].x * radius;
				vector.z = offsets[num2].z * radius;
				vector.y = -j;
				list.Add(vector);
				array[j * 5 + k].boneIndex0 = j;
				array[j * 5 + k].weight0 = 1f;
			}
			array2[j] = tBones[j].worldToLocalMatrix * base.transform.localToWorldMatrix;
		}
		int num3 = 1 + (list.Count - 10) / 5;
		for (int l = 0; l < num3; l++)
		{
			for (int m = 0; m < 4; m++)
			{
				int num4 = l * 5 + m;
				int item = ((m < 6) ? (num4 + 1) : (l * 5));
				int num5 = num4 + 5;
				int item2 = ((m < 6) ? (num5 + 1) : (l * 5 + 5));
				list3.Add(num4);
				list3.Add(num5);
				list3.Add(item);
				list3.Add(item);
				list3.Add(num5);
				list3.Add(item2);
			}
		}
		for (int n = 0; n < list.Count; n++)
		{
			Vector2 item3 = default(Vector2);
			item3.y = Mathf.FloorToInt((float)n / 5f);
			item3.x = (float)(n - Mathf.FloorToInt((float)n / 5f) * 5) / 4f;
			list2.Add(item3);
		}
		Mesh mesh = new Mesh();
		mesh.SetVertices(list);
		mesh.SetTriangles(list3, 0);
		mesh.SetUVs(0, list2);
		mesh.boneWeights = array;
		mesh.bindposes = array2;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		if (!mr)
		{
			mr = GetComponentInChildren<SkinnedMeshRenderer>();
		}
		mr.localBounds = mesh.bounds;
		mr.bones = tBones;
		mr.sharedMesh = mesh;
	}
}
