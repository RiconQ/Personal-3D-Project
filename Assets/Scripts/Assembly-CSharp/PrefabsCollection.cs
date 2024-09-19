using UnityEngine;

[CreateAssetMenu(fileName = "Prefabs Collection", menuName = "New Prefabs Collection", order = 1)]
public class PrefabsCollection : ScriptableObject
{
	public GameObject[] prefabs;
}
