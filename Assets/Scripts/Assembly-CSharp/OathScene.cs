using System;
using UnityEngine;

public class OathScene : MonoBehaviour
{
	public static OathScene instance;

	[TextArea]
	public string[] theOath;

	public int index;

	public GameObject objBoat;

	public GameObject objPrompt;

	public Transform tBoat;

	private Vector3 boatPos;

	private Vector3 startBoatPos;

	private void Awake()
	{
		instance = this;
		startBoatPos = tBoat.position;
		objBoat.SetActive(value: false);
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		index = 0;
		objBoat.SetActive(value: false);
		objPrompt.SetActive(value: true);
	}

	private void Update()
	{
		boatPos = startBoatPos;
		boatPos.y += Mathf.Sin(Time.time / 2f) * 0.5f;
		tBoat.position = boatPos;
		tBoat.localEulerAngles = new Vector3(Mathf.Sin(Time.time * 2f) * 5f, 0f, Mathf.Sin(Time.time) * 5f);
	}

	public bool GetNextLine(ref string result)
	{
		if (index < theOath.Length)
		{
			result = theOath[index];
			index++;
			if (index == theOath.Length)
			{
				objBoat.SetActive(value: true);
				objPrompt.SetActive(value: false);
			}
			return true;
		}
		return false;
	}
}
