using UnityEngine;

public class PerlinShake : MonoBehaviour
{
	public PerlinShakePreset[] presets;

	public bool play;

	public bool unscaled;

	private int count = 12;

	private Transform t;

	private Vector3 temp;

	private PerlinShakeEntry[] shakes;

	public Vector3 finalShake { get; private set; }

	private void Awake()
	{
		t = base.transform;
		shakes = new PerlinShakeEntry[count];
		for (int i = 0; i < count; i++)
		{
			shakes[i] = new PerlinShakeEntry();
		}
	}

	public void Reset()
	{
		for (int i = 0; i < count; i++)
		{
			shakes[i].Reset();
		}
		temp.x = (temp.y = (temp.z = 0f));
		finalShake = temp;
	}

	public void Shake(int index = 0)
	{
		for (int i = 0; i < shakes.Length; i++)
		{
			if (shakes[i].time == 0f)
			{
				shakes[i].Setup(presets[index]);
				break;
			}
		}
	}

	private void Update()
	{
		temp.x = (temp.y = (temp.z = 0f));
		for (int i = 0; i < count; i++)
		{
			if (shakes[i].time != 0f)
			{
				temp += shakes[i].GetShake(unscaled);
			}
		}
		if (finalShake != temp)
		{
			finalShake = temp;
			if (play)
			{
				t.localEulerAngles = finalShake;
			}
		}
	}
}
