public class QUI_LoadSceneData : QuickMenuItem
{
	public SceneData level;

	private QuickMenu menu;

	public override void Awake()
	{
		base.Awake();
		menu = GetComponentInParent<QuickMenu>();
	}

	public override bool Accept()
	{
		base.Accept();
		Game.instance.LoadLevel(level.sceneReference.ScenePath);
		menu.locked = true;
		return true;
	}
}
