using UnityEngine;

[CreateAssetMenu(fileName = "Breakable Effect", menuName = "New Breakable Effect", order = 1)]
public class BreakableEffect : ScriptableObject
{
	public string effectName;

	public AudioClip[] sounds;

	public AudioClip[] damage;

	public AudioClip kickSound;

	public AudioClip creaking;
}
