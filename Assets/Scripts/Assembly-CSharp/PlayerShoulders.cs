using UnityEngine;

public class PlayerShoulders : MonoBehaviour
{
	[Header("Position")]
	public float posSpeed = 15f;

	public float yOffset = 0.06f;

	[Header("Rotation")]
	public float rotSpeed = 5f;

	public float zAngle = 6f;

	private PlayerController player;

	public Camera cam;

	private Transform t;

	private Vector3 pos;

	private Vector3 rot;

	private void Awake()
	{
		player = GetComponentInParent<PlayerController>();
		t = base.transform;
	}

	private void Update()
	{
		pos.y = Mathf.Lerp(pos.y, player.grounder.grounded ? 0f : Mathf.Clamp(0f - player.rb.velocity.y, 0f - yOffset, yOffset), Time.deltaTime * posSpeed);
		pos.z = Mathf.Lerp(pos.z, Mathf.Clamp(0f - player.v, 0f - yOffset, yOffset), Time.deltaTime * posSpeed);
		rot.z = Mathf.LerpAngle(rot.z, (0f - player.h) * zAngle, Time.deltaTime * rotSpeed);
		t.localPosition = pos;
		t.localEulerAngles = rot;
	}
}
