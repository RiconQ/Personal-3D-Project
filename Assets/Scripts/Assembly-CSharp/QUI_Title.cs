using System;

public class QUI_Title : QuickUI
{
	public Action<bool> OnActivate = delegate
	{
	};

	public QuickUI nextUI;

	public override void Awake()
	{
		base.Awake();
		InputsManager.OnAccept = (Action)Delegate.Combine(InputsManager.OnAccept, new Action(Accept));
	}

	public override void OnDisable()
	{
		base.OnDisable();
		InputsManager.OnAccept = (Action)Delegate.Remove(InputsManager.OnAccept, new Action(Accept));
	}

	public override void Activate()
	{
		base.Activate();
		if (OnActivate != null)
		{
			OnActivate(obj: true);
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (OnActivate != null)
		{
			OnActivate(obj: false);
		}
	}

	public virtual void Accept()
	{
		if (base.active)
		{
			if ((bool)source && (bool)sounds)
			{
				source.PlayClip(sounds.accept);
			}
			nextUI.Activate();
			Deactivate();
		}
	}
}
