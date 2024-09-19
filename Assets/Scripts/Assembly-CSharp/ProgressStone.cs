using System;
using UnityEngine;

public class ProgressStone : MonoBehaviour
{
	public HubData hub;

	public Transform tStone;

	public MeshRenderer rend;

	private Vector3 temp;

	private MaterialPropertyBlock block;

	private void Awake()
	{
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Combine(Game.OnPlayableLevelLoaded, new Action<string>(OnLevelLoaded));
	}

	private void OnDestroy()
	{
		Game.OnPlayableLevelLoaded = (Action<string>)Delegate.Remove(Game.OnPlayableLevelLoaded, new Action<string>(OnLevelLoaded));
	}

	private void Update()
	{
		temp.z = 6f + Mathf.Sin(Time.time / 2f);
		tStone.localPosition = temp;
	}

	private void OnLevelLoaded(string sceneName)
	{
		hub = LevelsData.instance.GetHubByLevelName(sceneName);
		if ((bool)hub)
		{
			hub.CheckProgress();
			Debug.Log(hub.ProgressBySSSRank);
			block.SetFloat("_FillAmount", Mathf.Lerp(4f, 0f, hub.ProgressBySSSRank / 2f + hub.ProgressBySRank / 2f));
			rend.SetPropertyBlock(block);
		}
	}
}
