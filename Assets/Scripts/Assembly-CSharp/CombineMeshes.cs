using UnityEngine;

public class CombineMeshes : MonoBehaviour
{
	[Button]
	public void CombineA()
	{
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] array = new CombineInstance[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject.name == "5")
			{
				array[i].mesh = componentsInChildren[i].sharedMesh;
				array[i].transform = componentsInChildren[i].transform.localToWorldMatrix;
			}
		}
		Mesh mesh = new Mesh();
		mesh.CombineMeshes(array);
		mesh.Optimize();
		mesh.RecalculateNormals();
		mesh.Weld(0.1f, 0.1f);
		base.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
	}

	[Button]
	public void Combine()
	{
		MeshCollider[] componentsInChildren = GetComponentsInChildren<MeshCollider>();
		CombineInstance[] array = new CombineInstance[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			array[i].mesh = componentsInChildren[i].sharedMesh;
			array[i].transform = componentsInChildren[i].transform.localToWorldMatrix;
			componentsInChildren[i].enabled = false;
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
		Mesh mesh = new Mesh();
		mesh.CombineMeshes(array);
		mesh.Optimize();
		mesh.Weld(0.1f, 0.1f);
		mesh.Simplify();
		base.transform.gameObject.SetActive(value: true);
		base.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
	}
}
