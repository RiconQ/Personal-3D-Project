public class QuickmapNewLevelUI : QuickMenuItem
{
	public QuickInputField inputField;

	private QuickMenu ui;

	public override void Awake()
	{
		base.Awake();
		ui = GetComponentInParent<QuickMenu>();
	}

	public override void Select()
	{
		base.Select();
		inputField.Reset();
	}

	public override bool Accept()
	{
		ui.locked = true;
		inputField.Activate();
		return base.Accept();
	}
}
