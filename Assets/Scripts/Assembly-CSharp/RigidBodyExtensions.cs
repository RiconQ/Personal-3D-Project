using UnityEngine;

public static class RigidBodyExtensions
{
	public static void AddForceAndTorque(this Rigidbody rb, Vector3 force, Vector3 torque, ForceMode mode = ForceMode.Impulse)
	{
		rb.AddForce(force, mode);
		rb.AddTorque(torque, mode);
	}

	public static Vector3 AddBallisticForce(this Rigidbody rb, Vector3 target, float time, float gravity, bool resetVelocity = false)
	{
		Vector3 vector;
		Vector3 vector2 = (vector = target - rb.position);
		vector.y = 0f;
		float y = vector2.y;
		float magnitude = vector.magnitude;
		float y2 = y / time + 0.5f * (0f - gravity) * time;
		float num = magnitude / time;
		Vector3 normalized = vector.normalized;
		normalized *= num;
		normalized.y = y2;
		if (!resetVelocity)
		{
			rb.AddForce(normalized, ForceMode.VelocityChange);
		}
		else
		{
			rb.velocity = normalized;
		}
		return normalized;
	}
}
