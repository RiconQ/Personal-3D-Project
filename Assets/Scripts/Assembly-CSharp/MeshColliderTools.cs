using System;
using System.Collections.Generic;
using UnityEngine;

public static class MeshColliderTools
{
	private class Vertice
	{
		public List<Edge> edges = new List<Edge>();

		public Vector3 position;

		public int finalIndex;

		public Vector3? linearPosition;

		public bool IsStatic
		{
			get
			{
				Vector3? vector = linearPosition;
				Vector3 zero = Vector3.zero;
				if (!vector.HasValue)
				{
					return false;
				}
				if (!vector.HasValue)
				{
					return true;
				}
				return vector.GetValueOrDefault() == zero;
			}
		}

		public void AssignLinearPosition()
		{
			for (int i = 0; i < edges.Count; i++)
			{
				Edge edge = edges[i];
				if (!edge.HasEqualPlanes())
				{
					if (!linearPosition.HasValue)
					{
						linearPosition = edge.vertices[1].position - edge.vertices[0].position;
					}
					else if (!edge.IsParallel(linearPosition))
					{
						linearPosition = Vector3.zero;
						break;
					}
				}
			}
		}

		public Edge GetExistingConnectingEdge(Vertice v)
		{
			foreach (Edge edge in edges)
			{
				if (edge.vertices[0] == this && edge.vertices[1] == v)
				{
					return edge;
				}
				if (edge.vertices[1] == this && edge.vertices[0] == v)
				{
					return edge;
				}
			}
			return null;
		}

		public Edge GetConnectingEdge(Vertice v)
		{
			Edge edge = GetExistingConnectingEdge(v);
			if (edge == null)
			{
				edge = new Edge(this, v);
				edges.Add(edge);
				v.edges.Add(edge);
			}
			return edge;
		}

		public bool CanFollow(Edge transportEdge, out Vertice opposite)
		{
			if (IsStatic)
			{
				opposite = null;
				return false;
			}
			if (linearPosition.HasValue && !transportEdge.IsParallel(linearPosition))
			{
				opposite = null;
				return false;
			}
			HashSet<Face> hashSet = new HashSet<Face>();
			foreach (Edge edge in edges)
			{
				foreach (Face face in edge.faces)
				{
					hashSet.Add(face);
				}
			}
			hashSet.ExceptWith(transportEdge.faces);
			opposite = transportEdge.GetOpposite(this);
			Vector3 p = opposite.position;
			HashSet<Face>.Enumerator enumerator3 = hashSet.GetEnumerator();
			try
			{
				while (enumerator3.MoveNext())
				{
					if (enumerator3.Current.MoveWouldFlip(this, p))
					{
						return false;
					}
				}
			}
			finally
			{
				enumerator3.Dispose();
			}
			return true;
		}

		public void DisconnectFrom(Edge e)
		{
			edges.Remove(e);
		}

		public void Disconnect()
		{
			edges.Clear();
			edges = null;
		}
	}

	private class Edge
	{
		public List<Vertice> vertices;

		public List<Face> faces;

		public static void Collapse(Edge moved, Edge target, Face f)
		{
			Face opposite;
			try
			{
				opposite = moved.GetOpposite(f);
			}
			catch (Exception ex)
			{
				string[] obj = new string[5] { ex.Message, "\n", null, null, null };
				Vector3 position = moved.vertices[0].position;
				obj[2] = position.ToString();
				obj[3] = " <--> ";
				position = moved.vertices[1].position;
				obj[4] = position.ToString();
				throw new Exception(string.Concat(obj));
			}
			opposite.Replace(moved, target);
			target.Replace(f, opposite);
			foreach (Vertice vertex in moved.vertices)
			{
				vertex.edges.Remove(moved);
			}
		}

		public Edge(Vertice v0, Vertice v1)
		{
			vertices = new List<Vertice>(2);
			vertices.Add(v0);
			vertices.Add(v1);
			faces = new List<Face>(2);
		}

		public Vertice GetOpposite(Vertice v)
		{
			Vertice vertice = vertices[0];
			if (v == vertice)
			{
				return vertices[1];
			}
			return vertice;
		}

