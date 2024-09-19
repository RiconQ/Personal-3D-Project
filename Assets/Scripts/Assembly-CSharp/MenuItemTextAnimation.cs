using System;
using UnityEngine;

[RequireComponent(typeof(QuickMenuItem))]
public class MenuItemTextAnimation : MonoBehaviour
{
	private QuickMenuItem item;

	private TextAnimator animator;

	private void Start()
	{
		item = GetComponent<QuickMenuItem>();
		animator = GetComponentInChildren<TextAnimator>();
		animator.ResetChars();
		QuickMenuItem quickMenuItem = item;
		quickMenuItem.OnSelect = (Action)Delegate.Combine(quickMenuItem.OnSelect, new Action(Play));
	}

	private void Play()
	{
		animator.Play();
	}
}
