using UnityEngine;

public class HeadPosition : MonoBehaviour
{
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve bounceCurve = AnimationCurve.Linear(0f, 0f, 0f, 0f);

	public float speed = 4f;

	public float bounceSpeed = 2f;

	private Transform t;

	private Vector3 pos;

	private float yCurrent;

	private float yTarget;

	private float yBounce;

	private float maxBounce;

	private float baseTimer;

	private float bounceTimer;

	private void Awake()
	{
		t = base.transform;
		ChangeYPosition(t.localPosition.y);
	}

	public void Reset()
	{
		bounceTimer = (baseTimer = 1f);
		t.localPosition = new Vector3(0f, 0.75f, 0f);
	}

	public void ChangeYPosition(float y)
	{
		baseTimer = 0f;
		yCurrent = t.localPosition.y;
		yTarget = y;
	}

	public void Bounce(float value = -0.25f)
	{
		if (bounceTimer == 1f)
		{
			bounceTimer = 0f;
			yBounce = 0f;
			maxBounce = Mathf.Clamp(value, -0.5f, -0.15f);
		}
	}

	public void Tick()
	{
		baseTimer = Mathf.MoveTowards(baseTimer, 1f, Time.deltaTime * speed);
		pos.y = Mathf.LerpUnclamped(yCurrent, yTarget, curve.Evaluate(baseTimer));
		bounceTimer = Mathf.MoveTowards(bounceTimer, 1f, Time.deltaTime * bounceSpeed);
		yBounce = Mathf.LerpUnclamped(0f, maxBounce, bounceCurve.Evaluate(bounceTimer)) / (1f + (0.75f - pos.y));
		pos.y = Mathf.Clamp(pos.y + yBounce, -0.75f, float.PositiveInfinity);
		t.localPosition = pos;
	}
}
