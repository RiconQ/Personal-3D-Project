using UnityEngine;

public class DangerFlare : MonoBehaviour
{
	public Transform t;

	private Vector3 scale;

	private float dist;

	private void Update()
	{
		dist = Vector3.Distance(Game.player.t.position, t.position);
		scale.x = (scale.y = (scale.z = Mathf.Clamp(dist / 6f, 1f, float.PositiveInfinity)));
		t.localScale = scale;
	}
}
