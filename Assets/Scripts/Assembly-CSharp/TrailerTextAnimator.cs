using UnityEngine;
using UnityEngine.UI;

public class TrailerTextAnimator : MonoBehaviour
{
	public Transform t;

	public Transform tCam;

	public TextAnimator animator;

	public Text txt;

	public GameObject logo;

	private int i;

	private float shakeMgt;

	private Vector3 shake;

	public TrailerTextEntry[] entries;

	private void Awake()
	{
		i = 0;
		Show();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Game.instance.LoadLevel("MainMenu");
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			i = 0;
			Show();
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			i = 1;
			Show();
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			i = 2;
			Show();
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			i = 3;
			Show();
		}
		t.position = Vector3.Lerp(t.position, new Vector3(0f, 0f, -4.5f), Time.deltaTime);
		if (shakeMgt != 0f)
		{
			shakeMgt = Mathf.MoveTowards(shakeMgt, 0f, Time.deltaTime * 2f);
			shake.x = Random.Range(0f - shakeMgt, shakeMgt);
			shake.y = Random.Range(0f - shakeMgt, shakeMgt);
			shake.z = Random.Range(0f - shakeMgt, shakeMgt);
			if (i < 3)
			{
				entries[i].tSprite.transform.position += shake / 20f;
			}
		}
	}

	private void Show()
	{
		for (int i = 0; i < entries.Length; i++)
		{
			entries[i].tSprite.gameObject.SetActive(value: false);
		}
		t.position = new Vector3(0f, 0f, -1f);
		if (this.i < 3)
		{
			logo.SetActive(value: false);
			txt.gameObject.SetActive(value: true);
			txt.text = entries[this.i].text;
			animator.ResetAndPlay();
			entries[this.i].tSprite.gameObject.SetActive(value: true);
			shakeMgt = 1f;
		}
		else
		{
			txt.gameObject.SetActive(value: false);
			logo.SetActive(value: true);
		}
	}
}
