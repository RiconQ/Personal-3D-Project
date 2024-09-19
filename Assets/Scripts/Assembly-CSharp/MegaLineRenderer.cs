using System;
using System.Collections.Generic;
using UnityEngine;

public class MegaLineRenderer : MeshCreator
{
	public Action OnStopped = delegate
	{
	};

	private Vector3 aPoint;

	private Vector3 bPoint = new Vector3(0f, 0f, 10f);

	public Vector3 normal = Vector3.up;

	public float width = 1f;

	public int desnity = 4;

	public float speed = 0.5f;

	private Vector3[] vertices;

	private Mesh mesh;

	private Vector3 point;

	private MaterialPropertyBlock block;

	private MeshRenderer rend;

	private Color color;

	private float timer;

	public bool onAwake;

	private void Awake()
	{
		if (onAwake)
		{
			Setup();
		}
	}

	public void Setup()
	{
		CreateMesh();
		block = new MaterialPropertyBlock();
		rend = GetComponent<MeshRenderer>();
		rend.allowOcclusionWhenDynamic = false;
		if (!rend.material)
		{
			rend.material = Resources.Load("Materials/Add") as Material;
		}
		mesh = GetComponent<MeshFilter>().sharedMesh;
		color = Color.grey;
		rend.enabled = false;
		base.transform.SetParent(null);
		base.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
	}

	public void SetPointsAndPlay(Vector3 a, Vector3 b, Color c)
	{
		aPoint = base.transform.TransformPoint(a);
		bPoint = base.transform.TransformPoint(b);
		if (vertices.Length == 0)
		{
			CreateMesh();
		}
		else
		{
			UpdateVertices();
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		color = c;
		rend.GetPropertyBlock(block);
		block.SetColor("_TintColor", color);
		rend.SetPropertyBlock(block);
		timer = 0f;
		rend.enabled = true;
	}

	private void FixedUpdate()
	{
		if (timer == 1f)
		{
			return;
		}
		rend.GetPropertyBlock(block);
		block.SetColor("_TintColor", Color.Lerp(color, Color.clear, timer));
		rend.SetPropertyBlock(block);
		for (int i = 0; i < vertices.Length; i++)
		{
			point = vertices[i];
			point.y += Mathf.PerlinNoise((point.x + point.z) * 4f, point.y) * Time.deltaTime;
			vertices[i] = point;
		}
		mesh.vertices = vertices;
		timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
		if (timer == 1f)
		{
			rend.enabled = false;
			if (OnStopped != null)
			{
				OnStopped();
			}
		}
	}

	private void UpdateVertices()
	{
		for (int i = 0; i < desnity + 1; i++)
		{
			Vector3 vector = Vector3.Lerp(aPoint, bPoint, (float)i / (float)desnity);
			vector -= normal * width / 2f;
			vertices[i] = vector;
		}
		for (int j = 0; j < desnity + 1; j++)
		{
			Vector3 vector = Vector3.Lerp(aPoint, bPoint, (float)j / (float)desnity);
			vector += normal * width / 2f;
			vertices[j + desnity + 1] = vector;
		}
	}

	public override void CreateMesh()
	{
		base.CreateMesh();
		_ = (aPoint - bPoint).magnitude;
		int num = (desnity + 1) * 2;
		vertices = new Vector3[num];
		UpdateVertices();
		Mesh mesh = new Mesh();
		mesh.name = "MegaLine Renderer Mesh";
		mesh.vertices = vertices;
		List<int> list = new List<int>();
		for (int i = 0; i < num / 2; i++)
		{
			int item = i;
			int num2 = i.Next(num / 2);
			int item2 = i + num / 2;
			int item3 = num2 + num / 2;
			list.Add(item);
			list.Add(num2);
			list.Add(item2);
			list.Add(item2);
			list.Add(num2);
			list.Add(item3);
		}
		mesh.triangles = list.ToArray();
		Vector2[] array = new Vector2[num];
		for (int j = 0; j < num; j++)
		{
			int num3 = j - ((j >= num / 2) ? (num / 2) : 0);
			array[j].x = (float)num3 / ((float)num / 2f);
			array[j].y = ((j >= num / 2) ? 1 : 0);
		}
		mesh.uv = array;
		Color32[] array2 = new Color32[num];
		for (int k = 0; k < num; k++)
		{
			if (k == 0 || k == num / 2 - 1 || k == num / 2 || k == num - 1)
			{
				array2[k] = Color.clear;
			}
			else
			{
				array2[k] = Color.white;
			}
		}
		mesh.colors32 = array2;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		GetComponent<MeshFilter>().sharedMesh = mesh;
	}
}
