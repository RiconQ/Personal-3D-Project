using UnityEngine;

[CreateAssetMenu(fileName = "Footstep Surface", menuName = "New Footstep Surface", order = 1)]
public class FootstepSurface : ScriptableObject
{
	public string relatedTag;

	public AudioClip[] sounds;

	public AudioClip landing;
}
