using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CamTargetTrigger : MonoBehaviour
{
	[SerializeField]
	private float targetFOV = 70f;

	private Transform t;

	[SerializeField]
	private Transform tTarget;

	[SerializeField]
	private AnimationCurve curve;

	private PlayerController player;

	private BoxCollider clldr;

	public Vector3 offset;

	[TextArea]
	public string headerText = "";

	public UnityEvent extraEvent;

	private bool activated;

	private void Awake()
	{
		clldr = GetComponent<BoxCollider>();
	}

	private void OnTriggerEnter()
	{
		clldr.enabled = false;
		StartCoroutine(ShowingTarget());
	}

	private void Update()
	{
		if (activated)
		{
			t.rotation = Quaternion.Lerp(t.rotation, Quaternion.LookRotation(t.position.DirTo(tTarget.position + offset)), Time.unscaledDeltaTime * 4f);
			player.mouseLook.rotation.x = t.eulerAngles.x;
			player.mouseLook.rotation.y = t.eulerAngles.y;
			if (player.mouseLook.rotation.x > 90f)
			{
				player.mouseLook.rotation.x -= 360f;
			}
			else if (player.mouseLook.rotation.x < -90f)
			{
				player.mouseLook.rotation.x += 360f;
			}
		}
	}

	private IEnumerator ShowingTarget()
	{
		activated = true;
		extraEvent.Invoke();
		player = PlayerController.instance;
		player.Deactivate();
		t = player.tHead;
		Game.wideMode.Show();
		Game.time.SlowMotion(0.1f, 100f);
		float timer2 = 0f;
		float startFOV = Camera.main.fieldOfView;
		while (timer2 != 4f)
		{
			timer2 = Mathf.MoveTowards(timer2, 4f, Time.unscaledDeltaTime);
			Camera.main.fieldOfView = Mathf.LerpUnclamped(startFOV, targetFOV, curve.Evaluate(timer2));
			yield return null;
		}
		yield return new WaitForSeconds(0.1f);
		activated = false;
		player.Activate();
		Game.time.StopSlowmo();
		Game.wideMode.Hide();
		timer2 = 0f;
		while (timer2 != 1f)
		{
			timer2 = Mathf.MoveTowards(timer2, 1f, Time.unscaledDeltaTime * 2f);
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, startFOV, timer2);
			yield return null;
		}
		player.mouseLook.enabled = true;
		base.gameObject.SetActive(value: false);
	}
}
