using UnityEngine;

public class SpawnBoat : MonoBehaviour
{
	public static SpawnBoat instance;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float targetHeight = 1f;

	public float targetSpeed = 10f;

	public Transform t;

	public Transform tMesh;

	public Transform tPivot;

	public AudioClip sfxPaddling1;

	public AudioClip sfxPaddling2;

	private float speed;

	private float height = 1.5f;

	private Vector3 pos;

	private Vector3 angles;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		pos += t.forward * (speed * Time.deltaTime);
		pos.y = OceanScript.WaterHeightAtPoint(pos.x, pos.z, global: true) + height;
		t.position = pos;
		speed = Mathf.Lerp(speed, targetSpeed, Time.deltaTime);
		height = Mathf.Lerp(height, targetHeight, Time.deltaTime);
		angles.x = Mathf.Sin(Time.time) * 8f;
		angles.y = 0f;
		angles.z = Mathf.Sin(Time.time * 2f) * 10f;
		tMesh.localEulerAngles = angles;
	}

	public void SetPosAndRot(Vector3 newPos, Vector3 target)
	{
		height = (targetHeight = 1.5f);
		speed = (targetSpeed = 10f);
		newPos.y = OceanScript.WaterHeightAtPoint(newPos.x, newPos.z, global: true) + height;
		t.position = (pos = newPos);
		t.rotation = Quaternion.LookRotation(newPos.DirTo(target.With(null, newPos.y)));
	}
}
