using System;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmDialog : qmUI
{
	public UnityEvent yesEvent;

	public MyButton yesBtn;

	public MyButton noBtn;

	public Text text;

	protected override void Awake()
	{
		base.Awake();
		MyButton myButton = yesBtn;
		myButton.OnClick = (Action)Delegate.Combine(myButton.OnClick, new Action(Yes));
		MyButton myButton2 = noBtn;
		myButton2.OnClick = (Action)Delegate.Combine(myButton2.OnClick, new Action(No));
	}

	private void Yes()
	{
		yesEvent.Invoke();
		Back();
	}

	private void No()
	{
		Back();
	}
}
