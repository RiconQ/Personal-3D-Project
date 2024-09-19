using System.Collections.Generic;
using UnityEngine;

public class JumperTrail : MonoBehaviour
{
	public Transform t;

	public LineRenderer line;

	public ParticleSystem particle;

	private Mesh m;

	private int segments = 20;

	private List<Vector3> points = new List<Vector3>(20);

	private Jumper jumper;

	private void Awake()
	{
		jumper = GetComponentInParent<Jumper>();
		Vector3 vector = MegaHelp.BallisticTrajectory3D(t.position, jumper.target, jumper.timeToTarget, -40f);
		List<Vector3> list = new List<Vector3>(segments);
		Vector3 vector2 = default(Vector3);
		for (int i = 0; i < segments; i++)
		{
			float num = (float)i * (jumper.timeToTarget / (float)segments);
			vector2.x = vector.x * num;
			vector2.y = vector.y * num - 20f * (num * num);
			vector2.z = vector.z * num;
			list.Add(base.transform.position + vector2);
			if (i == 1)
			{
				jumper.SetParticleDir(vector2);
			}
		}
		line.positionCount = segments;
		line.SetPositions(list.ToArray());
		line.enabled = false;
		t.SetParent(null);
		t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
	}

	private void Start()
	{
		m = new Mesh();
		line.BakeMesh(m, useTransform: true);
		ParticleSystem.ShapeModule shape = GetComponent<ParticleSystem>().shape;
		shape.mesh = m;
		particle.Play();
	}
}
