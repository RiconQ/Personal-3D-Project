using UnityEngine;

public class QuickEffectsPool : MonoBehaviour
{
	public PrefabsArray array;

	public static PooledEffect[] effects;

	public static string[] names;

	private void Awake()
	{
		effects = new PooledEffect[array.prefabs.Length];
		names = new string[array.prefabs.Length];
		for (int i = 0; i < array.prefabs.Length; i++)
		{
			effects[i] = Object.Instantiate(array.prefabs[i], Vector3.zero, Quaternion.identity).GetComponent<PooledEffect>();
			names[i] = array.prefabs[i].name;
		}
	}

	public static PooledEffect Get(string name, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
	{
		for (int i = 0; i < effects.Length; i++)
		{
			if (names[i].Length == name.Length && names[i] == name)
			{
				if (position != default(Vector3))
				{
					effects[i].t.position = position;
				}
				if (rotation != default(Quaternion))
				{
					effects[i].t.rotation = rotation;
				}
				return effects[i];
			}
		}
		Debug.Log("There is no such an effect! " + name);
		return null;
	}
}
