using UnityEngine;

public static class MyRandom
{
	public static Vector3 temp;

	public static float Range(float min, float max)
	{
		return Random.Range(min, max);
	}

	public static int Sign()
	{
		if (!(Random.Range(-1f, 1f) > 0f))
		{
			return -1;
		}
		return 1;
	}

	public static Vector3 DirXZ(float mgt = 1f)
	{
		temp.x = Random.Range(-1f, 1f);
		temp.z = Random.Range(-1f, 1f);
		temp.y = 0f;
		temp.Normalize();
		temp *= Random.Range(0f, mgt);
		return temp;
	}
}
