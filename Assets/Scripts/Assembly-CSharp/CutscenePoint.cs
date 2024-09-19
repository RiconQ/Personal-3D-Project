using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CutscenePoint
{
	public Transform t;

	[TextArea]
	public string text;

	public UnityEvent Event;
}
