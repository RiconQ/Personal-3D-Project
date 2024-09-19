using System;
using System.Collections;
using UnityEngine;

public class WeakDoor : Door, IKickable<Vector3>, IDamageable<DamageData>
{
	private Transform t;

	private Rigidbody rb;

	private Collider clldr;

	public AudioClip koSound;

	public AudioClip breakSound;

	public AudioSource loopSource;

	private Vector3 targetPos;

	private Vector3 startPos;

	private Vector3 pos;

	private Vector3 dir;

	private Quaternion startRotation;

	private MeshFilter mf;

	private ParticleSystem.ShapeModule shape;

	private DamageData damage = new DamageData();

	private RaycastHit hit;

	public void Kick(Vector3 d)
	{
		if (rb.isKinematic)
		{
			StartCoroutine(Knocking(d));
			dir = d;
		}
	}

	public void Damage(DamageData dmg)
	{
	}

	private void Awake()
	{
		t = base.transform;
		rb = GetComponent<Rigidbody>();
		clldr = GetComponent<Collider>();
		startRotation = t.rotation;
		damage.amount = 50f;
		damage.knockdown = true;
		damage.newType = Game.style.basicStun;
		mf = GetComponent<MeshFilter>();
		loopSource = GetComponentInChildren<AudioSource>();
		startPos = t.localPosition;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		t.SetPositionAndRotation(startPos, startRotation);
		clldr.isTrigger = false;
		base.gameObject.layer = 14;
		base.gameObject.SetActive(value: true);
	}

	private IEnumerator Knocking(Vector3 dir)
	{
		dir = dir.With(null, 0f).normalized;
		Game.sounds.PlayClip(koSound);
		CameraController.shake.Shake(2);
		t.Rotate(dir, UnityEngine.Random.Range(-10f, 10f), Space.World);
		t.position += dir * 0.2f;
		base.gameObject.layer = 17;
		clldr.isTrigger = true;
		loopSource.Play();
		Physics.Raycast(t.position, dir, out hit, 16f, 1);
		if (hit.distance == 0f)
		{
			targetPos = rb.position + dir * 16f;
		}
		else
		{
			targetPos = hit.point - dir.normalized;
		}
		while (pos != targetPos)
		{
			pos = Vector3.MoveTowards(t.position, targetPos, Time.deltaTime * 30f);
			t.Rotate(Vector3.forward * (-1440f * Time.deltaTime));
			t.Rotate(dir * (90f * Time.deltaTime), Space.World);
			t.position = pos;
			yield return null;
		}
		QuickEffectsPool.Get("Wooden Debris", rb.worldCenterOfMass).Play();
		base.gameObject.SetActive(value: false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 10)
		{
			damage.dir = dir;
			other.GetComponentInChildren<IDamageable<DamageData>>().Damage(damage);
			QuickEffectsPool.Get("Wooden Debris", t.position).Play();
			StyleRanking.instance.AddStylePoint(StylePointTypes.DoorSlam);
			base.gameObject.SetActive(value: false);
		}
	}
}
