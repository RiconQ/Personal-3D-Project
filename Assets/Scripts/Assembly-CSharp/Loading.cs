using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
	public static Action OnLoadingStart = delegate
	{
	};

	public static Action OnLoadingEnd = delegate
	{
	};

	public static string levelToLoad = "MainMenu";

	public static string levelToLoadAddictive = "";

	public static string customPartingWords = "";

	public static bool quickLoading = false;

	public static AsyncOperation loading;

	private float minLoadingTime = 1f;

	private float progress;

	private float timer;

	private WaitForSeconds waitForSeconds = new WaitForSeconds(0.25f);

	private void Start()
	{
		StartCoroutine(SingleLoading());
	}

	private IEnumerator SingleLoading()
	{
		loading = SceneManager.LoadSceneAsync(levelToLoad);
		loading.allowSceneActivation = false;
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (!quickLoading && OnLoadingStart != null)
		{
			OnLoadingStart();
		}
		while (progress < 1f)
		{
			timer += Time.unscaledDeltaTime;
			progress = Mathf.Clamp01(loading.progress / 0.9f);
			yield return null;
		}
		if (!quickLoading)
		{
			while (timer < minLoadingTime)
			{
				timer = Mathf.MoveTowards(timer, minLoadingTime, Time.unscaledDeltaTime);
				yield return null;
			}
			if (OnLoadingEnd != null)
			{
				OnLoadingEnd();
			}
		}
		while (Game.loadingIcon.cg.alpha != 0f)
		{
			yield return null;
		}
		quickLoading = false;
		yield return waitForSeconds;
		loading.allowSceneActivation = true;
	}
}
