using System.Collections;
using UnityEngine;

public class MissionResultsUI : MonoBehaviour
{
	public CanvasGroup cg;

	public TextAnimator animatorLevelName;

	public TextAnimator animatorHubName;

	public RectTransform tHeader;

	public QuickMenu menu;

	public AudioClip sfxSwoosh;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private RectTransform t;

	private MissionResultsItem[] items;

	public float speed = 1.5f;

	private void Awake()
	{
		t = GetComponent<RectTransform>();
		items = GetComponentsInChildren<MissionResultsItem>();
		cg.alpha = 0f;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Setup();
		}
	}

	private void OnEnable()
	{
		StartCoroutine(Showing());
	}

	private IEnumerator Showing()
	{
		if ((bool)LevelsData.lastPlayableLevel)
		{
			animatorLevelName.ResetAndPlay(LevelsData.lastPlayableLevel.publicName);
			animatorHubName.ResetAndPlay(LevelsData.lastPlayableLevel.area.levels[0].publicName);
		}
		else
		{
			animatorLevelName.ResetAndPlay("Level Name");
			animatorHubName.ResetAndPlay("Hub Name");
		}
		yield return new WaitForEndOfFrame();
		cg.alpha = 1f;
		tHeader.anchoredPosition3D = new Vector3(80f + tHeader.sizeDelta.x / 2f, 64f + tHeader.sizeDelta.y / 2f, 0f);
		for (int i = 0; i < items.Length; i++)
		{
			items[i].bPos.x = (items[i].aPos.x = 80f + items[i].t.sizeDelta.x / 2f);
			items[i].bPos.y = (items[i].aPos.y = -64 + items.Length / 2 * 34 - i * 34);
			items[i].aPos.x -= Random.Range(-96f, 128f);
			items[i].aPos.y += Random.Range(-96f, 96f);
			items[i].t.anchoredPosition3D = items[i].aPos;
			items[i].t.localEulerAngles = new Vector3(0f, 0f, Random.Range(-30f, 30f));
		}
		float timer = 0f;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
			for (int j = 0; j < items.Length; j++)
			{
				items[j].cg.alpha = timer;
				items[j].t.anchoredPosition3D = Vector3.LerpUnclamped(items[j].aPos, items[j].bPos, curve.Evaluate(Mathf.Pow(timer, 1f + (float)j * 0.2f)));
				items[j].t.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(items[j].t.localEulerAngles.z, 0f, timer));
			}
			yield return null;
		}
		menu.Activate();
	}
}
