using System;
using System.Collections;
using UnityEngine;

public class RankLetter : MonoBehaviour
{
	public Transform tCamera;

	public Transform tLetter;

	public Mesh[] rankMeshes;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private MeshFilter filter;

	private MeshRenderer rend;

	private MaterialPropertyBlock block;

	public float value;

	public float blink;

	private int index;

	private float timer = 1f;

	private Vector3 pos;

	private Vector3 angles;

	private void Awake()
	{
		filter = tLetter.GetComponentInChildren<MeshFilter>();
		rend = tLetter.GetComponentInChildren<MeshRenderer>();
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
		filter.sharedMesh = rankMeshes[0];
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		SetRankLetter(0);
		value = 0f;
	}

	public void SetRankLetter(int i)
	{
		if (i < rankMeshes.Length && i > -1)
		{
			filter.sharedMesh = rankMeshes[i];
		}
		if (i != index && index < rankMeshes.Length && i > 0)
		{
			blink = 1f;
		}
		index = i;
	}

	private IEnumerator Switching(int sign)
	{
		Vector3 posA = tLetter.localPosition;
		Vector3 posB = new Vector3(0f, 0f, (sign == 1) ? 8 : 7);
		Quaternion rot = Quaternion.Euler(-90f, 90 * sign, 0f);
		timer = 0f;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 2f);
			tLetter.localPosition = Vector3.LerpUnclamped(posA, posB, curve.Evaluate(timer));
			tLetter.localRotation = Quaternion.SlerpUnclamped(rot, Quaternion.Euler(-90f, (sign == 1) ? 5 : 40, 0f), curve.Evaluate(timer));
			yield return null;
		}
	}

	private void Update()
	{
		block.SetFloat("_Fill", value * 3f);
		block.SetFloat("_Blink", blink);
		rend.SetPropertyBlock(block);
		blink = Mathf.MoveTowards(blink, 0f, Time.deltaTime);
		if (timer == 1f)
		{
			angles.x = -90f;
			angles.y = Mathf.LerpAngle(angles.y, Mathf.Lerp(10f, 40f, value), Time.deltaTime * 10f);
			angles.z = 0f;
			tLetter.localEulerAngles = angles;
			pos.x = 0f;
			pos.y = 0f;
			pos.z = Mathf.Lerp(8f, 7f, value);
			tLetter.localPosition = pos;
		}
	}
}
