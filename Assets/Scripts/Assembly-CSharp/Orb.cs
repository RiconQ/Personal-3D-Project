using UnityEngine;

public class Orb : Door
{
	public GameObject orb;

	private void Awake()
	{
		orb.SetActive(value: false);
	}

	public override void Open()
	{
		base.Open();
		if ((bool)BackgroundMusic.instance)
		{
			BackgroundMusic.instance.Stop(instant: true);
		}
		orb.SetActive(value: true);
		QuickEffectsPool.Get("Summon FX END", orb.transform.position).Play();
	}
}
