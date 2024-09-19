using UnityEngine;

public class LockedPortalPath : MonoBehaviour
{
	public ParticleSystem Particle;

	public LineRenderer Line;

	public void Setup(Vector3 aPos, Vector3 bPos)
	{
		int num = 15;
		Line.useWorldSpace = true;
		Line.positionCount = num;
		Vector3 vector = aPos;
		Vector3 vector2 = bPos;
		Vector3 vector3 = (vector + vector2) / 2f;
		vector3.y = ((vector.y > vector2.y) ? vector.y : vector2.y);
		float num2 = Vector3.Distance(vector, vector2);
		Vector3 p = vector + vector.DirTo(vector3 + Vector3.up * num2 / 2f).normalized * num2 / 2f;
		Vector3 p2 = vector2 + vector2.DirTo(vector3 + Vector3.up * num2 / 2f).normalized * num2 / 2f;
		for (int i = 0; i < num; i++)
		{
			Line.SetPosition(i, MegaHelp.CalculateCubicBezierPoint((float)i / (float)(num - 1), vector, p, p2, vector2));
		}
		Invoke("SetupParticle", Time.deltaTime);
	}

	public void SetupParticle()
	{
		Mesh mesh = new Mesh();
		Line.BakeMesh(mesh, useTransform: true);
		ParticleSystem.ShapeModule shape = Particle.shape;
		shape.mesh = mesh;
		Line.enabled = false;
	}
}
