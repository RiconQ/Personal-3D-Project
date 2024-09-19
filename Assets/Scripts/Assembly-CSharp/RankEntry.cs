using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class RankEntry : MonoBehaviour
{
	public Text enemyName;

	public Text deathInfo;

	public GameObject objDifference { get; private set; }

	public TextAnimator[] animator { get; private set; }

	public CanvasGroup[] cg { get; private set; }

	private void Awake()
	{
		cg = GetComponentsInChildren<CanvasGroup>();
		cg[0].alpha = 0f;
		cg[1].alpha = 0f;
		animator = GetComponentsInChildren<TextAnimator>();
		objDifference = base.transform.Find("Difference").gameObject;
	}
}
