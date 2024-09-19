using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
	public static Game instance;

	public static Action<string> OnPlayableLevelLoaded = delegate
	{
	};

	public static Action<bool> OnPause = delegate
	{
	};

	public static Action OnAnyLevelUnloaded = delegate
	{
	};

	public static bool paused = false;

	public TheSkullSequence[] sequences;

	public static bool actionCameraEnabled = false;

	public static bool godmode = false;

	public static bool debug = false;

	public static string lastSceneWithPlayer = string.Empty;

	public static PlayerController player;

	public static StyleData style;

	public static Localization localization;

	public static FadingUI fading;

	public static WideModeUI wideMode;

	public static MessageUI message;

	public static TimeManager time;

	public static AudioManager audioManager;

	public static SoundsManager sounds;

	public static Ambience ambience;

	public static MissionState mission;

	public static GamePrefs gamePrefs;

	public static LoadingIcon loadingIcon;

	private QuickMenu[] ingameMenus;

	private bool dontActivatePlayer;

	private bool pauseMenuEnabled;

	public GameObject _prefabPlayer;

	public GameObject _prefabDebugCam;

	public SceneReference _sceneLoadingScreen;

	public CanvasGroup cgPauseBackground;

	public static string fallbackScene { get; private set; }

	public static Coroutine loading { get; private set; }

	private void Awake()
	{
		fallbackScene = "MainMenu";
		Debug.unityLogger.logEnabled = false;
		if (!instance)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			Application.targetFrameRate = 60;
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			sequences = Resources.LoadAll<TheSkullSequence>("Skull/");
			localization = GetComponentInChildren<Localization>();
			fading = GetComponentInChildren<FadingUI>();
			loadingIcon = GetComponentInChildren<LoadingIcon>();
			style = GetComponentInChildren<StyleData>();
			wideMode = GetComponentInChildren<WideModeUI>();
			message = GetComponentInChildren<MessageUI>();
			message.Setup();
			time = GetComponentInChildren<TimeManager>();
			audioManager = GetComponentInChildren<AudioManager>();
			sounds = GetComponentInChildren<SoundsManager>();
			ingameMenus = GetComponentsInChildren<QuickMenu>();
			ambience = GetComponentInChildren<Ambience>();
			mission = GetComponent<MissionState>();
			gamePrefs = GetComponent<GamePrefs>();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		InputsManager.OnBack = (Action)Delegate.Combine(InputsManager.OnBack, new Action(Back));
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		InputsManager.OnBack = (Action)Delegate.Remove(InputsManager.OnBack, new Action(Back));
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Unpause();
		time.StopSlowmo();
		time.SetDefaultTimeScale(1f);
		if (mode == LoadSceneMode.Additive)
		{
			SceneManager.SetActiveScene(scene);
			LightProbes.Tetrahedralize();
		}
		if ((bool)QuickmapScene.instance)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			player = UnityEngine.Object.Instantiate(_prefabPlayer).GetComponent<PlayerController>();
			player.transform.SetAsFirstSibling();
			player.gameObject.SetActive(value: false);
		}
		else
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		if (scene.path != _sceneLoadingScreen.ScenePath)
		{
			GameObject gameObject = GameObject.FindGameObjectWithTag("Entrance");
			if ((bool)gameObject)
			{
				if (PlayerController.instance != null)
				{
					player = PlayerController.instance;
				}
				else
				{
					player = UnityEngine.Object.Instantiate(_prefabPlayer).GetComponent<PlayerController>();
				}
				player.transform.SetAsFirstSibling();
				if (!QuickmapScene.instance && (!Hub.instance || !Hub.instance.LastActivePortal()))
				{
					gameObject.GetComponent<SavePoint>().Spawn();
				}
				if (OnPlayableLevelLoaded != null)
				{
					OnPlayableLevelLoaded(scene.name);
				}
			}
			fading.Fade(0f);
		}
		pauseMenuEnabled = (bool)player && !QuickmapScene.instance;
	}

	private void Update()
	{
		cgPauseBackground.alpha = Mathf.MoveTowards(cgPauseBackground.alpha, paused ? 1 : 0, Time.unscaledDeltaTime * 8f);
		loadingIcon.Tick();
		if (loading != null)
		{
			return;
		}
		wideMode.Tick();
		mission.Tick();
		if (PlayerController.gamepad && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButton(0)))
		{
			PlayerController.gamepad = false;
		}
		if (!player || (bool)QuickmapScene.instance)
		{
			return;
		}
		if (Input.GetButtonDown("Pause") && !paused && player.gameObject.activeInHierarchy)
		{
			Pause();
		}
		if (!debug)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.F1))
		{
			if (player.gameObject.activeInHierarchy)
			{
				if (!ActionCam.instance)
				{
					UnityEngine.Object.Instantiate(_prefabDebugCam);
				}
				else
				{
					ActionCam.instance.gameObject.SetActive(value: true);
				}
				player.gameObject.SetActive(value: false);
			}
			else
			{
				player.gameObject.SetActive(value: true);
				ActionCam.instance.gameObject.SetActive(value: false);
			}
		}
		if (Input.GetKeyDown(KeyCode.F10))
		{
			godmode = !godmode;
			message.Show("godmode " + godmode);
		}
	}

	private void DoBeforeLoading()
	{
		wideMode.Hide();
		message.Hide();
		time.StopSlowmo();
		time.SetDefaultTimeScale(1f);
	}

	public void LoadLevel(string name, bool quickLoad = false)
	{
		if (loading == null && (Application.CanStreamedLevelBeLoaded(name) || name == "Quit"))
		{
			if ((bool)player)
			{
				lastSceneWithPlayer = SceneManager.GetActiveScene().name;
			}
			Loading.quickLoading = quickLoad;
			loading = StartCoroutine(LevelLoading(name));
		}
	}

	public void LoadLevel(SceneData data, bool quickLoad = false)
	{
		if (loading == null && (Application.CanStreamedLevelBeLoaded(base.name) || data.sceneName == "Quit"))
		{
			if ((bool)player)
			{
				lastSceneWithPlayer = SceneManager.GetActiveScene().name;
			}
			Loading.quickLoading = quickLoad;
			loading = StartCoroutine(LevelLoading(data.sceneName));
		}
	}

	public IEnumerator LevelLoading(string name)
	{
		if (paused)
		{
			Unpause();
		}
		time.SetDefaultTimeScale(0.1f);
		while (cgPauseBackground.alpha != 0f)
		{
			yield return null;
		}
		if (fading.cg.alpha != 1f)
		{
			fading.Fade(1f);
			yield return new WaitForEndOfFrame();
			while (fading.fading != null)
			{
				yield return null;
			}
		}
		yield return new WaitForEndOfFrame();
		loading = null;
		DoBeforeLoading();
		if (OnAnyLevelUnloaded != null)
		{
			OnAnyLevelUnloaded();
		}
		if (name != "Quit")
		{
			Loading.levelToLoad = name;
			if (SceneManager.GetActiveScene().name != "Skull")
			{
				TheSkullSequence[] array = sequences;
				foreach (TheSkullSequence theSkullSequence in array)
				{
					if ((bool)theSkullSequence.loadAtScene && ((theSkullSequence.moment == TheSkullSequence.SequenceTiming.BeforeScene && theSkullSequence.loadAtScene.sceneReference.ScenePath == name) || (theSkullSequence.moment == TheSkullSequence.SequenceTiming.AfterScene && SceneManager.GetActiveScene().path == theSkullSequence.loadAtScene.sceneReference.ScenePath)))
					{
						TheSkull.overrideSequence = theSkullSequence;
						TheSkull.overrideSceneToLoad = name;
						SceneManager.LoadScene("Skull");
						yield break;
					}
				}
			}
			SceneManager.LoadScene(_sceneLoadingScreen.ScenePath);
		}
		else
		{
			Application.Quit();
		}
	}

	public void Back()
	{
		if (ingameMenus[0].active && paused && loading == null)
		{
			Unpause();
		}
	}

	public void Pause()
	{
		if ((bool)player)
		{
			if (player.inputActive)
			{
				player.Deactivate();
			}
			else
			{
				dontActivatePlayer = true;
			}
		}
		paused = true;
		ingameMenus[0].Activate();
		time.Stop();
		if (OnPause != null)
		{
			OnPause(obj: true);
		}
	}

	public void Unpause()
	{
		if ((bool)player)
		{
			if (!dontActivatePlayer)
			{
				player.Activate();
			}
			else
			{
				dontActivatePlayer = false;
			}
		}
		paused = false;
		ingameMenus[0].Deactivate();
		time.Play();
		if (OnPause != null)
		{
			OnPause(obj: false);
		}
	}

	public void Restart()
	{
		LoadLevel(SceneManager.GetActiveScene().name);
	}

	public void Quit()
	{
		LoadLevel("Quit");
	}
}