		public Face GetOpposite(Face v)
		{
			if (faces.Count != 2)
			{
				throw new Exception("Collapsing an edge with only 1 face into another. This is not supported.");
			}
			Face face = faces[0];
			if (face != v)
			{
				return face;
			}
			return faces[1];
		}

		public bool HasEqualPlanes()
		{
			if (faces.Count != 2)
			{
				return false;
			}
			Face face = faces[0];
			Edge edge = faces[0].edges[0];
			Face face2 = faces[1];
			Edge edge2 = faces[1].edges[0];
			Edge edge3 = ((edge != this) ? edge : face.edges[1]);
			Edge edge4 = ((edge2 != this) ? edge2 : face2.edges[1]);
			Vector3 lhs = vertices[1].position - vertices[0].position;
			Vector3 rhs = edge3.vertices[1].position - edge3.vertices[0].position;
			float num = Vector3.Dot(rhs: edge4.vertices[1].position - edge4.vertices[0].position, lhs: Vector3.Cross(lhs, rhs));
			if (-0.005 < (double)num)
			{
				return (double)num < 0.005;
			}
			return false;
		}

		public void Replace(Face oldFace, Face newFace)
		{
			for (int i = 0; i < faces.Count; i++)
			{
				if (faces[i] == oldFace)
				{
					faces[i] = newFace;
					break;
				}
			}
		}

		public void Reconnect(Vertice oldVertice, Vertice newVertice)
		{
			if (vertices[0] == oldVertice)
			{
				vertices[0] = newVertice;
			}
			else
			{
				vertices[1] = newVertice;
			}
			newVertice.edges.Add(this);
		}

		public bool Contains(Vertice v)
		{
			if (v != vertices[0])
			{
				return v == vertices[1];
			}
			return true;
		}

		public bool IsParallel(Vector3? nv)
		{
			Vector3 position = vertices[0].position;
			float sqrMagnitude = Vector3.Cross(vertices[1].position - position, nv.Value).sqrMagnitude;
			if (-5E-06f < sqrMagnitude)
			{
				return sqrMagnitude < 5E-06f;
			}
			return false;
		}

		public void DisconnectIncludingFaces()
		{
			vertices.Clear();
			vertices = null;
			foreach (Face face in faces)
			{
				face.Disconnect();
			}
			faces.Clear();
			faces = null;
		}
	}

	private class Face
	{
		public Edge[] edges;

		private Face(Edge e0, Edge e1, Edge e2)
		{
			edges = new Edge[3] { e0, e1, e2 };
		}

		public static void AddFace(List<Vertice> allVertices, int[] tris, int triIndex)
		{
			Vertice vertice = allVertices[tris[triIndex]];
			Vertice vertice2 = allVertices[tris[triIndex + 1]];
			Vertice v = allVertices[tris[triIndex + 2]];
			Edge connectingEdge = vertice.GetConnectingEdge(vertice2);
			Edge connectingEdge2 = vertice.GetConnectingEdge(v);
			Edge connectingEdge3 = vertice2.GetConnectingEdge(v);
			Face item = new Face(connectingEdge, connectingEdge2, connectingEdge3);
			connectingEdge.faces.Add(item);
			connectingEdge2.faces.Add(item);
			connectingEdge3.faces.Add(item);
		}

		public Edge GetOpposite(Vertice v)
		{
			Edge result;
			if (!(result = edges[0]).Contains(v))
			{
				return result;
			}
			Edge result2;
			if (!(result2 = edges[1]).Contains(v))
			{
				return result2;
			}
			return edges[2];
		}

		public Vertice GetOpposite(Edge o)
		{
			Vertice vertice = o.vertices[0];
			Vertice vertice2 = o.vertices[1];
			for (int i = 0; i < 3; i++)
			{
				Edge edge = edges[i];
				Vertice vertice3 = edge.vertices[0];
				if (vertice3 != vertice && vertice3 != vertice2)
				{
					return vertice3;
				}
				Vertice vertice4 = edge.vertices[1];
				if (vertice4 != vertice && vertice4 != vertice2)
				{
					return vertice4;
				}
			}
			throw new Exception("A face seems to have three edges that all share a vertice with a given edge.");
		}

