using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GenerateMesh02 : MonoBehaviour
{
	public struct ExtrudeShape
	{
		public Vertex[] vert2Ds;

		public int[] lines;

		public ExtrudeShape(Vertex[] vert2Ds, int[] lines)
		{
			this.vert2Ds = vert2Ds;
			this.lines = lines;
		}
	}

	public struct Vertex
	{
		public Vector3 point;

		public Vector3 normal;

		public float uCoord;

		public Vertex(Vector3 point, Vector3 normal, float uCoord)
		{
			this.point = point;
			this.normal = normal;
			this.uCoord = uCoord;
		}
	}

	public struct OrientedPoint
	{
		public Vector3 position;

		public Quaternion rotation;

		public OrientedPoint(Vector3 position, Quaternion rotation)
		{
			this.position = position;
			this.rotation = rotation;
		}

		public Vector3 LocalToWorld(Vector3 point)
		{
			return position + rotation * point;
		}

		public Vector3 WorldToLocal(Vector3 point)
		{
			return Quaternion.Inverse(rotation) * (point - position);
		}

		public Vector3 LocalToWorldDirection(Vector3 dir)
		{
			return rotation * dir;
		}
	}

	private MeshFilter mf;

	public List<Vector3> pathPoints = new List<Vector3>();

	private void OnDrawGizmos()
	{
		if (pathPoints.Count >= 2)
		{
			for (int i = 0; i < pathPoints.Count - 1; i++)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
			}
		}
	}

	private void Start()
	{
		mf = GetComponent<MeshFilter>();
		GenerateMesh();
	}

	private void GenerateMesh()
	{
		Mesh mesh = GetMesh();
		ExtrudeShape extrudeShape = GetExtrudeShape();
		OrientedPoint[] path = GetPath();
		Extrude(mesh, extrudeShape, path);
	}

	private ExtrudeShape GetExtrudeShape()
	{
		Vertex[] vert2Ds = new Vertex[4]
		{
			new Vertex(new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), 0f),
			new Vertex(new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), 0f),
			new Vertex(new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), 0f),
			new Vertex(new Vector3(1f, -1f, 0f), new Vector3(1f, 0f, 0f), 0f)
		};
		int[] lines = new int[8] { 0, 1, 1, 2, 2, 3, 3, 0 };
		return new ExtrudeShape(vert2Ds, lines);
	}

	private OrientedPoint[] GetPath()
	{
		List<OrientedPoint> list = new List<OrientedPoint>();
		for (int i = 0; i < pathPoints.Count - 1; i++)
		{
			Vector3 vector = pathPoints[i].DirTo(pathPoints[i + 1]);
			Vector3[] p = new Vector3[4]
			{
				pathPoints[i],
				pathPoints[i] + vector * 8f,
				pathPoints[i + 1] - vector * 9f,
				pathPoints[i + 1]
			};
			for (float num = 0f; num <= 1f; num += 0.1f)
			{
				Vector3 point = GetPoint(p, num);
				Quaternion orientation3D = GetOrientation3D(p, num, Vector3.up);
				list.Add(new OrientedPoint(point, orientation3D));
			}
		}
		return list.ToArray();
	}

	private OrientedPoint[] GetPath2()
	{
		Vector3[] p = new Vector3[4]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(0f, 0f, 10f),
			new Vector3(10f, 0f, 10f),
			new Vector3(20f, 0f, 0f)
		};
		List<OrientedPoint> list = new List<OrientedPoint>();
		for (float num = 0f; num <= 1f; num += 0.1f)
		{
			Vector3 point = GetPoint(p, num);
			Quaternion orientation3D = GetOrientation3D(p, num, Vector3.up);
			list.Add(new OrientedPoint(point, orientation3D));
		}
		return list.ToArray();
	}

	private Mesh GetMesh()
	{
		if (mf.sharedMesh == null)
		{
			mf.sharedMesh = new Mesh();
		}
		return mf.sharedMesh;
	}

	private Vector3 GetPoint(Vector3[] p, float t)
	{
		float num = 1f - t;
		float num2 = num * num;
		float num3 = t * t;
		return p[0] * (num2 * num) + p[1] * (3f * num2 * t) + p[2] * (3f * num * num3) + p[3] * (num3 * t);
	}

	private Vector3 GetTangent(Vector3[] p, float t)
	{
		float num = 1f - t;
		float num2 = num * num;
		float num3 = t * t;
		return (p[0] * (0f - num2) + p[1] * (3f * num2 - 2f * num) + p[2] * (-3f * num3 + 2f * t) + p[3] * num3).normalized;
	}

	private Vector3 GetNormal3D(Vector3[] p, float t, Vector3 up)
	{
		Vector3 tangent = GetTangent(p, t);
		Vector3 normalized = Vector3.Cross(up, tangent).normalized;
		return Vector3.Cross(tangent, normalized);
	}

	private Quaternion GetOrientation3D(Vector3[] p, float t, Vector3 up)
	{
		Vector3 tangent = GetTangent(p, t);
		Vector3 normal3D = GetNormal3D(p, t, up);
		return Quaternion.LookRotation(tangent, normal3D);
	}

	private void Extrude(Mesh mesh, ExtrudeShape shape, OrientedPoint[] path)
	{
		int num = shape.vert2Ds.Length;
		int num2 = path.Length - 1;
		int num3 = path.Length;
		int num4 = num * num3;
		int[] array = new int[shape.lines.Length * num2 * 3];
		Vector3[] array2 = new Vector3[num4];
		Vector3[] array3 = new Vector3[num4];
		Vector2[] array4 = new Vector2[num4];
		float num5 = 0f;
		float num6 = 0f;
		for (int i = 0; i < path.Length - 1; i++)
		{
			float num7 = Vector3.Distance(path[i].position, path[i + 1].position);
			num5 += num7;
		}
		for (int j = 0; j < path.Length; j++)
		{
			int num8 = j * num;
			if (j > 0)
			{
				float num9 = Vector3.Distance(path[j].position, path[j - 1].position);
				num6 += num9;
			}
			float y = num6 / num5;
			for (int k = 0; k < num; k++)
			{
				int num10 = num8 + k;
				array2[num10] = path[j].LocalToWorld(shape.vert2Ds[k].point);
				array3[num10] = path[j].LocalToWorldDirection(shape.vert2Ds[k].normal);
				array4[num10] = new Vector2(shape.vert2Ds[k].uCoord, y);
			}
		}
		int num11 = 0;
		for (int l = 0; l < num2; l++)
		{
			int num12 = l * num;
			for (int m = 0; m < shape.lines.Length; m += 2)
			{
				int num13 = num12 + shape.lines[m] + num;
				int num14 = num12 + shape.lines[m];
				int num15 = num12 + shape.lines[m + 1];
				int num16 = num12 + shape.lines[m + 1] + num;
				array[num11] = num15;
				num11++;
				array[num11] = num14;
				num11++;
				array[num11] = num13;
				num11++;
				array[num11] = num13;
				num11++;
				array[num11] = num16;
				num11++;
				array[num11] = num15;
				num11++;
			}
		}
		mesh.Clear();
		mesh.vertices = array2;
		mesh.normals = array3;
		mesh.uv = array4;
		mesh.triangles = array;
	}
}
