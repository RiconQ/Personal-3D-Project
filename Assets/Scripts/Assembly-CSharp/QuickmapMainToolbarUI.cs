public class QuickmapMainToolbarUI : qmUI
{
	public override void Activate()
	{
		if ((bool)QuickmapScene.instance)
		{
			QuickmapScene.instance.editorCamera.enabled = true;
		}
	}

	public override void Deactivate()
	{
		if ((bool)QuickmapScene.instance)
		{
			QuickmapScene.instance.editorCamera.enabled = false;
		}
	}
}
