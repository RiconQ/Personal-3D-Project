using UnityEngine;

public class Tutorial_WorldLabel : MonoBehaviour
{
	private TextMesh text;

	private Color color;

	private float timer;

	private void Awake()
	{
		text = GetComponent<TextMesh>();
		color = Color.clear;
		text.color = color;
	}

	private void Update()
	{
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 4f);
		}
		text.color = Color.Lerp(text.color, color, timer);
	}

	private void OnTriggerEnter(Collider other)
	{
		timer = 0f;
		color = Color.white * 0.5f;
	}

	private void OnTriggerExit(Collider other)
	{
		timer = 0f;
		color = Color.clear;
	}
}
