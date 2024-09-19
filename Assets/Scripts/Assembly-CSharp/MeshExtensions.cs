using System.Collections.Generic;
using UnityEngine;

public static class MeshExtensions
{
	public static Mesh Combine(this List<Mesh> meshes, bool mergeSubMeshes = true, Transform t = null)
	{
		List<CombineInstance> list = new List<CombineInstance>(meshes.Count);
		for (int i = 0; i < meshes.Count; i++)
		{
			CombineInstance item = default(CombineInstance);
			item.mesh = meshes[i];
			if (t != null)
			{
				item.transform = t.worldToLocalMatrix;
			}
			list.Add(item);
		}
		Mesh mesh = new Mesh();
		mesh.CombineMeshes(list.ToArray(), mergeSubMeshes, t != null);
		return mesh;
	}

	public static Mesh Combine(this Mesh[] meshes, bool mergeSubMeshes = true, Transform t = null)
	{
		List<CombineInstance> list = new List<CombineInstance>(meshes.Length);
		for (int i = 0; i < meshes.Length; i++)
		{
			CombineInstance item = default(CombineInstance);
			item.mesh = meshes[i];
			if (t != null)
			{
				item.transform = t.worldToLocalMatrix;
			}
			list.Add(item);
		}
		Mesh mesh = new Mesh();
		mesh.CombineMeshes(list.ToArray(), mergeSubMeshes, t != null);
		return mesh;
	}
}
