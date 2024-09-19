using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MegaCubeRegion : ISerializationCallbackReceiver
{
	public Vector3Int origin;

	public HashSet<Vector3Int> points = new HashSet<Vector3Int>();

	public Vector3 center;

	public Vector3 size;

	public GameObject root;

	public Transform tRoot;

	public MeshFilter filter;

	public MeshRenderer rend;

	public MeshCollider clldr;

	public Mesh mesh;

	[SerializeField]
	private List<Vector3Int> s_Points = new List<Vector3Int>();

	public void OnBeforeSerialize()
	{
		s_Points.Clear();
		foreach (Vector3Int point in points)
		{
			s_Points.Add(point);
		}
	}

	public void OnAfterDeserialize()
	{
		points.Clear();
		foreach (Vector3Int s_Point in s_Points)
		{
			points.Add(s_Point);
		}
	}
}
