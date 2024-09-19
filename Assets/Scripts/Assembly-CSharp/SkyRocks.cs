using UnityEngine;

public class SkyRocks : MonoBehaviour
{
	public GameObject[] prefabs;

	public int prefabCount = 20;

	public Vector2 dist = new Vector2(10f, 15f);

	public Vector2 MinMaxScale = new Vector2(0.3f, 1f);

	private Transform[] transforms;

	private void Awake()
	{
		transforms = GetComponentsInChildren<Transform>();
	}

	private void Update()
	{
		transforms[0].Rotate(0f, Time.deltaTime, 0f);
		for (int i = 1; i < transforms.Length; i++)
		{
			transforms[i].Rotate(Vector3.one * ((float)(i + 1) * Time.deltaTime));
		}
	}
}
