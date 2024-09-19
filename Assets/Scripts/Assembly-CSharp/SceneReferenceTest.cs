using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReferenceTest : MonoBehaviour
{
	public SceneReference exampleNull;

	public SceneReference exampleMissing;

	public SceneReference exampleDisabled;

	public SceneReference exampleEnabled;

	private void OnGUI()
	{
		DisplayLevel(exampleNull);
		DisplayLevel(exampleMissing);
		DisplayLevel(exampleDisabled);
		DisplayLevel(exampleEnabled);
	}

	public void DisplayLevel(SceneReference scene)
	{
		GUILayout.Label(new GUIContent("Scene name Path: " + scene));
		if (GUILayout.Button("Load " + scene))
		{
			SceneManager.LoadScene(scene);
		}
	}
}
