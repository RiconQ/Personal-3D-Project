using UnityEngine;

public class MeshPerlinColors : MonoBehaviour
{
	public Material mat;

	private void Awake()
	{
		UpdateColors();
	}

	public void UpdateColors()
	{
		MeshFilter[] array = Object.FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
		Vector3 vector = default(Vector3);
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i].GetComponent<MeshRenderer>().sharedMaterial != mat))
			{
				Mesh mesh = new Mesh();
				mesh.SetVertices(array[i].sharedMesh.vertices);
				Color[] array2 = new Color[mesh.vertices.Length];
				for (int j = 0; j < mesh.vertices.Length; j++)
				{
					vector = array[i].transform.TransformPoint(mesh.vertices[j]);
					array2[j] = Color.Lerp(Color.red, Color.black, Mathf.PerlinNoise((vector.x + vector.y) / 12f, (vector.x + vector.z) / 12f));
				}
				mesh.SetColors(array2);
				mesh.UploadMeshData(markNoLongerReadable: true);
				array[i].GetComponent<MeshRenderer>().additionalVertexStreams = mesh;
			}
		}
	}
}
