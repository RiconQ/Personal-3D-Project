using UnityEngine;

public class SwayEntry
{
	public float x;

	public float y;

	public float z;

	public float speed;

	public float time;

	public Vector3 result;

	public SwayEntry()
	{
		result = default(Vector3);
	}

	public void Reset()
	{
		if (time != 0f)
		{
			time = 0f;
		}
	}

	public void Setup(float x, float y, float z, float speed)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.speed = speed;
		time = 1f;
	}

	public Vector3 GetSway(AnimationCurve curve)
	{
		result.x = Mathf.LerpUnclamped(0f, x, curve.Evaluate(1f - time));
		result.y = Mathf.LerpUnclamped(0f, y, curve.Evaluate(1f - time));
		result.z = Mathf.LerpUnclamped(0f, z, curve.Evaluate(1f - time));
		time = Mathf.MoveTowards(time, 0f, Time.deltaTime * speed);
		return result;
	}
}
