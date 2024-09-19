using System;
using System.Collections.Generic;
using UnityEngine;

public class DoorAlarm : MonoBehaviour
{
	[HideInInspector]
	public Door door;

	[HideInInspector]
	public List<BaseEnemy> enemies = new List<BaseEnemy>();

	private void Awake()
	{
		GetComponent<MeshRenderer>().enabled = false;
	}

	private void OnEnable()
	{
		Door obj = door;
		obj.OnOpening = (Action)Delegate.Combine(obj.OnOpening, new Action(OnOpening));
	}

	private void OnDisable()
	{
		Door obj = door;
		obj.OnOpening = (Action)Delegate.Remove(obj.OnOpening, new Action(OnOpening));
	}

	private void OnOpening()
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].SetTarget(PlayerController.instance.t);
		}
	}
}