		public void Replace(Edge oldEdge, Edge newEdge)
		{
			if (edges[0] == oldEdge)
			{
				edges[0] = newEdge;
			}
			else if (edges[1] == oldEdge)
			{
				edges[1] = newEdge;
			}
			else
			{
				edges[2] = newEdge;
			}
		}

		public bool MoveWouldFlip(Vertice v, Vector3 p)
		{
			Edge opposite = GetOpposite(v);
			Vector3 position = opposite.vertices[0].position;
			Vector3 rhs = opposite.vertices[1].position - position;
			Vector3 lhs = p - position;
			Vector3 lhs2 = v.position - position;
			Vector3 lhs3 = Vector3.Cross(lhs, rhs);
			if (lhs3.sqrMagnitude < 0.0001f)
			{
				return true;
			}
			Vector3 rhs2 = Vector3.Cross(lhs2, rhs);
			if (rhs2.sqrMagnitude < 0.0001f)
			{
				return true;
			}
			if (Mathf.Sign(Vector3.Dot(lhs3, rhs2)) < 0f)
			{
				return true;
			}
			return false;
		}

		public void GetIndexes(List<int> results)
		{
			results.Add(GetOpposite(edges[2]).finalIndex);
			results.Add(GetOpposite(edges[1]).finalIndex);
			results.Add(GetOpposite(edges[0]).finalIndex);
		}

		public void Disconnect()
		{
			edges = null;
		}
	}

