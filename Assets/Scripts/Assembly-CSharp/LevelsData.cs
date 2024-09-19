using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelsData : MonoBehaviour
{
	public static LevelsData instance;

	public static SceneData currentPlayableLevel;

	public static SceneData lastPlayableLevel;

	public static HubData currentHub;

	public SceneData[] levels;

	public HubData[] hubs;

	public GameObject MusicPlayer;

	public GameObject AmbientMusicPlayer;

	public Dictionary<string, LevelResults> missions = new Dictionary<string, LevelResults>(100);

	public Dictionary<HubData, int> hubsProgress = new Dictionary<HubData, int>(10);

	private LevelResults tempResults;

	private void Awake()
	{
		instance = this;
		levels = Resources.LoadAll<SceneData>("Levels");
		hubs = Resources.LoadAll<HubData>("Levels/Hubs");
		SceneData[] array = levels;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RefreshSceneName();
		}
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Combine(Game.OnPlayableLevelLoaded, new Action<string>(SetActiveLevel));
	}

	private void OnDestroy()
	{
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Remove(Game.OnPlayableLevelLoaded, new Action<string>(SetActiveLevel));
	}

	public void SetActiveLevel(string name)
	{
		currentPlayableLevel = GetLevelByName(name);
		currentHub = GetCurrentHub();
		if ((bool)currentHub)
		{
			if ((bool)currentHub.MusicSet && currentPlayableLevel.sceneType > SceneData.SceneType.Hub)
			{
				UnityEngine.Object.Instantiate(MusicPlayer).GetComponent<DynamicPlayer>().PlayMusic(currentHub.MusicSet);
			}
			else if ((bool)currentHub.AmbientMusic)
			{
				UnityEngine.Object.Instantiate(AmbientMusicPlayer).GetComponent<BackgroundMusic>().Setup(currentHub.AmbientMusic);
			}
		}
	}

	public bool UnlockCurrentHub()
	{
		if (!currentHub)
		{
			return false;
		}
		if (!hubsProgress.ContainsKey(currentHub))
		{
			hubsProgress.Add(currentHub, 1);
			return true;
		}
		if (hubsProgress[currentHub] == 0)
		{
			hubsProgress[currentHub] = 1;
			return true;
		}
		return false;
	}

	public HubData GetCurrentHub()
	{
		if (!currentPlayableLevel)
		{
			return null;
		}
		HubData[] array = hubs;
		foreach (HubData hubData in array)
		{
			if (hubData.levels.Contains(currentPlayableLevel))
			{
				return hubData;
			}
		}
		return null;
	}

	public HubData GetHubByLevelName(string name)
	{
		HubData[] array = hubs;
		foreach (HubData hubData in array)
		{
			if (string.Compare(hubData.GetFirstLevel().sceneName, name, ignoreCase: true) == 0)
			{
				return hubData;
			}
		}
		return null;
	}

	public HubData GetHubByAnyLevel(string name)
	{
		HubData[] array = hubs;
		foreach (HubData hubData in array)
		{
			for (int j = 0; j < hubData.levels.Count; j++)
			{
				if (hubData.levels[j].sceneName == name)
				{
					return hubData;
				}
			}
		}
		return null;
	}

	public SceneData GetLevelByName(string name)
	{
		SceneData[] array = levels;
		foreach (SceneData sceneData in array)
		{
			if (string.Compare(sceneData.sceneName, name, ignoreCase: true) == 0)
			{
				lastPlayableLevel = sceneData;
				return sceneData;
			}
		}
		return null;
	}

	public bool RegisterMission(SceneData data)
	{
		if (!missions.TryGetValue(data.sceneName, out tempResults))
		{
			tempResults = new LevelResults(data.sceneName);
			tempResults.reached = PlayerPrefs.GetInt($"{data.sceneName}_reached");
			tempResults.rank = PlayerPrefs.GetInt($"{data.sceneName}_rank");
			tempResults.points = PlayerPrefs.GetInt($"{data.sceneName}_points");
			tempResults.combo = PlayerPrefs.GetInt($"{data.sceneName}_combo");
			tempResults.noDanage = PlayerPrefs.GetInt($"{data.sceneName}_noDamage");
			tempResults.noFalls = PlayerPrefs.GetInt($"{data.sceneName}_noFalls");
			tempResults.noMercy = PlayerPrefs.GetInt($"{data.sceneName}_noMercy");
			tempResults.time = PlayerPrefs.GetFloat($"{data.sceneName}_time");
			missions.Add(data.sceneName, tempResults);
			data.results = missions[data.sceneName];
			return true;
		}
		return false;
	}

	public void RegisterMission(string sceneName, out LevelResults results)
	{
		if (!missions.TryGetValue(sceneName, out tempResults))
		{
			tempResults = new LevelResults(sceneName);
			tempResults.reached = PlayerPrefs.GetInt($"{sceneName}_reached");
			tempResults.rank = PlayerPrefs.GetInt($"{sceneName}_rank");
			tempResults.points = PlayerPrefs.GetInt($"{sceneName}_points");
			tempResults.combo = PlayerPrefs.GetInt($"{sceneName}_combo");
			tempResults.noDanage = PlayerPrefs.GetInt($"{sceneName}_noDamage");
			tempResults.noFalls = PlayerPrefs.GetInt($"{sceneName}_noFalls");
			tempResults.noMercy = PlayerPrefs.GetInt($"{sceneName}_noMercy");
			tempResults.secret = PlayerPrefs.GetInt($"{sceneName}_secret");
			tempResults.time = PlayerPrefs.GetFloat($"{sceneName}_time");
			missions.Add(sceneName, tempResults);
		}
		results = missions[sceneName];
	}

	public int GetHubState(HubData hub)
	{
		int value = 0;
		if (!hubsProgress.TryGetValue(hub, out value))
		{
			value = PlayerPrefs.GetInt($"{hub.GetFirstLevel().sceneName}_progress");
			hubsProgress.Add(hub, value);
		}
		return value;
	}

	public void UpdateLastMissionResults()
	{
		if (!Game.mission.levelData)
		{
			Debug.Log("No Level Data!");
			return;
		}
		if (!missions.TryGetValue(Game.mission.levelData.sceneName, out tempResults))
		{
			RegisterMission(Game.mission.levelData.sceneName, out tempResults);
		}
		if (tempResults.combo.SetIfLower(Game.mission.rawResults.combo))
		{
			Game.mission.rawResults.pbCombo = true;
			Debug.Log("New Combo");
		}
		if (tempResults.rank.SetIfLower(Game.mission.rawResults.rank))
		{
			Game.mission.rawResults.pbRank = true;
			Debug.Log("New Rank");
		}
		if (tempResults.points.SetIfLower(Game.mission.rawResults.points))
		{
			Game.mission.rawResults.pbPoints = true;
			Debug.Log("New Best Points");
		}
		if (tempResults.noDanage.SetIfLower(Game.mission.rawResults.noDamage ? 1 : 0))
		{
			Game.mission.rawResults.pbNoDamage = true;
			Debug.Log("NoDamage Achieved");
		}
		if (tempResults.noFalls.SetIfLower(Game.mission.rawResults.noFalls ? 1 : 0))
		{
			Game.mission.rawResults.pbNoFalls = true;
			Debug.Log("NoFalls Achieved");
		}
		if (tempResults.noMercy.SetIfLower(Game.mission.rawResults.noMercy ? 1 : 0))
		{
			Debug.Log("NoMercy Achieved");
		}
		if (tempResults.secret.SetIfLower(Game.mission.rawResults.secret ? 1 : 0))
		{
			Debug.Log("Secret Collected");
		}
		if (tempResults.time == 0f || Game.mission.rawResults.time < tempResults.time)
		{
			Game.mission.rawResults.pbTime = true;
			tempResults.time = Game.mission.rawResults.time;
			Debug.Log("New Best Time");
		}
	}

	public void SaveAllCurrentResults()
	{
		foreach (KeyValuePair<string, LevelResults> mission in missions)
		{
			PlayerPrefs.SetInt($"{mission.Key}_reached", mission.Value.reached);
			PlayerPrefs.SetInt($"{mission.Key}_rank", mission.Value.rank);
			PlayerPrefs.SetInt($"{mission.Key}_points", mission.Value.points);
			PlayerPrefs.SetInt($"{mission.Key}_combo", mission.Value.combo);
			PlayerPrefs.SetInt($"{mission.Key}_noDamage", mission.Value.noDanage);
			PlayerPrefs.SetInt($"{mission.Key}_noFalls", mission.Value.noDanage);
			PlayerPrefs.SetInt($"{mission.Key}_noMercy", mission.Value.noMercy);
			PlayerPrefs.SetInt($"{mission.Key}_secret", mission.Value.secret);
			PlayerPrefs.SetFloat($"{mission.Key}_time", mission.Value.time);
		}
		foreach (KeyValuePair<HubData, int> item in hubsProgress)
		{
			PlayerPrefs.SetInt($"{item.Key.GetFirstLevel().sceneName}_progress", item.Value);
		}
	}

	public void ClearAll()
	{
		missions.Clear();
		hubsProgress.Clear();
	}

	private void OnApplicationQuit()
	{
		SaveAllCurrentResults();
		if (Hub.lastHub.Length > 0)
		{
			PlayerPrefs.SetString("Last Hub", Hub.lastHub);
		}
	}
}
