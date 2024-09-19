using UnityEngine;
using UnityEngine.Rendering;

public class SkyRocksRenderer : MonoBehaviour
{
	private struct MeshProperties
	{
		public Matrix4x4 matrix;

		public Vector4 color;

		public static int Size()
		{
			return 80;
		}
	}

	public int population;

	public float range;

	public Transform t;

	public Material material;

	private ComputeBuffer meshPropertiesBuffer;

	private ComputeBuffer argsBuffer;

	private Bounds bounds;

	public Mesh mesh;

	private void Setup()
	{
		t = base.transform;
		bounds = new Bounds(t.position, Vector3.one * (range + 100f));
		InitializeBuffers();
	}

	private void InitializeBuffers()
	{
		uint[] array = new uint[5]
		{
			mesh.GetIndexCount(0),
			(uint)population,
			mesh.GetIndexStart(0),
			mesh.GetBaseVertex(0),
			0u
		};
		argsBuffer = new ComputeBuffer(1, array.Length * 4, ComputeBufferType.IndirectArguments);
		argsBuffer.SetData(array);
		Vector3 vector = t.forward;
		MeshProperties[] array2 = new MeshProperties[population];
		for (int i = 0; i < population; i++)
		{
			MeshProperties meshProperties = default(MeshProperties);
			vector = Quaternion.Euler(0f, 360f / (float)population, 0f) * vector;
			Vector3 pos = vector * Random.Range(range / 2f, range);
			pos.x += Random.Range(-2f, 2f);
			pos.y += Random.Range(-2f, 2f);
			pos.z += Random.Range(-2f, 2f);
			Quaternion q = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
			Vector3 s = Vector3.one * (0.1f + Mathf.PerlinNoise(pos.x, pos.z) * 0.6f);
			meshProperties.matrix = Matrix4x4.TRS(pos, q, s);
			meshProperties.color = Color.Lerp(Color.red, Color.blue, Random.value);
			array2[i] = meshProperties;
		}
		meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
		meshPropertiesBuffer.SetData(array2);
		material.SetBuffer("_Properties", meshPropertiesBuffer);
	}

	private void Start()
	{
		Setup();
	}

	private void Update()
	{
		Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer, 0, null, ShadowCastingMode.On, receiveShadows: true, 4);
	}

	private void OnDisable()
	{
		if (meshPropertiesBuffer != null)
		{
			meshPropertiesBuffer.Release();
		}
		meshPropertiesBuffer = null;
		if (argsBuffer != null)
		{
			argsBuffer.Release();
		}
		argsBuffer = null;
	}
}
