public class QUI_GameMethods : QuickMenuItem
{
	public enum GameMethods
	{
		LoadCurrent = 0,
		LoadNext = 1
	}

	public GameMethods methods;

	public override bool Accept()
	{
		base.Accept();
		GetComponentInParent<QuickMenu>().locked = true;
		if (QuickMissionsMenu.missionNames.Length != 0)
		{
			switch (methods)
			{
			case GameMethods.LoadCurrent:
				Game.instance.LoadLevel(QuickMissionsMenu.CurrentMissionName());
				break;
			case GameMethods.LoadNext:
				QuickMissionsMenu.NextMission();
				if (QuickMissionsMenu.current != 0)
				{
					Game.instance.LoadLevel(QuickMissionsMenu.CurrentMissionName());
				}
				else
				{
					Game.instance.LoadLevel("MainMenu");
				}
				break;
			}
		}
		else
		{
			Game.instance.LoadLevel("MainMenu");
		}
		return true;
	}
}
