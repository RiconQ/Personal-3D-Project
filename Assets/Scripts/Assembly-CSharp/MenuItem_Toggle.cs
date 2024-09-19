public class MenuItem_Toggle : QuickMenuOptionItem
{
	public override void Awake()
	{
		values.Clear();
		values.Add("OFF");
		values.Add("ON");
		base.Awake();
	}

	public override bool Accept()
	{
		base.Next((index <= 0) ? 1 : (-1));
		return true;
	}

	public override void Next(int sign)
	{
	}
}
