using System;
using UnityEngine;

[Serializable]
public class VinePoint
{
	public Vector3 point;

	public Vector3 normal;

	public Vector3 tangent;

	public Quaternion rotation;

	public int desnity;

	public VinePoint(Vector3 p, Vector3 n)
	{
		point = p;
		normal = n;
	}
}
