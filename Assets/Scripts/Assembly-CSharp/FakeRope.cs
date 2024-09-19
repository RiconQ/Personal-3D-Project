using System;
using UnityEngine;

public class FakeRope : MonoBehaviour
{
	[Serializable]
	public class RopeSegment
	{
		public Vector3 dirNow;

		public Vector3 dirOld;

		public Vector3 posNow;

		public Vector3 posOld;

		public float timer;
	}

	public SwingRope chain;

	public Transform tTarget;

	public RopeRenderer rend;

	private float speed = 2f;

	private Vector3 dir;

	private Vector3 offset;

	private Transform t;

	private RopeSegment[] segments;

	private AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 8f), new Keyframe(1f, 1f, 0f, 0f));

	private void Awake()
	{
		t = base.transform;
		segments = new RopeSegment[rend.tBones.Length];
		for (int i = 0; i < segments.Length; i++)
		{
			segments[i] = new RopeSegment();
			segments[i].posNow = (segments[i].posOld = rend.tBones[i].position);
		}
	}

	private void LateUpdate()
	{
		if (chain.isActiveAndEnabled)
		{
			dir = t.position.DirTo(chain.joint.connectedBody.position + chain.joint.connectedAnchor);
			for (int i = 0; i < rend.tBones.Length; i++)
			{
				if (i > 0)
				{
					if (i < chain.closestPointIndex)
					{
						rend.tBones[i].position = rend.tBones[i - 1].position + dir;
					}
					else
					{
						rend.tBones[i].position = Vector3.Lerp(rend.tBones[i].position, rend.tBones[i - 1].position + Vector3.down, Time.deltaTime * 10f);
						offset = rend.tBones[i].position - rend.tBones[i - 1].position;
						if (offset.magnitude > 1f)
						{
							rend.tBones[i].position = rend.tBones[i - 1].position + offset.normalized;
						}
					}
				}
				if (i < rend.tBones.Length - 1)
				{
					rend.tBones[i].LookAt(rend.tBones[i + 1].position);
				}
				segments[i].dirNow = (segments[i].dirOld = dir);
				segments[i].timer = (float)(-i) / 10f;
			}
			return;
		}
		for (int j = 0; j < segments.Length; j++)
		{
			segments[j].posNow = (segments[j].posOld = rend.tBones[j].position);
		}
		for (int k = 0; k < segments.Length; k++)
		{
			if (segments[k].timer != 1f)
			{
				segments[k].timer = Mathf.MoveTowards(segments[k].timer, 1f, Time.deltaTime * speed);
				segments[k].dirNow = Vector3.LerpUnclamped(segments[k].dirOld, Vector3.down, curve.Evaluate(segments[k].timer));
				if (k > 0)
				{
					rend.tBones[k].position = rend.tBones[k - 1].position + segments[k].dirNow.normalized;
				}
			}
			if (k < rend.tBones.Length - 1)
			{
				rend.tBones[k].LookAt(rend.tBones[k + 1].position);
			}
		}
	}
}
