using UnityEngine;

public class ShadowCulling : MonoBehaviour
{
	public float cullDist = 30f;

	public float dist;

	public Transform t;

	public Projector projector;

	private void Update()
	{
		dist = Vector3.Distance(t.position, Game.player.tHead.position);
		if (projector.enabled != dist < cullDist)
		{
			projector.enabled = !projector.enabled;
		}
	}
}
