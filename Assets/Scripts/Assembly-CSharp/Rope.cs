using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
	public struct RopeSegment
	{
		public Vector2 posNow;

		public Vector2 posOld;

		public RopeSegment(Vector2 pos)
		{
			posNow = pos;
			posOld = pos;
		}
	}

	private LineRenderer lineRenderer;

	private List<RopeSegment> ropeSegments = new List<RopeSegment>();

	private float ropeSegLen = 0.25f;

	private int segmentLength = 35;

	private float lineWidth = 0.1f;

	public int count = 50;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		for (int i = 0; i < segmentLength; i++)
		{
			ropeSegments.Add(new RopeSegment(vector));
			vector.y -= ropeSegLen;
		}
	}

	private void Update()
	{
		DrawRope();
	}

	private void FixedUpdate()
	{
		Simulate();
	}

	private void Simulate()
	{
		new Vector2(0f, -1.5f);
		for (int i = 1; i < segmentLength; i++)
		{
			RopeSegment value = ropeSegments[i];
			Vector2 vector = value.posNow - value.posOld;
			value.posOld = value.posNow;
			value.posNow += vector;
			ropeSegments[i] = value;
		}
		RopeSegment value2 = ropeSegments[0];
		value2.posNow = base.transform.position;
		ropeSegments[0] = value2;
		for (int j = 0; j < count; j++)
		{
			ApplyConstraint();
		}
	}

	private void ApplyConstraint()
	{
		for (int i = 0; i < segmentLength - 1; i++)
		{
			RopeSegment value = ropeSegments[i];
			RopeSegment value2 = ropeSegments[i + 1];
			float magnitude = (value.posNow - value2.posNow).magnitude;
			float num = Mathf.Abs(magnitude - ropeSegLen);
			Vector2 vector = Vector2.zero;
			if (magnitude > ropeSegLen)
			{
				vector = (value.posNow - value2.posNow).normalized;
			}
			else if (magnitude < ropeSegLen)
			{
				vector = (value2.posNow - value.posNow).normalized;
			}
			Vector2 vector2 = vector * num;
			if (i != 0)
			{
				value.posNow -= vector2 * 0.5f;
				ropeSegments[i] = value;
				value2.posNow += vector2 * 0.5f;
				ropeSegments[i + 1] = value2;
			}
			else
			{
				value2.posNow += vector2;
				ropeSegments[i + 1] = value2;
			}
		}
	}

	private void DrawRope()
	{
		float num = lineWidth;
		lineRenderer.startWidth = num;
		lineRenderer.endWidth = num;
		Vector3[] array = new Vector3[segmentLength];
		for (int i = 0; i < segmentLength; i++)
		{
			array[i] = ropeSegments[i].posNow;
		}
		lineRenderer.positionCount = array.Length;
		lineRenderer.SetPositions(array);
	}
}
