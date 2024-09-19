using System;
using UnityEngine;

public class MissionState : MonoBehaviour
{
	public enum MissionStates
	{
		Intro = 0,
		InProcess = 1,
		Complete = 2
	}

	public static Action OnMissionCompleted = delegate
	{
	};

	public static string currentMissionName = "";

	public const float _fadingDelay = 2.25f;

	public SceneReference resultsScene;

	public SceneData levelData;

	private float timer;

	public RawLevelResults rawResults { get; private set; }

	public MissionStates state { get; private set; }

	private void Awake()
	{
		rawResults = new RawLevelResults();
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Combine(Game.OnPlayableLevelLoaded, new Action<string>(StartNewMission));
		QuickmapScene.OnPlayMode = (Action)Delegate.Combine(QuickmapScene.OnPlayMode, new Action(ResetMission));
		CrowdControl.OnAllEnemiesDead = (Action)Delegate.Combine(CrowdControl.OnAllEnemiesDead, new Action(OnAllEnemiesDead));
		PlayerController.OnDamage = (Action<Vector3>)Delegate.Combine(PlayerController.OnDamage, new Action<Vector3>(OnDamage));
		OceanScript.OnFall = (Action)Delegate.Combine(OceanScript.OnFall, new Action(OnFall));
		HiddenObject.OnPick = (Action)Delegate.Combine(HiddenObject.OnPick, new Action(OnObjectPicked));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Combine(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void OnDestroy()
	{
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Remove(Game.OnPlayableLevelLoaded, new Action<string>(StartNewMission));
		QuickmapScene.OnPlayMode = (Action)Delegate.Remove(QuickmapScene.OnPlayMode, new Action(ResetMission));
		CrowdControl.OnAllEnemiesDead = (Action)Delegate.Remove(CrowdControl.OnAllEnemiesDead, new Action(OnAllEnemiesDead));
		PlayerController.OnDamage = (Action<Vector3>)Delegate.Remove(PlayerController.OnDamage, new Action<Vector3>(OnDamage));
		OceanScript.OnFall = (Action)Delegate.Remove(OceanScript.OnFall, new Action(OnFall));
		HiddenObject.OnPick = (Action)Delegate.Remove(HiddenObject.OnPick, new Action(OnObjectPicked));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		QuickmapScene.OnEditMode = (Action)Delegate.Remove(QuickmapScene.OnEditMode, new Action(Reset));
	}

	private void OnAllEnemiesDead()
	{
		rawResults.noMercy = true;
	}

	private void OnDamage(Vector3 dir)
	{
		if (state == MissionStates.InProcess)
		{
			rawResults.noDamage = false;
		}
	}

	private void OnFall()
	{
		if (state == MissionStates.InProcess)
		{
			rawResults.noFalls = false;
		}
	}

	private void OnObjectPicked()
	{
		if (state == MissionStates.InProcess)
		{
			rawResults.secret = true;
		}
	}

	private void Reset()
	{
		rawResults.Clear();
		SetState(0);
		Game.time.StopSlowmo();
		Game.time.SetDefaultTimeScale(1f);
		Game.wideMode.Hide();
	}

	private void ResetMission()
	{
		SetState(0);
	}

	private void StartNewMission(string name)
	{
		rawResults.Clear();
		levelData = LevelsData.instance.GetLevelByName(name);
		if (levelData.results.reached == 0)
		{
			LevelsData.instance.RegisterMission(levelData);
			levelData.results.reached = 1;
			PlayerPrefs.SetInt($"{levelData.sceneName}_reached", 1);
		}
		SetState(0);
	}

	public string GetLastLevel()
	{
		if (!levelData)
		{
			return Game.fallbackScene;
		}
		return levelData.sceneName;
	}

	public void SetState(int i)
	{
		if (state == (MissionStates)i)
		{
			return;
		}
		state = (MissionStates)i;
		switch (state)
		{
		case MissionStates.InProcess:
			rawResults.Clear();
			break;
		case MissionStates.Complete:
		{
			timer = 0f;
			int num = (rawResults.rank = StyleRanking.instance.rankIndex);
			if (num < 1 && rawResults.noFalls)
			{
				rawResults.rank = 1;
			}
			if (num < 2 && rawResults.noDamage)
			{
				rawResults.rank = 2;
			}
			rawResults.combo = StyleRanking.instance.combo.maxCombo;
			rawResults.points = StyleRanking.instance.GetScore();
			LevelsData.instance.UpdateLastMissionResults();
			Game.player.RageOFF();
			Game.wideMode.Show();
			Game.time.StopSlowmo();
			Game.time.SetDefaultTimeScale(0.25f);
			if (OnMissionCompleted != null)
			{
				OnMissionCompleted();
			}
			break;
		}
		}
	}

	public void Tick()
	{
		if (Game.paused || !Game.player)
		{
			return;
		}
		switch (state)
		{
		case MissionStates.InProcess:
			rawResults.time += Time.unscaledDeltaTime;
			if (!Game.player.inputActive || Game.player.rb.isKinematic)
			{
				break;
			}
			if (Game.player.grounder.grounded)
			{
				if (Game.player.rb.velocity.sqrMagnitude > 16f)
				{
					rawResults.groundedTime += Time.unscaledDeltaTime;
					if (Game.player.slide.isSliding)
					{
						rawResults.slideTime += Time.unscaledDeltaTime;
					}
				}
			}
			else
			{
				rawResults.airTime += Time.unscaledDeltaTime;
				if (Game.player.parkourActionsCount > 0)
				{
					rawResults.parkourTime += Time.unscaledDeltaTime;
				}
			}
			break;
		case MissionStates.Complete:
			timer = Mathf.MoveTowards(timer, 2.25f, Time.unscaledDeltaTime);
			if (timer != 2.25f || !Game.player.isActiveAndEnabled)
			{
				break;
			}
			if ((bool)QuickmapScene.instance)
			{
				QuickmapScene.instance.Play();
				Game.time.StopSlowmo();
				Game.wideMode.Set(0f);
			}
			else
			{
				Game.fading.speed = 0.2f;
				if (levelData.sceneType == SceneData.SceneType.Tutorial)
				{
					Game.instance.LoadLevel(Hub.GetLashHub());
				}
				else
				{
					Game.instance.LoadLevel(resultsScene.ScenePath, quickLoad: true);
				}
			}
			SetState(0);
			break;
		case MissionStates.Intro:
			break;
		}
	}
}
