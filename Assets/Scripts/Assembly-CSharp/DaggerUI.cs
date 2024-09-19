using UnityEngine;

public class DaggerUI : MonoBehaviour
{
	private CanvasGroup cg;

	public Animation anim;

	private float alpha;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		int num = (Game.player.weapons.daggerController.isCooling ? 1 : 0);
		if (alpha != (float)num)
		{
			alpha = num;
			if (alpha == 1f)
			{
				anim.Play();
			}
		}
		if (cg.alpha != alpha)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.deltaTime * 3f);
		}
	}
}
