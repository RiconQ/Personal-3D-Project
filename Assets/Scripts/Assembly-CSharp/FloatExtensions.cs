using System;
using UnityEngine;

public static class FloatExtensions
{
	public static bool SameSign(this float a, float b)
	{
		if (!(a >= 0f) || !(b >= 0f))
		{
			if (a < 0f)
			{
				return b < 0f;
			}
			return false;
		}
		return true;
	}

	public static int Sign(this float i)
	{
		if (!(i >= 0f))
		{
			return -1;
		}
		return 1;
	}

	public static float RandomUpTo(this float min, float max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static float Clamp(this float i, float max)
	{
		if (i > max)
		{
			i = max;
		}
		else if (i < 0f - max)
		{
			i = 0f - max;
		}
		return i;
	}

	public static float Abs(this float i)
	{
		if (!(i < 0f))
		{
			return i;
		}
		return 0f - i;
	}

	public static bool InRange(this float i, float min, float max)
	{
		return i > min && i < max;
	}

	public static float WrapAngle(this float angle)
	{
		angle %= 360f;
		if (angle > 180f)
		{
			return angle - 360f;
		}
		return angle;
	}

	public static float UnwrapAngle(this float angle)
	{
		if (angle >= 0f)
		{
			return angle;
		}
		angle = (0f - angle) % 360f;
		return 360f - angle;
	}

	public static float InDegrees(this float normal)
	{
		return 57.29578f * Mathf.Acos(Mathf.Clamp(normal, -1f, 1f));
	}

	public static float FromDegress(this float angle)
	{
		return Mathf.Cos(angle * ((float)Math.PI / 180f));
	}

	public static bool MoveTowards(this ref float i, float target, float speed = 1f)
	{
		i = Mathf.MoveTowards(i, target, Time.deltaTime * speed);
		return i != target;
	}

	public static bool MoveTowardsUnscaled(this ref float i, float target, float speed = 1f)
	{
		i = Mathf.MoveTowards(i, target, Time.unscaledDeltaTime * speed);
		return i != target;
	}

	public static bool MoveTowardsFixed(this ref float i, float target, float speed = 1f)
	{
		i = Mathf.MoveTowards(i, target, Time.fixedDeltaTime * speed);
		return i != target;
	}

	public static bool SetIfLower(this ref float i, float target)
	{
		if (i < target)
		{
			i = target;
			return true;
		}
		return false;
	}

	public static bool SetIfHigher(this ref float i, float target)
	{
		if (i > target)
		{
			i = target;
			return true;
		}
		return false;
	}
}
