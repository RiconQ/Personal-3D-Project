using UnityEngine;

public class SimpleFlare : MonoBehaviour
{
	public ParticleSystem.MainModule modMain;

	public ParticleSystem.TextureSheetAnimationModule modSheet;

	public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private bool inactive;

	private float dist;

	private Vector3 scale;

	private Transform tTarget;

	private Light myLight;

	private float timer;

	public Transform t { get; private set; }

	public ParticleSystem particle { get; private set; }

	private void Awake()
	{
		t = base.transform;
		particle = GetComponentInChildren<ParticleSystem>();
		myLight = GetComponent<Light>();
		modSheet = particle.textureSheetAnimation;
		modMain = particle.main;
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		particle.Clear();
	}

	public void Tick(Transform target, bool value)
	{
		if (inactive != value)
		{
			timer = 0f;
			inactive = value;
			particle.Clear();
			particle.Play();
			modMain.startColor = (inactive ? Color.green : Color.red);
			myLight.color = (inactive ? (Color.green / 2f) : Color.red);
		}
		if (tTarget != target)
		{
			tTarget = target;
			timer = 0f;
			return;
		}
		t.position = tTarget.position;
		dist = Vector3.Distance(Game.player.t.position, t.position);
		scale.x = (scale.y = (scale.z = Mathf.Clamp(dist / 6f, 1f, float.PositiveInfinity)));
		t.localScale = scale;
		if (timer != 1f)
		{
			timer = Mathf.MoveTowards(timer, 1f, Time.deltaTime * 3f);
			myLight.intensity = Mathf.LerpUnclamped(0f, 2f, curve.Evaluate(timer));
			myLight.range = Mathf.LerpUnclamped(0f, 1f, curve.Evaluate(timer));
		}
	}

	public void Switch(bool value)
	{
		if (inactive != value)
		{
			inactive = value;
			particle.Clear();
			particle.Play();
			modMain.startColor = (inactive ? Color.green : Color.red);
		}
	}
}
