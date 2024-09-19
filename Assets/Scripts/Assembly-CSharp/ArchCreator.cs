using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ArchCreator : MeshCreator
{
	public enum TextureModes
	{
		Clouds = 0,
		Streach = 1
	}

	public float radius = 10f;

	public float depth = 1f;

	public int corners = 2;

	public int count = 4;

	public TextureModes textureModes;

	private void Awake()
	{
		CreateMesh();
	}

	public override void CreateMesh()
	{
		base.CreateMesh();
		List<Vector3> list = new List<Vector3>();
		Vector3 vector = Vector3.right * radius + Vector3.forward * depth / 2f;
		for (int i = 0; i < corners * count + 1; i++)
		{
			list.Add(vector);
			vector = Quaternion.Euler(0f, 0f, 90f / (float)count) * vector;
		}
		for (int j = 0; j < corners * count + 1; j++)
		{
			vector = list[j] - Vector3.forward * depth;
			list.Add(vector);
		}
		Mesh mesh = new Mesh();
		mesh.name = "Arc Renderer Mesh";
		mesh.vertices = list.ToArray();
		List<int> list2 = new List<int>();
		for (int k = 0; k < corners * count; k++)
		{
			int item = k;
			int num = k.Next(list.Count / 2);
			int item2 = k + list.Count / 2;
			int item3 = num + list.Count / 2;
			list2.Add(item);
			list2.Add(num);
			list2.Add(item2);
			list2.Add(item2);
			list2.Add(num);
			list2.Add(item3);
		}
		mesh.triangles = list2.ToArray();
		Vector2[] array = new Vector2[list.Count];
		for (int l = 0; l < list.Count; l++)
		{
			int num2 = l - ((l >= list.Count / 2) ? (list.Count / 2) : 0);
			if (textureModes == TextureModes.Clouds)
			{
				array[l].x = (float)num2 * radius / depth * 0.125f;
			}
			else
			{
				array[l].x = (float)num2 / ((float)list.Count / 2f);
			}
			array[l].y = ((l >= list.Count / 2) ? 1 : 0);
		}
		mesh.uv = array;
		Color32[] array2 = new Color32[list.Count];
		for (int m = 0; m < list.Count; m++)
		{
			if (m == 0 || m == list.Count / 2 - 1 || m == list.Count / 2 || m == list.Count - 1)
			{
				array2[m] = Color.clear;
			}
			else
			{
				array2[m] = Color.white;
			}
		}
		mesh.colors32 = array2;
		mesh.RecalculateNormals();
		GetComponent<MeshFilter>().sharedMesh = mesh;
		GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
	}
}
