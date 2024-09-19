using System;
using UnityEngine;

public class ThreatIndicator : MonoBehaviour
{
	private RectTransform t;

	private CanvasGroup cg;

	private float timer;

	private float angle;

	private float spriteAngle;

	private Vector3 dir;

	private Vector3 temp;

	public Transform target;

	private void Awake()
	{
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
	}

	public void Set(Transform newTarget)
	{
		target = newTarget;
		timer = 1f;
	}

	public void Tick(RectTransform tCanvas)
	{
		if (timer != 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime * 0.33333f);
			float num = Mathf.Sin(timer * (float)Math.PI);
			cg.alpha = num;
			if (timer == 0f)
			{
				target = null;
				return;
			}
			dir = Vector3.ProjectOnPlane(Game.player.tHead.InverseTransformDirection(target.position.DirTo(Game.player.t.position)), Vector3.up);
			angle = Mathf.Atan2(0f - dir.z, dir.x);
			temp.x = Mathf.Cos(angle) * (tCanvas.sizeDelta.x / 2f);
			temp.y = Mathf.Sin(angle) * (tCanvas.sizeDelta.y / 2f);
			temp.z = 0f;
			t.anchoredPosition3D = temp;
			temp.x = (temp.y = 0f);
			temp.z = t.anchoredPosition.Rotation2D().eulerAngles.z;
			t.localEulerAngles = temp;
			temp.x = (temp.y = (temp.z = num / 2f + 0.5f));
			t.localScale = temp;
		}
	}
}
