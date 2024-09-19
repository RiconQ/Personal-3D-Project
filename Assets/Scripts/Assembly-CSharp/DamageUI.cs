using System;
using UnityEngine;
using UnityEngine.UI;

public class DamageUI : MonoBehaviour
{
	public static DamageUI instance;

	public RectTransform tArea;

	public RectTransform t;

	public CanvasGroup cg;

	public CanvasScaler scaler;

	public Material mat;

	public RectTransform[] tSplash;

	private int index;

	private float splashAngle;

	private float angle;

	private Vector3 dir;

	private void Awake()
	{
		instance = this;
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		scaler = GetComponentInParent<CanvasScaler>();
		PlayerController.OnDamage = (Action<Vector3>)Delegate.Combine(PlayerController.OnDamage, new Action<Vector3>(SetDir));
	}

	private void OnDestroy()
	{
		PlayerController.OnDamage = (Action<Vector3>)Delegate.Remove(PlayerController.OnDamage, new Action<Vector3>(SetDir));
	}

	public static float CalculateAngle(Vector3 from, Vector3 to)
	{
		return Quaternion.FromToRotation(PlayerController.instance.tHead.up, to - from).eulerAngles.z;
	}

	private void Update()
	{
		if ((bool)PlayerController.instance)
		{
			angle = CalculateAngle(PlayerController.instance.tHead.forward, dir);
			angle /= 90f;
			angle *= (float)Math.PI;
			mat.SetFloat("_Angle", angle);
		}
	}

	public void SetDir(Vector3 dmg)
	{
		dir = Vector3.ProjectOnPlane(Game.player.tHead.InverseTransformDirection(dmg), Vector3.up);
		float f = Mathf.Atan2(0f - dir.z, dir.x);
		float x = Mathf.Cos(f) * (tArea.sizeDelta.x / 2f);
		float y = Mathf.Sin(f) * (tArea.sizeDelta.y / 2f);
		tSplash[index].anchoredPosition3D = new Vector3(x, y, 0f);
		splashAngle = tSplash[index].anchoredPosition.Rotation2D().eulerAngles.z;
		tSplash[index].localEulerAngles = new Vector3(0f, 0f, splashAngle);
		tSplash[index].GetComponent<Animation>().Play();
		index = index.Next(tSplash.Length);
	}
}
