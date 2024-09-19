using UnityEngine;

public class PlaceOnWater : MonoBehaviour
{
	private Vector3 pos;

	private Vector3 startPos;

	private void Awake()
	{
		startPos = base.transform.position;
	}

	private void FixedUpdate()
	{
		pos = base.transform.position;
		pos.y = startPos.y + OceanScript.WaterHeightAtPoint(pos);
		base.transform.position = pos;
	}
}
