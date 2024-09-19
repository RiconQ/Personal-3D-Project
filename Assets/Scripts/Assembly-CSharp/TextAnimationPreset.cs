using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TextAnimationPreset : ScriptableObject
{
	public float speed = 1f;

	public float perCharDelay = 0.1f;

	public float perWordDelay = 0.3f;

	public float perLineDelay = 1f;

	public List<TextAnimation> animations = new List<TextAnimation>();
}
