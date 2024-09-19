using UnityEngine;

[CreateAssetMenu(fileName = "New Shake Preset", menuName = "Shake Preset", order = 1)]
public class PerlinShakePreset : ScriptableObject
{
	public float speed = 10f;

	public float amplitude = 0.1f;

	public float duration = 4f;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
}
