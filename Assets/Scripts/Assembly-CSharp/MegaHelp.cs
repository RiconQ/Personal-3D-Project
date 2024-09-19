using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MegaHelp
{
	public static Color white = new Color(1f, 1f, 1f, 1f);

	public static Color black = new Color(0f, 0f, 0f, 1f);

	public static Color grey = new Color(0.25f, 0.25f, 0.25f, 1f);

	public static Vector3 ClosestPoint(Vector3 target, Vector3[] points)
	{
		Vector3 result = target;
		float num = float.PositiveInfinity;
		foreach (Vector3 vector in points)
		{
			float sqrMagnitude = (vector - target).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = vector;
			}
		}
		return result;
	}

	public static Transform ClosestTransform(Vector3 currentPos, Transform[] targets)
	{
		Transform result = null;
		float num = float.PositiveInfinity;
		foreach (Transform transform in targets)
		{
			float sqrMagnitude = (transform.position - currentPos).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = transform;
			}
		}
		return result;
	}

	public static Vector3 SnapVector3(Vector3 v, float snap = 1f)
	{
		v.x = Mathf.Round(v.x);
		v.y = Mathf.Round(v.y);
		v.z = Mathf.Round(v.z);
		return v;
	}

	public static Vector3 RandomVector3()
	{
		return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
	}

	public static Vector2 RandomVector2()
	{
		return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
	}

	public static Vector2[] Vector3ArrayToEdgeColliderPoints(Vector3[] v, bool lopped)
	{
		Vector2[] array = new Vector2[v.Length + (lopped ? 1 : 0)];
		for (int i = 0; i < v.Length; i++)
		{
			array[i] = v[i];
		}
		if (lopped)
		{
			array[v.Length] = v[0];
		}
		return array;
	}

	public static Vector2[] Vector3toVector2(Vector3[] v)
	{
		Vector2[] array = new Vector2[v.Length];
		for (int i = 0; i < v.Length; i++)
		{
			array[i] = v[i];
		}
		return array;
	}

	public static Vector3[] LocalToWorldArray(Transform t, Vector3[] points)
	{
		Vector3[] array = new Vector3[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			array[i] = t.TransformPoint(points[i]);
		}
		return array;
	}

	public static Vector3[] AngleDirections(Vector3 startDir, int count, float angle = 360f, bool centered = true)
	{
		Vector3[] array = new Vector3[count];
		Vector3 vector = startDir;
		for (int i = 0; i < count; i++)
		{
			if (i > 0)
			{
				vector = Quaternion.Euler(0f, 0f, angle / (float)count) * vector;
			}
			else if (centered)
			{
				vector = Quaternion.Euler(0f, 0f, (0f - angle) / (float)count) * vector;
			}
			array[i] = vector;
		}
		return array;
	}

	public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float num = 1f - t;
		float num2 = t * t;
		float num3 = num * num;
		float num4 = num3 * num;
		float num5 = num2 * t;
		return num4 * p0 + 3f * num3 * t * p1 + 3f * num * num2 * p2 + num5 * p3;
	}

	public static Vector3 BallisticTrajectory(Vector3 origin, Vector3 target, float timeToTarget, float gravityScale = 1f)
	{
		Vector3 vector;
		Vector3 vector2 = (vector = target - origin);
		vector.y = 0f;
		float y = vector2.y;
		float magnitude = vector.magnitude;
		float y2 = y / timeToTarget + 0.5f * ((0f - Physics2D.gravity.y) * gravityScale) * timeToTarget;
		float num = magnitude / timeToTarget;
		Vector3 normalized = vector.normalized;
		normalized *= num;
		normalized.y = y2;
		return normalized;
	}

	public static Vector3 BallisticTrajectory3D(Vector3 origin, Vector3 target, float timeToTarget, float gravity)
	{
		Vector3 vector;
		Vector3 vector2 = (vector = target - origin);
		vector.y = 0f;
		float y = vector2.y;
		float magnitude = vector.magnitude;
		float y2 = y / timeToTarget + 0.5f * (0f - gravity) * timeToTarget;
		float num = magnitude / timeToTarget;
		Vector3 normalized = vector.normalized;
		normalized *= num;
		normalized.y = y2;
		return normalized;
	}

	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{
		Vector3 lhs = linePoint2 - linePoint1;
		Vector3 rhs = Vector3.Cross(lineVec1, lineVec2);
		Vector3 lhs2 = Vector3.Cross(lhs, lineVec2);
		if (Mathf.Abs(Vector3.Dot(lhs, rhs)) < 0.0001f && rhs.sqrMagnitude > 0.0001f)
		{
			float num = Vector3.Dot(lhs2, rhs) / rhs.sqrMagnitude;
			intersection = linePoint1 + lineVec1 * num;
			return true;
		}
		intersection = Vector3.zero;
		return false;
	}

	public static bool SameSign(float num1, float num2)
	{
		if (!(num1 >= 0f) || !(num2 >= 0f))
		{
			if (num1 < 0f)
			{
				return num2 < 0f;
			}
			return false;
		}
		return true;
	}

	public static int IndexStep(int i, int max, int step = 1)
	{
		i = ((i + step <= max - 1) ? ((i + step >= 0) ? (i + step) : (max - 1)) : 0);
		return i;
	}

	public static int IndexStepClamped(int i, int max, int step = 1)
	{
		i = ((i + step > max - 1) ? (max - 1) : ((i + step >= 0) ? (i + step) : 0));
		return i;
	}

	public static int IndexBigStep(int i, int max, int step = 1)
	{
		int num = Mathf.Abs(step);
		int num2 = (int)Mathf.Sign(step);
		while (num > 0)
		{
			i = ((i + num2 <= max - 1) ? ((i + num2 >= 0) ? (i + num2) : (max - 1)) : 0);
			num--;
		}
		return i;
	}

	public static int[] IndicesRange(int a, int b, int max, int count)
	{
		int step = ((IndexBigStep(b, max, count - 1) != a) ? 1 : (-1));
		int[] array = new int[count];
		int num = a;
		for (int i = 0; i < count; i++)
		{
			array[i] = num;
			num = IndexStep(num, max, step);
		}
		return array;
	}

	public static IEnumerator FadeCanvasGroup(CanvasGroup cg, float alpha, float speed = 1f)
	{
		while (cg.alpha != alpha)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, Time.unscaledDeltaTime * speed);
			yield return null;
		}
	}

	public static void SpritesReset(SpriteRenderer[] rends, Color defaultColor = default(Color))
	{
		if (defaultColor == default(Color))
		{
			defaultColor = white;
		}
		for (int i = 0; i < rends.Length; i++)
		{
			rends[i].color = defaultColor;
		}
	}

	public static IEnumerator SpritesBlink(SpriteRenderer[] rends, Color blinkColor, float speed = 1f, Color defaultColor = default(Color))
	{
		if (defaultColor == default(Color))
		{
			defaultColor = white;
		}
		Color c = blinkColor;
		float t = 0f;
		while (t != 1f)
		{
			for (int i = 0; i < rends.Length; i++)
			{
				rends[i].color = c;
			}
			t = Mathf.MoveTowards(t, 1f, Time.deltaTime * speed);
			c = Color.Lerp(c, defaultColor, t);
			yield return null;
		}
		for (int j = 0; j < rends.Length; j++)
		{
			rends[j].color = defaultColor;
		}
	}

	public static IEnumerator SpritesFade(SpriteRenderer[] rends, float speed = 1f)
	{
		float a = 1f;
		while (a != 0f)
		{
			a = Mathf.MoveTowards(a, 0f, Time.deltaTime * speed);
			for (int i = 0; i < rends.Length; i++)
			{
				Color color = rends[i].color;
				color.a = a;
				rends[i].color = color;
			}
			yield return null;
		}
	}

	public static IEnumerator MaterialBlink(SpriteRenderer[] rends)
	{
		MaterialPropertyBlock pb = new MaterialPropertyBlock();
		Color c = Color.red;
		while (c != Color.black)
		{
			for (int i = 0; i < rends.Length; i++)
			{
				rends[i].GetPropertyBlock(pb);
				pb.SetColor("_BlinkColor", c);
				rends[i].SetPropertyBlock(pb);
			}
			c = Color.Lerp(c, Color.black, Time.deltaTime * 20f);
			yield return null;
		}
	}

	public static void LoadLevel(string levelName)
	{
		SceneManager.LoadScene(levelName);
	}

	public static Mesh CombineMeshes(Mesh[] meshes, bool mergeSubmeshes = true, Transform t = null)
	{
		List<CombineInstance> list = new List<CombineInstance>();
		for (int i = 0; i < meshes.Length; i++)
		{
			CombineInstance item = default(CombineInstance);
			item.mesh = meshes[i];
			if (t != null)
			{
				item.transform = t.worldToLocalMatrix;
			}
			list.Add(item);
		}
		Mesh mesh = new Mesh();
		mesh.CombineMeshes(list.ToArray(), mergeSubmeshes, t != null);
		return mesh;
	}
}
