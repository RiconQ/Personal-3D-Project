public class MenuItem_Language : QuickMenuOptionItem
{
	public override void Awake()
	{
		values.Clear();
		for (int i = 0; i < Game.localization.languages.Length; i++)
		{
			values.Add(Game.localization.languages[i].strings[0]);
		}
		base.Awake();
		index = Game.gamePrefs.GetValue("Language");
	}

	public override bool Accept()
	{
		base.Accept();
		Game.gamePrefs.UpdateValue("Language", index);
		if (Game.localization.ResetCurrent())
		{
			Game.instance.Restart();
		}
		return true;
	}
}
