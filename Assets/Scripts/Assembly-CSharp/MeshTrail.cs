using UnityEngine;

public class MeshTrail : MonoBehaviour
{
	public Mesh mesh;

	public Material blurredMaterial;

	public Transform start;

	public Transform end;

	[Range(1f, 30f)]
	public int numIntermediates = 30;

	private Matrix4x4[] intermediatePositions = new Matrix4x4[30];

	private Vector3[] poses = new Vector3[30];

	private MeshTrailElement[] elements = new MeshTrailElement[30];

	private int index;

	private void Start()
	{
		for (int i = 0; i < intermediatePositions.Length; i++)
		{
			elements[i] = new MeshTrailElement();
		}
	}

	private void Update()
	{
		if (numIntermediates != intermediatePositions.Length)
		{
			intermediatePositions = new Matrix4x4[numIntermediates];
		}
		for (int i = 0; i < intermediatePositions.Length; i++)
		{
			if (i == index)
			{
				elements[i].pos = start.position;
				elements[i].rot = start.rotation;
				elements[i].scale = start.lossyScale;
			}
			else
			{
				elements[i].pos.y += Time.deltaTime;
				elements[i].scale = Vector3.Lerp(elements[i].scale, Vector3.zero, Time.deltaTime * 2f);
			}
			intermediatePositions[i] = Matrix4x4.TRS(elements[i].pos, elements[i].rot, elements[i].scale);
		}
		index = index.Next(numIntermediates);
		blurredMaterial.SetInt("_numSamples", numIntermediates);
		Graphics.DrawMeshInstanced(mesh, 0, blurredMaterial, intermediatePositions);
	}
}
