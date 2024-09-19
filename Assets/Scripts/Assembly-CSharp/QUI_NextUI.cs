public class QUI_NextUI : QuickMenuItem
{
	public QuickUI nextUI;

	private QuickUI myUI;

	public override void Awake()
	{
		base.Awake();
		myUI = GetComponentInParent<QuickUI>();
	}

	public override bool Accept()
	{
		nextUI.Activate();
		myUI.Deactivate();
		return true;
	}
}
