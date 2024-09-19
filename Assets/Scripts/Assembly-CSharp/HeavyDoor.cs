using UnityEngine;

public class HeavyDoor : Door
{
	private Transform t;

	private Vector3 pos;

	private Vector3 startPos;

	public float yOffset = 5f;

	private float timer;

	private int state = -1;

	private CanvasGroup[] cgs;

	private void Awake()
	{
		t = base.transform;
		startPos = t.position;
		pos = t.position;
		pos.y += yOffset;
	}

	public override void SetupDeathLock(int count)
	{
		base.SetupDeathLock(count);
		cgs = GetComponentsInChildren<CanvasGroup>();
		for (int i = 0; i < cgs.Length; i++)
		{
			if (i < count)
			{
				cgs[i].gameObject.SetActive(value: true);
			}
			else
			{
				cgs[i].gameObject.SetActive(value: false);
			}
		}
	}

	public override void UpdateDeathLock(int index)
	{
		base.UpdateDeathLock(index);
		for (int i = 0; i < cgs.Length; i++)
		{
			cgs[i].alpha = ((i < index) ? 0.1f : 1f);
		}
	}

	public override void Open()
	{
		if (state == -1)
		{
			state = 0;
			if (OnOpening != null)
			{
				OnOpening();
			}
			GetComponentInChildren<AudioSource>().Play();
			base.Open();
		}
	}

	private void Update()
	{
		if (state != -1)
		{
			if (state != 0)
			{
				_ = 1;
				return;
			}
			if (t.position != pos)
			{
				t.position = Vector3.Lerp(t.position, pos, Time.deltaTime * 8f);
				return;
			}
			timer = 0f;
			state++;
		}
	}
}
