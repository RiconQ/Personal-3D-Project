using UnityEngine;

public class LoadLevelTrigger : MonoBehaviour
{
	public SceneData level;

	private void OnTriggerEnter(Collider other)
	{
		GetComponent<Collider>().enabled = false;
		Game.fading.InstantFade(1f);
		Game.instance.LoadLevel(level.sceneReference.ScenePath);
	}
}
