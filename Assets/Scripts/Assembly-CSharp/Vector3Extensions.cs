using UnityEngine;

public static class Vector3Extensions
{
	public static Vector3 Random(this Vector3 v)
	{
		v.x = UnityEngine.Random.Range(-1f, 1f);
		v.y = UnityEngine.Random.Range(-1f, 1f);
		v.z = UnityEngine.Random.Range(-1f, 1f);
		return v.normalized;
	}

	public static Vector3 CardinalDirection(this Vector3 v)
	{
		v.y = 0f;
		v.Normalize();
		if (v.x.Abs() > v.z.Abs())
		{
			v.z = 0f;
			v.x = v.x.Sign();
		}
		else
		{
			v.x = 0f;
			v.z = v.z.Sign();
		}
		return v;
	}

	public static Vector3 Snap(this Vector3 v)
	{
		v.x = Mathf.Round(v.x);
		v.y = Mathf.Round(v.y);
		v.z = Mathf.Round(v.z);
		return v;
	}

	public static Vector3Int SnapToInt(this Vector3 v, int step = 6)
	{
		Vector3Int result = default(Vector3Int);
		result.x = Mathf.RoundToInt(v.x / (float)step) * step;
		result.y = Mathf.RoundToInt(v.y / (float)step) * step;
		result.z = Mathf.RoundToInt(v.z / (float)step) * step;
		return result;
	}

	public static Vector3 Snap(this Vector3 v, float step = 1f)
	{
		v.x = Mathf.Round(v.x / step) * step;
		v.y = Mathf.Round(v.y / step) * step;
		v.z = Mathf.Round(v.z / step) * step;
		return v;
	}

	public static Vector3 DirTo(this Vector3 v, Vector3 target)
	{
		return (target - v).normalized;
	}

	public static Vector3 DirToXZ(this Vector3 v, Vector3 target)
	{
		target.x -= v.x;
		target.z -= v.z;
		target.y = 0f;
		return target.normalized;
	}

