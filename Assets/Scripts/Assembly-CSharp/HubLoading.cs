using System.Collections;
using UnityEngine;

public class HubLoading : MonoBehaviour
{
	public static SceneData LevelToLoad;

	public ParticleSystem particle;

	private void Awake()
	{
		Game.fading.InstantFade(0f);
		StartCoroutine(Waiting());
	}

	private IEnumerator Waiting()
	{
		while (particle.isPlaying)
		{
			yield return null;
		}
		Game.fading.InstantFade(1f);
		Game.instance.LoadLevel(LevelToLoad.sceneName);
	}
}
