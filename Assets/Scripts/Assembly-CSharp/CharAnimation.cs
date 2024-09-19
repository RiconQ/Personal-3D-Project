using System;

[Serializable]
public class CharAnimation
{
	public float time;

	public float progress;

	public void Reset(float progress = 0f)
	{
		time = ((progress > 0f) ? 99999 : 0);
		this.progress = progress;
	}
}
