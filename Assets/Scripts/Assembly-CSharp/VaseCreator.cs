using System;
using System.Collections.Generic;
using UnityEngine;

public class VaseCreator : MeshCreator
{
	public Action OnMeshCreated = delegate
	{
	};

	public float height = 1f;

	public float baseWidth = 0.5f;

	public float width = 1f;

	public int xDesnity = 12;

	public int yDesnity = 8;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private List<Vector3> points = new List<Vector3>();

	private List<Mesh> meshes = new List<Mesh>();

	private Vector3 dir = Vector3.forward;

	public override void CreateMesh()
	{
		points.Clear();
		meshes.Clear();
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < yDesnity; i++)
		{
			num = (float)i / (float)(yDesnity - 1);
			num2 = curve.Evaluate(num) * width + baseWidth;
			dir = Vector3.forward * num2;
			dir.y = num * height;
			for (int j = 0; j < xDesnity + 1; j++)
			{
				dir = Quaternion.Euler(0f, 360f / (float)xDesnity, 0f) * dir;
				points.Add(dir);
			}
			if (i == yDesnity - 1)
			{
				dir = Vector3.forward * (num2 - 0.1f);
				dir.y = num * height;
				for (int k = 0; k < xDesnity + 1; k++)
				{
					dir = Quaternion.Euler(0f, 360f / (float)xDesnity, 0f) * dir;
					points.Add(dir);
				}
			}
		}
		meshes.Add(CreateVase());
		meshes.Add(CreateCap());
		meshes.Add(CreateCap(top: true));
		Mesh mesh = MegaHelp.CombineMeshes(meshes.ToArray());
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		GetComponent<MeshFilter>().sharedMesh = mesh;
		if (OnMeshCreated != null)
		{
			OnMeshCreated();
		}
	}

	private void OnDrawGizmos()
	{
		for (int i = 0; i < points.Count; i++)
		{
			Gizmos.DrawLine(points[i], points[i.Next(points.Count)]);
		}
	}

	private Mesh CreateCap(bool top = false)
	{
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>(xDesnity + 2);
		list = (top ? points.GetRange(points.Count - xDesnity - 1, xDesnity + 1) : points.GetRange(0, xDesnity + 1));
		list.Add(new Vector3(0f, top ? (height / 2f) : 0f, 0f));
		mesh.vertices = list.ToArray();
		List<int> list2 = new List<int>();
		for (int i = 0; i < list.Count - 1; i++)
		{
			list2.Add(i + ((!top) ? 1 : 0));
			list2.Add(i + (top ? 1 : 0));
			list2.Add(list.Count - 1);
		}
		mesh.triangles = list2.ToArray();
		Vector2[] array = new Vector2[list.Count];
		for (int j = 0; j < array.Length; j++)
		{
			array[j].x = list[j].x;
			array[j].y = list[j].z;
		}
		Color32[] array2 = new Color32[list.Count];
		for (int k = 0; k < array2.Length; k++)
		{
			array2[k] = Color.black;
		}
		mesh.colors32 = array2;
		return mesh;
	}

	private Mesh CreateVase()
	{
		Mesh mesh = new Mesh();
		mesh.name = "Vase";
		mesh.vertices = points.ToArray();
		List<int> list = new List<int>();
		for (int i = 0; i < yDesnity; i++)
		{
			for (int j = 0; j < xDesnity; j++)
			{
				int num = xDesnity + 1;
				int num2 = j + i * num;
				int num3 = j.Next(num) + i * num;
				int item = num2 + num;
				int item2 = num3 + num;
				list.Add(num2);
				list.Add(num3);
				list.Add(item);
				list.Add(item);
				list.Add(num3);
				list.Add(item2);
			}
		}
		mesh.triangles = list.ToArray();
		Vector2[] array = new Vector2[points.Count];
		for (int k = 0; k < array.Length; k++)
		{
			int num4 = xDesnity + 1;
			int num5 = Mathf.FloorToInt(k / num4);
			array[k].x = (float)(k - num5 * num4) / (float)(num4 - 1) * 2f;
			array[k].y = (float)num5 / (float)(yDesnity - 1);
		}
		mesh.uv = array;
		return mesh;
	}
}
