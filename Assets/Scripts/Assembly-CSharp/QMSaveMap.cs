using System;

public class QMSaveMap : qmUI
{
	public QuickInputField inputField;

	public override void Activate()
	{
		base.Activate();
		inputField.Activate();
	}

	protected override void Awake()
	{
		base.Awake();
		QuickInputField.OnEnter = (Action)Delegate.Combine(QuickInputField.OnEnter, new Action(OnEnter));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		QuickInputField.OnEnter = (Action)Delegate.Remove(QuickInputField.OnEnter, new Action(OnEnter));
	}

	private void OnEnter()
	{
		if (inputField.text.text.Length != 0)
		{
			Quickmap.customMapName = inputField.text.text;
			QuickmapScene.instance.SaveCurrentMap();
			Back();
		}
	}
}
