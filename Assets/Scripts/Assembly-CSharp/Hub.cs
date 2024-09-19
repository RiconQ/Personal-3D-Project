using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hub : MonoBehaviour
{
	public static Hub instance;

	public static HubData lastHubData;

	public static SceneData levelData;

	public static string lastHub = "";

	public static HubPortal lastPortal;

	public GameObject _prefabHubPortalUI;

	public GameObject _prefabBlackBox;

	public HubData data;

	public bool dontShowProgressCutscene;

	public List<HubPortal> portals { get; private set; }

	public int levelsCount { get; private set; }

	public int progress { get; private set; }

	public int index { get; private set; }

	public Transform t { get; private set; }

	public RectTransform tCanvas { get; private set; }

	public static string GetLashHub()
	{
		if (lastHub.Length <= 0)
		{
			return Game.fallbackScene;
		}
		return lastHub;
	}

	public static string GetNext()
	{
		if (lastHubData != null)
		{
			for (int i = 0; i < lastHubData.levels.Count; i++)
			{
				if (lastHubData.levels[i].sceneName == Game.lastSceneWithPlayer)
				{
					if (i + 1 < lastHubData.levels.Count)
					{
						return lastHubData.levels[i + 1].sceneName;
					}
					return lastHubData.levels[0].sceneName;
				}
			}
		}
		return Game.fallbackScene;
	}

	public bool LastActivePortal()
	{
		if (Game.lastSceneWithPlayer.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < portals.Count; i++)
		{
			if (string.Compare(Game.lastSceneWithPlayer, portals[i].data.sceneName, ignoreCase: true) == 0)
			{
				lastPortal = portals[i];
				lastPortal.SpawnPlayer();
				Game.lastSceneWithPlayer = "";
				return true;
			}
		}
		return false;
	}

	private void Awake()
	{
		instance = this;
		index = -1;
		t = base.transform;
		tCanvas = t.Find("Canvas").GetComponent<RectTransform>();
		portals = new List<HubPortal>();
		portals.AddRange(Object.FindObjectsOfTypeAll(typeof(HubPortal)) as HubPortal[]);
		Object.Instantiate(_prefabBlackBox, new Vector3(0f, 300f, 0f), Quaternion.identity);
		int num2 = (progress = 0);
		levelsCount = num2;
		for (int i = 0; i < portals.Count; i++)
		{
			HubPortal hubPortal = portals[i];
			if (hubPortal.data == null)
			{
				portals.RemoveAt(i);
				i--;
				continue;
			}
			portals[i].Setup();
			Object.Instantiate(_prefabHubPortalUI, tCanvas).GetComponent<HubPortalUI>().Setup(hubPortal);
			if (hubPortal.GetType() == typeof(PortalDock))
			{
				levelsCount++;
				if (hubPortal.data.sceneType > SceneData.SceneType.Hub && hubPortal.data.results.time > 0f)
				{
					progress++;
				}
			}
		}
		if (levelsCount > 0)
		{
			progress /= levelsCount;
		}
		lastHubData = data;
		lastHub = SceneManager.GetActiveScene().name;
		levelData = LevelsData.instance.GetLevelByName(lastHub);
	}

	public HubPortal GetHubPortal()
	{
		foreach (HubPortal portal in portals)
		{
			if (portal.GetType() == typeof(HubPortalToHub))
			{
				return portal;
			}
		}
		return null;
	}

	private void Start()
	{
		LevelsData.instance.UnlockCurrentHub();
		foreach (HubPortal portal in portals)
		{
			portal.Check();
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	private void Update()
	{
		int num = -1;
		float num2 = 10f;
		for (int i = 0; i < portals.Count; i++)
		{
			if (portals[i].isActiveAndEnabled)
			{
				float num3 = Vector3.Distance(Game.player.t.position, portals[i].t.position);
				if (num3 < num2)
				{
					num2 = num3;
					num = i;
				}
			}
		}
		if (index != num && !Game.player.rb.isKinematic)
		{
			if (index > -1)
			{
				portals[index].Toggle(value: false);
			}
			index = num;
			if (index > -1)
			{
				portals[index].Toggle(value: true);
			}
		}
	}
}
