using UnityEngine;

public class MainMenuLogo : MonoBehaviour
{
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float speed = 2f;

	public PerlinShake shake;

	public AudioClip clip;

	public Vector3 aPos = new Vector3(0f, 0f, 0f);

	public Vector3 bPos = new Vector3(0f, 0f, 6f);

	private Transform t;

	private float timer;

	private void Awake()
	{
		t = base.transform;
		Game.fading.speed = 0.1f;
	}

	private void OnEnable()
	{
		t.localPosition = aPos;
		timer = 0f;
		shake.Shake(1);
		Game.sounds.PlayClip(clip);
	}

	private void Update()
	{
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * speed);
			t.localPosition = Vector3.LerpUnclamped(aPos, bPos, curve.Evaluate(timer));
		}
	}
}
