using UnityEngine;

public class ArenaBonfire : MonoBehaviour
{
	public GameObject fire;

	public Transform t { get; private set; }

	public void Setup()
	{
		t = base.transform;
		fire.SetActive(value: false);
	}

	public void Reset()
	{
		fire.SetActive(value: false);
	}

	public void Activate()
	{
		fire.SetActive(value: true);
	}
}
