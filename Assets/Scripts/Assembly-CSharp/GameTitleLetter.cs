using UnityEngine;

public class GameTitleLetter : MonoBehaviour
{
	public Transform t;

	public GameTitle gameTitle;

	public Vector3 posStart;

	public Vector3 posTarget;

	public Quaternion rotStart;

	public Quaternion rotTarget;

	public void Setup(GameTitle title)
	{
		t = base.transform;
		posTarget = (posStart = t.localPosition);
		rotTarget = (rotStart = t.localRotation);
		gameTitle = title;
	}

	public void Prepare(Vector3 pos, Quaternion rot)
	{
		t.localPosition = (posStart = posTarget + pos);
		t.localRotation = (rotStart = rot);
		base.gameObject.SetActive(value: false);
	}

	public void Tick(float time)
	{
		if (!base.gameObject.activeInHierarchy && time != 0f)
		{
			base.gameObject.SetActive(value: true);
		}
		t.localPosition = Vector3.LerpUnclamped(posStart, posTarget, gameTitle.curve.Evaluate(time));
		t.localRotation = Quaternion.SlerpUnclamped(rotStart, rotTarget, gameTitle.rotCurve.Evaluate(time));
	}
}
