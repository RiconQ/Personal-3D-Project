using UnityEngine;

[CreateAssetMenu(fileName = "PartingWords", menuName = "Data/PartingWords", order = 1)]
public class PartingWords : ScriptableObject
{
	public PartingWordsEntry[] entries;
}
