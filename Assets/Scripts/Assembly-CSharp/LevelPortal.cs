using UnityEngine;

public class LevelPortal : MonoBehaviour
{
	public SceneData level;

	public string levelToLoad;

	[TextArea]
	public string partingWords;

	public AudioClip sfxOnEnable;

	public AudioClip sfxOnDisable;

	public AudioClip sfxTeleportation;

	private AudioSource source;

	private ParticleSystem[] particles;

	public bool isOpened { get; private set; }

	public Transform t { get; private set; }

	private void Awake()
	{
		t = base.transform;
		source = GetComponentInChildren<AudioSource>();
		particles = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].Stop();
		}
	}

	private void Update()
	{
		bool value = Vector3.Distance(t.position, PlayerController.instance.t.position) < 9f;
		OpenOrClose(value);
		if (isOpened)
		{
			source.volume = Mathf.MoveTowards(source.volume, 1f, Time.deltaTime);
		}
	}

	public void OpenOrClose(bool value)
	{
		if (value == isOpened)
		{
			return;
		}
		isOpened = value;
		StopAllCoroutines();
		if (isOpened)
		{
			Game.sounds.PlayClipAtPosition(sfxOnEnable, 1f, t.position);
			source.volume = 0f;
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Play();
			}
			source.Play();
		}
		else
		{
			Game.sounds.PlayClipAtPosition(sfxOnDisable, 0.5f, t.position);
			for (int j = 0; j < particles.Length; j++)
			{
				particles[j].Stop();
			}
			source.Stop();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (Game.mission.state == MissionState.MissionStates.InProcess)
		{
			Game.mission.SetState(2);
		}
		LoadLevel();
	}

	public void LoadLevel()
	{
		Game.mission.SetState(2);
		Game.sounds.PlayClip(sfxTeleportation);
		Game.fading.InstantFade(1f);
		if (levelToLoad == "Hub")
		{
			Game.instance.LoadLevel(Hub.lastHub);
		}
		else if (levelToLoad.Length > 0)
		{
			Game.instance.LoadLevel(levelToLoad);
		}
		else if (Hub.lastHub.Length != 0)
		{
			Game.instance.LoadLevel(Hub.lastHub);
		}
		else
		{
			Game.instance.LoadLevel("MainMenu");
		}
		if (partingWords.Length > 0)
		{
			Loading.customPartingWords = partingWords;
		}
	}
}