	public static void SnapToGrid(this Mesh mesh, float gridDelta)
	{
		if (!(gridDelta < 1E-05f))
		{
			float num = 1f / gridDelta;
			Vector3[] vertices = mesh.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i].x = (float)Mathf.RoundToInt(vertices[i].x * num) / num;
				vertices[i].y = (float)Mathf.RoundToInt(vertices[i].y * num) / num;
				vertices[i].z = (float)Mathf.RoundToInt(vertices[i].z * num) / num;
			}
			mesh.vertices = vertices;
		}
	}

	public static void Weld(this Mesh mesh, float threshold, float bucketStep)
	{
		Vector3[] vertices = mesh.vertices;
		Vector3[] array = new Vector3[vertices.Length];
		int[] array2 = new int[vertices.Length];
		int num = 0;
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < vertices.Length; i++)
		{
			if (vertices[i].x < vector.x)
			{
				vector.x = vertices[i].x;
			}
			if (vertices[i].y < vector.y)
			{
				vector.y = vertices[i].y;
			}
			if (vertices[i].z < vector.z)
			{
				vector.z = vertices[i].z;
			}
			if (vertices[i].x > vector2.x)
			{
				vector2.x = vertices[i].x;
			}
			if (vertices[i].y > vector2.y)
			{
				vector2.y = vertices[i].y;
			}
			if (vertices[i].z > vector2.z)
			{
				vector2.z = vertices[i].z;
			}
		}
		vector -= Vector3.one * 0.111111f;
		vector2 += Vector3.one * 0.899999f;
		int num2 = Mathf.FloorToInt((vector2.x - vector.x) / bucketStep) + 1;
		int num3 = Mathf.FloorToInt((vector2.y - vector.y) / bucketStep) + 1;
		int num4 = Mathf.FloorToInt((vector2.z - vector.z) / bucketStep) + 1;
		List<int>[,,] array3 = new List<int>[num2, num3, num4];
		for (int j = 0; j < vertices.Length; j++)
		{
			int num5 = Mathf.FloorToInt((vertices[j].x - vector.x) / bucketStep);
			int num6 = Mathf.FloorToInt((vertices[j].y - vector.y) / bucketStep);
			int num7 = Mathf.FloorToInt((vertices[j].z - vector.z) / bucketStep);
			if (array3[num5, num6, num7] == null)
			{
				array3[num5, num6, num7] = new List<int>();
			}
			int num8 = 0;
			while (true)
			{
				if (num8 < array3[num5, num6, num7].Count)
				{
					if (Vector3.SqrMagnitude(array[array3[num5, num6, num7][num8]] - vertices[j]) < 0.001f)
					{
						array2[j] = array3[num5, num6, num7][num8];
						break;
					}
					num8++;
					continue;
				}
				array[num] = vertices[j];
				array3[num5, num6, num7].Add(num);
				array2[j] = num;
				num++;
				break;
			}
		}
		int[] triangles = mesh.triangles;
		int[] array4 = new int[triangles.Length];
		for (int k = 0; k < triangles.Length; k++)
		{
			array4[k] = array2[triangles[k]];
		}
		Vector3[] array5 = new Vector3[num];
		for (int l = 0; l < num; l++)
		{
			array5[l] = array[l];
		}
		mesh.Clear();
		mesh.vertices = array5;
		mesh.triangles = array4;
	}

	public static void Simplify(this MeshCollider meshCollider)
	{
		meshCollider.sharedMesh.Simplify();
	}

	public static void Simplify(this Mesh mesh)
	{
		Vector3[] vertices = mesh.vertices;
		int num = vertices.Length;
		List<Vertice> list = new List<Vertice>(num);
		for (int i = 0; i < num; i++)
		{
			Vertice vertice = new Vertice();
			vertice.position = vertices[i];
			list.Add(vertice);
		}
		int[] triangles = mesh.triangles;
		int num2 = triangles.Length;
		for (int j = 0; j < num2; j += 3)
		{
			Face.AddFace(list, triangles, j);
		}
		for (int k = 0; k < num; k++)
		{
			list[k].AssignLinearPosition();
		}
		HashSet<Vertice> hashSet = new HashSet<Vertice>();
		foreach (Vertice item in list)
		{
			if (!item.IsStatic)
			{
				hashSet.Add(item);
			}
		}
		while (hashSet.Count != 0)
		{
			HashSet<Vertice> hashSet2 = hashSet;
			hashSet = new HashSet<Vertice>();
			foreach (Vertice item2 in hashSet2)
			{
				if (item2.edges == null)
				{
					continue;
				}
				foreach (Edge edge in item2.edges)
				{
					if (!item2.CanFollow(edge, out var opposite))
					{
						continue;
					}
					foreach (Face face in edge.faces)
					{
						Edge.Collapse(face.GetOpposite(opposite), face.GetOpposite(item2), face);
					}
					foreach (Edge edge2 in item2.edges)
					{
						if (edge2 != edge)
						{
							Vertice opposite2 = edge2.GetOpposite(item2);
							if (!opposite2.IsStatic)
							{
								hashSet.Add(opposite2);
							}
							edge2.Reconnect(item2, opposite);
						}
					}
					if (!opposite.IsStatic)
					{
						hashSet.Add(opposite);
					}
					opposite.DisconnectFrom(edge);
					item2.Disconnect();
					edge.DisconnectIncludingFaces();
					break;
				}
			}
		}
		List<Vertice> list2 = new List<Vertice>();
		foreach (Vertice item3 in list)
		{
			if (item3.edges != null)
			{
				list2.Add(item3);
			}
		}
		int count = list2.Count;
		Vector3[] array = new Vector3[count];
		for (int l = 0; l < count; l++)
		{
			list2[l].finalIndex = l;
			array[l] = list2[l].position;
		}
		List<int> list3 = new List<int>();
		HashSet<Face> hashSet3 = new HashSet<Face>();
		foreach (Vertice item4 in list2)
		{
			foreach (Edge edge3 in item4.edges)
			{
				foreach (Face face2 in edge3.faces)
				{
					if (!hashSet3.Contains(face2))
					{
						hashSet3.Add(face2);
						face2.GetIndexes(list3);
					}
				}
			}
		}
		mesh.Clear();
		mesh.vertices = array;
		mesh.triangles = list3.ToArray();
		mesh.RecalculateBounds();
		mesh.Optimize();
	}
}
