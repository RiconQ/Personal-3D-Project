using UnityEngine;

[ExecuteInEditMode]
[SelectionBase]
public class QuickIK2 : MonoBehaviour
{
	public Transform a;

	public Transform b;

	public Transform c;

	public Transform tTarget;

	[SerializeField]
	private int sign = 1;

	[SerializeField]
	private float width = 1f;

	public float startOffset;

	public float targetOffset;

	public bool active;

	public bool cLockRotation;

	public bool debugLines;

	private float dist;

	private float height;

	private Vector3 midPoint;

	private Vector3 kneeDir;

	private Vector3 kneePos;

	private Vector3 startPos;

	private Vector3 targetPos;

	private Quaternion rot;

	public void LateUpdate()
	{
		if (!active)
		{
			return;
		}
		UpdateIK();
		if (debugLines)
		{
			if (startOffset != 0f)
			{
				Debug.DrawLine(startPos, a.position, Color.black);
			}
			Debug.DrawLine(startPos, kneePos, Color.green);
			Debug.DrawLine(kneePos, c.position, Color.green);
			if (dist > width * 2f)
			{
				Debug.DrawLine(c.position, tTarget.position, Color.black);
				Debug.DrawRay(midPoint, -kneeDir * 0.1f, Color.magenta);
			}
			else
			{
				Debug.DrawLine(midPoint, kneePos, Color.magenta);
			}
		}
	}

	public void Reset()
	{
		tTarget.localPosition = a.localPosition - a.parent.forward * width * 2f;
		tTarget.localEulerAngles = new Vector3(180f, 0f, 0f);
	}

	public void Setup()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		if (componentsInChildren.Length >= 2)
		{
			a = componentsInChildren[0];
			b = componentsInChildren[1];
			c = componentsInChildren[2];
		}
		if (!tTarget)
		{
			Transform transform = new GameObject($"{a.name} IK target").transform;
			transform.position = c.position;
			transform.rotation = c.rotation;
			if ((bool)a.parent)
			{
				transform.SetParent(a.parent);
			}
			tTarget = transform;
		}
		width = Vector3.Distance(a.position, c.position) / 2f;
		active = true;
	}

	public void UpdateIK()
	{
		targetPos = tTarget.position;
		if (targetOffset != 0f)
		{
			targetPos += tTarget.forward * targetOffset;
		}
		startPos = a.position;
		if (startOffset != 0f)
		{
			startPos += a.right * startOffset;
		}
		dist = Vector3.Distance(startPos, targetPos);
		bool flag = dist < width * 2f;
		if (dist < width * 2f)
		{
			height = Mathf.Sqrt(width * width - dist * dist / 4f);
			midPoint = (startPos + targetPos) / 2f;
		}
		else
		{
			height = 0f;
			midPoint = startPos + startPos.DirTo(targetPos).normalized * width;
		}
		kneeDir = Vector3.Cross(startPos - targetPos, -tTarget.right).normalized;
		kneePos = midPoint + kneeDir * (height * (float)sign);
		rot = Quaternion.LookRotation(-startPos.DirTo(kneePos), -kneeDir);
		a.rotation = rot;
		b.position = kneePos;
		c.position = (flag ? targetPos : (startPos + startPos.DirTo(targetPos) * (width * 2f)));
		c.rotation = (cLockRotation ? b.rotation : (flag ? tTarget.rotation : Quaternion.Slerp(tTarget.rotation, b.rotation, Mathf.Clamp01(dist - width * 2f))));
		rot = Quaternion.LookRotation(-b.position.DirTo(c.position), -kneeDir);
		b.rotation = rot;
	}
}
