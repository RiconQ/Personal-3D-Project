using System;
using UnityEngine;

public class TheSkullDagger : MonoBehaviour
{
	public Transform tPivot;

	public Transform tDagger;

	public ParticleSystem ps;

	public AudioClip sfxPull;

	public AudioClip sfxPulled;

	public AudioSource source;

	private int state;

	private void Awake()
	{
		TheSkull.OnEvent = (Action)Delegate.Combine(TheSkull.OnEvent, new Action(PullItOut));
	}

	private void OnDestroy()
	{
		TheSkull.OnEvent = (Action)Delegate.Remove(TheSkull.OnEvent, new Action(PullItOut));
	}

	private void PullItOut()
	{
		switch (state)
		{
		case 0:
			tDagger.gameObject.SetActive(value: true);
			break;
		case 1:
			ps.Play();
			tDagger.Translate(0f, 0.1f, 0f, Space.Self);
			source.PlayClip(sfxPull);
			break;
		case 2:
			ps.Play();
			tDagger.Translate(0f, 0.05f, 0f, Space.Self);
			source.PlayClip(sfxPull);
			break;
		case 3:
			Game.time.SlowMotion(0.5f, 0.3f, 0.05f);
			tDagger.gameObject.SetActive(value: false);
			ps.Play();
			source.PlayClip(sfxPulled);
			break;
		}
		state++;
	}
}
