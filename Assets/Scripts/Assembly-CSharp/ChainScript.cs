using UnityEngine;

[ExecuteInEditMode]
public class ChainScript : MonoBehaviour
{
	private LineRenderer line;

	[SerializeField]
	private Transform[] tTarget;

	private void Awake()
	{
		line = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		line.SetPosition(0, base.transform.position);
		for (int i = 0; i < tTarget.Length; i++)
		{
			line.SetPosition(i + 1, tTarget[i].position);
		}
	}
}
