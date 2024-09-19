using UnityEngine;

public static class CubeMesh
{
	public static Vector3Int right = new Vector3Int(1, 0, 0);

	public static Vector3Int up = new Vector3Int(0, 1, 0);

	public static Vector3Int forward = new Vector3Int(0, 0, 1);

	public static Vector3[] vertices = new Vector3[8]
	{
		new Vector3(-1f, -1f, -1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(1f, -1f, 1f),
		new Vector3(-1f, -1f, 1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(1f, 1f, -1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(-1f, 1f, 1f)
	};

	public static Vector2[] uv = new Vector2[4]
	{
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	public static int[] faceTriangles = new int[6] { 0, 1, 2, 2, 3, 0 };

	public static int[] faceFlippedTriangles = new int[6] { 0, 3, 2, 2, 1, 0 };

	public static Vector3Int[] faceDirections = new Vector3Int[6]
	{
		new Vector3Int(0, 0, -1),
		new Vector3Int(0, 0, 1),
		new Vector3Int(-1, 0, 0),
		new Vector3Int(1, 0, 0),
		new Vector3Int(0, -1, 0),
		new Vector3Int(0, 1, 0)
	};

	public static int[][] indices = new int[6][]
	{
		new int[4] { 1, 0, 4, 5 },
		new int[4] { 3, 2, 6, 7 },
		new int[4] { 0, 3, 7, 4 },
		new int[4] { 2, 1, 5, 6 },
		new int[4] { 0, 1, 2, 3 },
		new int[4] { 7, 6, 5, 4 }
	};

	public static Vector3[] GetFaceVertices(int dir)
	{
		Vector3[] array = new Vector3[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = vertices[indices[dir][i]];
		}
		return array;
	}
}
