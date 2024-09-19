using UnityEngine;

public class CutsceneLevel : MonoBehaviour
{
	public static CutsceneLevel instance;

	public SceneData LevelToLoad;

	private void Awake()
	{
		instance = this;
	}

	public virtual void Skip()
	{
		Game.fading.speed = 4f;
		Game.instance.LoadLevel(LevelToLoad.sceneName);
	}
}
