using System;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
	public LayerMask mask;

	private Vector4 damageInfo;

	private float timer;

	private float delay;

	private Transform t;

	private Transform tMesh;

	private Collider clldr;

	private void OnEnable()
	{
		timer = 0f;
		delay = 0f;
		clldr.enabled = false;
	}

	private void Awake()
	{
		t = base.transform;
		tMesh = t.Find("Mesh").transform;
		clldr = GetComponent<Collider>();
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (delay != 1f)
		{
			delay = Mathf.MoveTowards(delay, 1f, Time.deltaTime);
			if (delay == 1f)
			{
				clldr.enabled = true;
			}
		}
		timer = Mathf.MoveTowards(timer, 10f, Time.deltaTime);
		if (timer == 10f)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		HandleTriggerEvents(c);
	}

	private void OnTriggerStay(Collider c)
	{
		HandleTriggerEvents(c);
	}

	private void HandleTriggerEvents(Collider c)
	{
		if ((int)mask == ((int)mask | (1 << c.gameObject.layer)))
		{
			damageInfo = Vector3.up;
			damageInfo.w = 50f;
			c.GetComponent<IDamageable<Vector4>>().Damage(damageInfo);
			base.gameObject.SetActive(value: false);
		}
	}
}
