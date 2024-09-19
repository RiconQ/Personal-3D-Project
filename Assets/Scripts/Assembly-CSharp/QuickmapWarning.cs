using UnityEngine;

public class QuickmapWarning : MonoBehaviour
{
	public static QuickmapWarning instance;

	public CanvasGroup cg;

	private void Awake()
	{
		instance = this;
		if (!cg)
		{
			cg = GetComponent<CanvasGroup>();
		}
		cg.alpha = 0f;
	}
}
