using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Camera Trigger Preset")]
public class CameraTriggerPreset : ScriptableObject
{
	[Header("Focus")]
	public float FocusDuration;

	public float FocusFOV = 40f;

	public AnimationCurve FocusCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 8f), new Keyframe(0.2f, 1f, 0f, 0f), new Keyframe(0.8f, 1f, 0f, 0f), new Keyframe(1f, 0f, -8f, 0f));

	[Header("Closeup")]
	public float Duration = 2.5f;

	public float TimeScale = 0.2f;

	public float LookSpeed = 4f;

	public float FOV = 60f;

	public AnimationCurve Curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 8f), new Keyframe(0.2f, 1f, 0f, 0f), new Keyframe(0.8f, 1f, 0f, 0f), new Keyframe(1f, 0f, -8f, 0f));

	public AnimationCurve FOVCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 8f), new Keyframe(0.2f, 1f, 0f, 0f), new Keyframe(0.8f, 1f, 0f, 0f), new Keyframe(1f, 0f, -8f, 0f));
}
