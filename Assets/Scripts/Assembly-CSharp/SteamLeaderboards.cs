using System.Threading;
using Steamworks;
using UnityEngine;

public class SteamLeaderboards : MonoBehaviour
{
	private const string s_leaderboardName = "StoryMode";

	private const ELeaderboardUploadScoreMethod s_leaderboardMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;

	private static SteamLeaderboard_t s_currentLeaderboard;

	private static bool s_initialized = false;

	private static CallResult<LeaderboardFindResult_t> m_findResult = new CallResult<LeaderboardFindResult_t>();

	private static CallResult<LeaderboardScoreUploaded_t> m_uploadResult = new CallResult<LeaderboardScoreUploaded_t>();

	private static Timer timer1;

	public static void UpdateScore(int score)
	{
		if (!s_initialized)
		{
			Debug.Log("Can't upload to the leaderboard because isn't loadded yet");
			return;
		}
		Debug.Log("uploading score(" + score + ") to steam leaderboard(StoryMode)");
		SteamAPICall_t hAPICall = SteamUserStats.UploadLeaderboardScore(s_currentLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, null, 0);
		m_uploadResult.Set(hAPICall, OnLeaderboardUploadResult);
	}

	public static void Init()
	{
		SteamAPICall_t hAPICall = SteamUserStats.FindLeaderboard("StoryMode");
		m_findResult.Set(hAPICall, OnLeaderboardFindResult);
		InitTimer();
	}

	private static void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool failure)
	{
		Debug.Log("STEAM LEADERBOARDS: Found - " + pCallback.m_bLeaderboardFound + " leaderboardID - " + pCallback.m_hSteamLeaderboard.m_SteamLeaderboard);
		s_currentLeaderboard = pCallback.m_hSteamLeaderboard;
		s_initialized = true;
	}

	private static void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
	{
		Debug.Log("STEAM LEADERBOARDS: failure - " + failure + " Completed - " + pCallback.m_bSuccess + " NewScore: " + pCallback.m_nGlobalRankNew + " Score " + pCallback.m_nScore + " HasChanged - " + pCallback.m_bScoreChanged);
	}

	public static void InitTimer()
	{
		timer1 = new Timer(timer1_Tick, null, 0, 1000);
	}

	private static void timer1_Tick(object state)
	{
		SteamAPI.RunCallbacks();
	}
}
