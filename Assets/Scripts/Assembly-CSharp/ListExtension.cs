using System.Collections.Generic;

public static class ListExtension
{
	public static void AddElseRemove<T>(this List<T> list, T item)
	{
		if (list.Contains(item))
		{
			list.Remove(item);
		}
		else
		{
			list.Add(item);
		}
	}
}
