using UnityEngine;
using UnityEngine.UI;

public class HubPortalUI : MonoBehaviour
{
	[HideInInspector]
	public HubPortal portal;

	public RectTransform t;

	public CanvasGroup cg;

	public TextAnimator animator;

	public TextBackground textBackground;

	public Text txtName;

	public Text txtRank;

	public Text txtScore;

	public Image image;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float xOffset = 16f;

	public float yOffset = -32f;

	private float timer;

	private bool isShowing;

	private Vector3 temp;

	private void Awake()
	{
		cg.alpha = 0f;
		image.enabled = false;
	}

	public void Show()
	{
		if (!isShowing)
		{
			timer = 0f;
			isShowing = true;
			cg.alpha = 0f;
			animator.ResetAndPlay();
			temp.y = 64f;
			t.anchoredPosition3D = temp;
		}
	}

	public void Hide()
	{
		if (isShowing)
		{
			isShowing = false;
		}
	}

	public void Setup(HubPortal newPortal)
	{
		portal = newPortal;
		portal.ui = this;
		if (portal.data.sceneType > SceneData.SceneType.Hub && !LevelsData.instance.missions.TryGetValue(portal.data.sceneName, out portal.data.results))
		{
			LevelsData.instance.RegisterMission(portal.data.sceneName, out portal.data.results);
		}
		if (portal.data.sceneType == SceneData.SceneType.Hub)
		{
			txtName.text = portal.data.GetLevelPublicName().ToUpper();
		}
		else
		{
			txtName.text = portal.data.GetLevelPublicName();
		}
		textBackground.Invoke("Setup", 0.1f);
	}

	public void CheckIcon()
	{
		if (!portal.isLocked && (portal.data.sceneType == SceneData.SceneType.Tower || portal.data.sceneType == SceneData.SceneType.Arena) && portal.data.results.rank.Inside(StyleData.instance.ranks.ranks.Length))
		{
			image.enabled = true;
			image.sprite = StyleData.instance.ranks.ranks[portal.data.results.rank].sprite;
		}
	}

	private void Update()
	{
		float num = Vector3.Dot(Game.player.tHead.forward, Game.player.t.position.DirToXZ(portal.tMesh.position));
		cg.alpha = (Game.paused ? 0f : Mathf.MoveTowards(cg.alpha, isShowing ? (Mathf.Clamp01(num * 2f) - Game.wideMode.cg.alpha) : 0f, Time.deltaTime * 6f));
		temp.y = Mathf.Lerp(temp.y, 96f, Time.deltaTime * 4f);
		t.anchoredPosition3D = temp;
	}
}