	public static Vector3 ClosestToMe(this Vector3 v, Vector3[] points)
	{
		Vector3 result = default(Vector3);
		float num = float.PositiveInfinity;
		foreach (Vector3 vector in points)
		{
			float sqrMagnitude = (vector - v).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = vector;
			}
		}
		return result;
	}

	public static Vector3 ClosestPointOnLine(this Vector3 v, Vector3 a, Vector3 b)
	{
		Vector3 vector = b - a;
		float magnitude = vector.magnitude;
		vector.Normalize();
		float value = Vector3.Dot(v - a, vector);
		value = Mathf.Clamp(value, 0f, magnitude);
		return a + vector * value;
	}

	public static float PerlinNoise3D(this Vector3 v, float scale = 1f, int seed = 0, int octave = 1)
	{
		float num = 0f;
		Vector3 vector = default(Vector3);
		float num2 = Mathf.PerlinNoise((float)seed / 1000f, 0f) * 1000f;
		float num3 = Mathf.PerlinNoise(1f, (float)seed / 1000f) * 1000f;
		float num4 = Mathf.PerlinNoise((float)(-seed) / 1000f, 1f) * 1000f;
		for (int i = 0; i < octave; i++)
		{
			vector.x = (v.x + num2) * scale;
			vector.y = (v.y + num3) * scale;
			vector.z = (v.z + num4) * scale;
			num += (Mathf.PerlinNoise(vector.x, vector.y) + Mathf.PerlinNoise(vector.y, vector.z) + Mathf.PerlinNoise(vector.z, vector.x)) / 3f;
			scale /= 2f;
		}
		return num / (float)octave;
	}

	public static float PerlinNoiseFromPosition(this Vector3 v, int seed, float frequency = 0.1f, float amplitude = 1f, float persistence = 1f, int octave = 1)
	{
		float num = 0f;
		for (int i = 0; i < octave; i++)
		{
			float num2 = Mathf.PerlinNoise(v.x * frequency + (float)seed, v.y * frequency + (float)seed) * amplitude;
			float num3 = Mathf.PerlinNoise(v.x * frequency + (float)seed, v.z * frequency + (float)seed) * amplitude;
			float num4 = Mathf.PerlinNoise(v.y * frequency + (float)seed, v.z * frequency + (float)seed) * amplitude;
			float num5 = Mathf.PerlinNoise(v.y * frequency + (float)seed, v.x * frequency + (float)seed) * amplitude;
			float num6 = Mathf.PerlinNoise(v.z * frequency + (float)seed, v.x * frequency + (float)seed) * amplitude;
			float num7 = Mathf.PerlinNoise(v.z * frequency + (float)seed, v.y * frequency + (float)seed) * amplitude;
			num += (num2 + num3 + num4 + num5 + num6 + num7) / 6f;
			amplitude *= persistence;
			frequency *= 2f;
		}
		return num / (float)octave;
	}

	public static int SideFromDirection(this Vector3 fwd, Vector3 dir, Vector3 normal)
	{
		float num = Vector3.Dot(Vector3.Cross(fwd, dir), normal);
		if (num > 0f)
		{
			return 1;
		}
		if (num < 0f)
		{
			return -1;
		}
		return 0;
	}

	public static int IndexOfClosestToMe(this Vector3 v, Vector3[] points)
	{
		int result = 0;
		float num = float.PositiveInfinity;
		for (int i = 0; i < points.Length; i++)
		{
			float sqrMagnitude = (points[i] - v).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = i;
			}
		}
		return result;
	}

	public static Vector3 Reset(this Vector3 v, float? x = null, float? y = null, float? z = null)
	{
		v.x = x.GetValueOrDefault();
		v.y = y.GetValueOrDefault();
		v.z = z.GetValueOrDefault();
		return v;
	}

	public static Vector4 Reset(this Vector4 v, float? x = null, float? y = null, float? z = null, float? w = null)
	{
		v.x = x.GetValueOrDefault();
		v.y = y.GetValueOrDefault();
		v.z = z.GetValueOrDefault();
		v.w = w.GetValueOrDefault();
		return v;
	}

	public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
	{
		return new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
	}

	public static Vector4 With(this Vector4 v, float? x = null, float? y = null, float? z = null, float? w = null)
	{
		v.x = x ?? v.x;
		v.y = y ?? v.y;
		v.z = z ?? v.z;
		v.w = w ?? v.w;
		return v;
	}

	public static Vector2 With(this Vector2 v, float? x = null, float? y = null)
	{
		return new Vector2(x ?? v.x, y ?? v.y);
	}

	public static Vector2 Cross(this Vector2 v)
	{
		v = new Vector2(v.y, 0f - v.x);
		return v;
	}

	public static Vector3 Mid(this Vector3 a, Vector3 b, float distance = 0.5f)
	{
		return (a + b) * distance;
	}

	public static Vector2 Mid(this Vector2 a, Vector2 b, float distance = 0.5f)
	{
		return (a + b) * distance;
	}

	public static Vector3 RotateVector(this Vector3 v, float? x = null, float? y = null, float? z = null)
	{
		v = Quaternion.Euler(x.GetValueOrDefault(), y.GetValueOrDefault(), z.GetValueOrDefault()) * v;
		return v;
	}

	public static Quaternion Rotation2D(this Vector2 vector)
	{
		return Quaternion.AngleAxis(Mathf.Atan2(vector.y, vector.x) * 57.29578f, Vector3.forward);
	}

	public static Quaternion Rotate(this Quaternion quaternion, Quaternion target, float speed = 1f)
	{
		quaternion = Quaternion.RotateTowards(quaternion, target, Time.deltaTime * speed * 360f);
		return quaternion;
	}

	public static Quaternion Quaternionise(this Vector3 v)
	{
		return Quaternion.LookRotation(v);
	}

	public static Vector2 WorldToScreenSpace(this Vector3 v, Camera cam, RectTransform area)
	{
		Vector3 vector = cam.WorldToScreenPoint(v);
		vector.z = 0f;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(area, vector, cam, out var localPoint))
		{
			return localPoint;
		}
		return vector;
	}
}
