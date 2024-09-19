using System;
using UnityEngine;

[Serializable]
public class TextAnimation
{
	public enum AnimType
	{
		Color = 0,
		Position = 1,
		Rotation = 2,
		Scale = 3,
		Shake = 4
	}

	public AnimType type;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Color aColor = Color.clear;

	public Color bColor = Color.white;

	public Vector2 aVector = new Vector2(0f, -32f);

	public Vector2 bVector = new Vector2(0f, 0f);

	public float aFloat;

	public float bFloat;

	private float progress;

	private Vector3 tempVector;

	public virtual void UpdateAnimation(ref UIVertex uiVertex, ref CharAnimation charAnim, Vector3 center)
	{
		progress = curve.Evaluate(charAnim.progress);
		switch (type)
		{
		case AnimType.Color:
			uiVertex.color = Color.LerpUnclamped(aColor, bColor, progress);
			break;
		case AnimType.Position:
			uiVertex.position.x += Mathf.LerpUnclamped(aVector.x, bVector.x, progress);
			uiVertex.position.y += Mathf.LerpUnclamped(aVector.y, bVector.y, progress);
			break;
		case AnimType.Rotation:
			tempVector = uiVertex.position - center;
			tempVector = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(aFloat, bFloat, progress)) * tempVector;
			uiVertex.position = tempVector + center;
			break;
		case AnimType.Scale:
			tempVector = (center - uiVertex.position).normalized;
			uiVertex.position += tempVector * Mathf.LerpUnclamped(aFloat, bFloat, progress);
			break;
		case AnimType.Shake:
			uiVertex.position.x += (Mathf.PerlinNoise(center.y + center.x, Time.time) - 0.5f) * Mathf.LerpUnclamped(aFloat, bFloat, progress);
			uiVertex.position.y += (Mathf.PerlinNoise(Time.time, center.x - center.y) - 0.5f) * Mathf.LerpUnclamped(aFloat, bFloat, progress);
			break;
		}
	}

	public virtual void Reset(ref CharAnimation charAnim)
	{
		charAnim.progress = 0f;
		charAnim.time = 0f;
	}
}
