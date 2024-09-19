using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneData", menuName = "Scenes/Scene Data")]
public class SceneData : ScriptableObject
{
	public enum SceneType
	{
		Indefinite = 0,
		Hub = 1,
		Tutorial = 2,
		Tower = 3,
		Arena = 4,
		Cutscene = 5
	}

	public SceneReference sceneReference;

	public HubData area;

	[TextArea]
	public string publicName;

	public bool testLevel = true;

	public SceneType sceneType;

	public LevelResults results;

	public SceneData[] lockedBy;

	public string sceneName { get; private set; }

	public bool IsPlayable()
	{
		if (sceneType != SceneType.Hub && sceneType != SceneType.Tutorial && sceneType != SceneType.Tower)
		{
			return sceneType == SceneType.Arena;
		}
		return true;
	}

	public bool IsPlayebleNotHub()
	{
		if (sceneType != SceneType.Tutorial && sceneType != SceneType.Tower)
		{
			return sceneType == SceneType.Arena;
		}
		return true;
	}

	public bool IsRankable()
	{
		if (sceneType != SceneType.Tower)
		{
			return sceneType == SceneType.Arena;
		}
		return true;
	}

	public void RefreshSceneName()
	{
		if (sceneReference != null)
		{
			sceneName = Path.GetFileNameWithoutExtension(sceneReference.ScenePath);
		}
		else
		{
			sceneName = Game.fallbackScene;
		}
	}

	public SceneData(Scene scene)
	{
		sceneReference = new SceneReference();
		sceneReference.ScenePath = scene.path;
		sceneName = Path.GetFileNameWithoutExtension(sceneReference.ScenePath);
	}

	public string GetLevelPublicName()
	{
		if (publicName.Length <= 0)
		{
			return sceneName;
		}
		return publicName;
	}

	public void OnValidate()
	{
	}
}
