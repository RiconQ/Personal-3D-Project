using UnityEngine;
using UnityEngine.UI;

public class PlayerHoldingBar : MonoBehaviour
{
	public Image imgLeft;

	public Image imgRight;

	private float speed = 2f;

	[Range(0f, 1f)]
	public float value;

	public CanvasGroup cg;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		imgRight.fillAmount = value;
		imgLeft.fillAmount = value;
		cg.alpha = value;
	}
}
