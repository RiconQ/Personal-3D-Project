using UnityEngine;

public static class AudioSourceExtensions
{
	public static void PlayClip(this AudioSource source, AudioClip clip, float volume = -1f, float pitch = -1f)
	{
		if (source.isActiveAndEnabled)
		{
			source.volume = ((volume == -1f) ? 1f : volume);
			source.pitch = ((pitch == -1f) ? 1f : pitch);
			if (source.isPlaying)
			{
				source.Stop();
			}
			if (source.clip != clip)
			{
				source.clip = clip;
			}
			source.Play();
		}
	}
}
