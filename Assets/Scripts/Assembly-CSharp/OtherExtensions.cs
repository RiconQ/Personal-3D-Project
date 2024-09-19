using UnityEngine;

public static class OtherExtensions
{
	public static AudioClip GetRandom(this AudioClip[] clips)
	{
		return clips[Random.Range(0, clips.Length)];
	}

	public static void PlayNow(this Animation anim)
	{
		if (anim.isPlaying)
		{
			anim.Stop();
		}
		anim.Play();
	}
}
