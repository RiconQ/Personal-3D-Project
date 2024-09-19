using System;
using UnityEngine;

[Serializable]
public class SkullSpeech
{
	public enum Mood
	{
		None = 0,
		Positive = 1,
		Negative = 2,
		Surprised = 3
	}

	[TextArea]
	public string message;

	public Vector3 pos;

	public Vector3 lookAtPos;

	public bool triggerEvent;

	public Mood mood;
}
