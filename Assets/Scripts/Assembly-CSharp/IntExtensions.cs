using UnityEngine;

public static class IntExtensions
{
	public static bool SameSign(this int a, int b)
	{
		if (a < 0 || b < 0)
		{
			if (a < 0)
			{
				return b < 0;
			}
			return false;
		}
		return true;
	}

	public static bool Inside(this int i, int count)
	{
		if (i >= 0)
		{
			return i < count;
		}
		return false;
	}

	public static int Abs(this int i)
	{
		if (i < 0)
		{
			return -i;
		}
		return i;
	}

	public static int Sign(this int i)
	{
		if (i < 0)
		{
			return -1;
		}
		return 1;
	}

	public static int RandomUpTo(this int min, int max)
	{
		return Random.Range(min, max);
	}

	public static int Next(this int i, int max, int sign = 1)
	{
		i += sign;
		if (i < 0)
		{
			return max - 1;
		}
		if (i >= max)
		{
			return 0;
		}
		return i;
	}

	public static int NextClamped(this int i, int max, int sign = 1)
	{
		i = Mathf.Clamp(i + sign, 0, max - 1);
		return i;
	}

	public static int RowIndex(this int i, int rowsCount)
	{
		return i - Mathf.FloorToInt((float)i / (float)rowsCount) * rowsCount;
	}

	public static int ColumnIndex(this int i, int columnsCount)
	{
		return 1 + Mathf.FloorToInt((float)i / (float)columnsCount);
	}

	public static bool SetIfLower(this ref int i, int target)
	{
		if (i < target)
		{
			i = target;
			return true;
		}
		return false;
	}

	public static bool SetIfHigher(this ref int i, int target)
	{
		if (i > target)
		{
			i = target;
			return true;
		}
		return false;
	}

	public static int PercentOf(this int i, int count)
	{
		if (count != 0)
		{
			return Mathf.RoundToInt((float)i / (float)count * 100f);
		}
		return 0;
	}
}
