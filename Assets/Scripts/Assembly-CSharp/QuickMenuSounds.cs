using UnityEngine;

[CreateAssetMenu(fileName = "Menu Sounds", menuName = "New Menu Sounds", order = 1)]
public class QuickMenuSounds : ScriptableObject
{
	public AudioClip accept;

	public AudioClip back;

	public AudioClip next;

	public AudioClip nextItem;
}
