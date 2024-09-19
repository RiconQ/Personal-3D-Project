using UnityEngine;

public class GameTitleB : MonoBehaviour
{
	public Transform t;

	public Transform[] tKNIGHT;

	public MeshRenderer[] rends;

	public Vector3 aPos = new Vector3(0f, 0f, 1f);

	public Vector3 bPos = new Vector3(0f, 0f, 2f);

	public string text = "Press E to Continue";

	private float timer;

	private MaterialPropertyBlock block;

	private void Awake()
	{
		Setup();
	}

	public void Setup()
	{
		t = base.transform;
		rends = GetComponentsInChildren<MeshRenderer>();
		block = new MaterialPropertyBlock();
		rends[0].GetPropertyBlock(block);
		OnEnable();
	}

	private void OnEnable()
	{
		t.position = aPos;
		t.localEulerAngles = new Vector3(80f, 180f, 0f);
		timer = -1f;
		block.SetFloat("_Alpha", -1f);
		MeshRenderer[] array = rends;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(block);
		}
		for (int j = 0; j < tKNIGHT.Length; j++)
		{
			tKNIGHT[j].localEulerAngles = new Vector3(0f, 0f, 90f);
		}
	}

	private void Update()
	{
		t.position = Vector3.Lerp(t.position, bPos, Time.deltaTime * 0.5f);
		t.rotation = Quaternion.Slerp(t.rotation, Quaternion.Euler(90f, 180f, 0f), Time.deltaTime * 1f);
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
			block.SetFloat("_Alpha", timer);
			if (timer == 1f && text.Length > 0)
			{
				Game.message.Show(text);
			}
		}
		MeshRenderer[] array = rends;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(block);
		}
		for (int j = 0; j < tKNIGHT.Length; j++)
		{
			tKNIGHT[j].localRotation = Quaternion.Slerp(tKNIGHT[j].localRotation, Quaternion.identity, Time.deltaTime * 0.5f);
		}
	}
}
