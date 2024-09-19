using System.Collections;
using UnityEngine;

public class StoneDoor : Door, IKickable<Vector3>
{
	public Transform tMesh;

	public float openHeight = 5f;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private Vector3 pos;

	public void Kick(Vector3 dir)
	{
		opened = !opened;
		StartCoroutine(DoorMoving());
	}

	private IEnumerator DoorMoving()
	{
		float timer = 0f;
		while (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime);
			pos.y = Mathf.LerpUnclamped(0f, openHeight, curve.Evaluate(timer));
			tMesh.localPosition = pos;
			yield return null;
		}
	}
}
