using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
	public GameObject targetObject;

	private ParticleSystem particle;

	private CanvasGroup cg;

	private TextAnimator animator;

	private float alpha;

	private void Awake()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Symbols"), base.transform) as GameObject;
		gameObject.transform.localPosition = Vector3.zero;
		particle = gameObject.GetComponentInChildren<ParticleSystem>();
		cg = targetObject.GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		animator = targetObject.GetComponent<TextAnimator>();
	}

	private void Update()
	{
		if (cg.alpha != alpha)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.deltaTime * 2f);
		}
	}

	private void OnTriggerEnter()
	{
		animator.ResetAndPlay();
		cg.alpha = 1f;
		alpha = 1f;
		particle.Stop();
	}

	private void OnTriggerExit()
	{
		alpha = 0f;
		particle.Play();
	}
}
