using System;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerFootsteps : MonoBehaviour
{
	public PlayerController p;

	public AudioSource source;

	public FootstepSurface[] surfaces;

	private int index;

	private int clipIndex;

	private float timer;

	private void Start()
	{
		Grounder grounder = p.grounder;
		grounder.OnColliderChanged = (Action)Delegate.Combine(grounder.OnColliderChanged, new Action(SwitchSurface));
	}

	private void OnDestroy()
	{
		Grounder grounder = p.grounder;
		grounder.OnColliderChanged = (Action)Delegate.Remove(grounder.OnColliderChanged, new Action(SwitchSurface));
	}

	public void SwitchSurface()
	{
		for (int i = 0; i < surfaces.Length; i++)
		{
			if (p.grounder.gCollider.CompareTag(surfaces[i].relatedTag))
			{
				index = i;
				return;
			}
		}
		index = 0;
	}

	public void PlayLandingSound()
	{
		Game.sounds.PlayClipAtPosition(surfaces[index].landing, 0.5f, p.t.position);
	}

	public void Tick()
	{
		timer += Time.deltaTime / p.speed;
		if (timer > 0.25f)
		{
			source.panStereo = ((source.panStereo > 0f) ? (-0.25f) : 0.25f);
			source.PlayClip(surfaces[index].sounds[clipIndex], 0.3f, UnityEngine.Random.Range(0.8f, 1.2f));
			timer = 0f;
			clipIndex = clipIndex.Next(surfaces[index].sounds.Length);
		}
	}
}
