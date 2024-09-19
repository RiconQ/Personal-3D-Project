using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/New", order = 1)]
public class Weapon : ScriptableObject
{
	public GameObject prefabPickable;

	public GameObject prefabThrowed;

	public Mesh mesh;

	public int index;
}
