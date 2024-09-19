using UnityEngine;

[CreateAssetMenu(fileName = "KnockedEnemySounds", menuName = "Enemies/Knocked Enemy Sounds", order = 1)]
public class BodySounds : ScriptableObject
{
	public AudioClip implied;

	public AudioClip dash;

	public AudioClip[] impacts;

	public AudioClip[] kick;
}
