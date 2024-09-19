using System;
using UnityEngine;

[ExecuteInEditMode]
public class ChainBarrier : MonoBehaviour
{
	public static ChainBarrier instance;

	public ParticleSystem lockedParticle;

	public Transform t;

	public float speed;

	public Material mat;

	public Collider[] clldrs;

	public Transform[] chains;

	public float[] speeds;

	private void Start()
	{
		instance = this;
		t = base.transform;
		mat.SetVector("_Center", t.position);
		speeds = new float[chains.Length];
		for (int i = 0; i < speeds.Length; i++)
		{
			speeds[i] = speed * UnityEngine.Random.Range(0.5f, 1.5f);
		}
		lockedParticle.transform.SetParent(null);
		ArenaSpawner.OnReachedNumber = (Action)Delegate.Combine(ArenaSpawner.OnReachedNumber, new Action(Break));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		ArenaSpawner.OnReachedNumber = (Action)Delegate.Remove(ArenaSpawner.OnReachedNumber, new Action(Break));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Break()
	{
		Collider[] array = clldrs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		Transform[] array2 = chains;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].gameObject.SetActive(value: false);
		}
		GetComponent<ParticleSystem>().Play();
		CameraController.shake.Shake();
		QuickEffectsPool.Get("Orb Explosion", base.transform.position).Play();
	}

	private void Reset()
	{
		Collider[] array = clldrs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		Transform[] array2 = chains;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].gameObject.SetActive(value: true);
		}
		GetComponent<ParticleSystem>().Clear();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 9)
		{
			lockedParticle.transform.position = other.transform.position;
			lockedParticle.Play();
			lockedParticle.GetComponent<AudioSource>().Play();
			if (Game.player.grounder.grounded)
			{
				Game.player.grounder.Ungrounded(forced: true);
			}
			Game.player.slide.Interrupt();
			Game.player.airControlBlock = 0.2f;
			Game.player.rb.velocity = (t.forward + Vector3.up).normalized * 20f;
			CameraController.shake.Shake();
		}
	}

	private void Update()
	{
		for (int i = 0; i < chains.Length; i++)
		{
			chains[i].Rotate(0f, 0f, speeds[i] * Mathf.PerlinNoise(chains[i].position.x, chains[i].position.y) * Time.deltaTime, Space.Self);
		}
	}
}
