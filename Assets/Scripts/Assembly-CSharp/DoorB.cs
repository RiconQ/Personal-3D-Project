using System;
using UnityEngine;

public class DoorB : MonoBehaviour, IDamageable<DamageData>, IKickable<Vector3>
{
	public enum DoorTypes
	{
		Kick = 0,
		Bash = 1
	}

	public DoorTypes type;

	public GameObject prefab;

	private FlyingDoor door;

	private RaycastHit hit;

	private void Awake()
	{
		door = UnityEngine.Object.Instantiate(prefab, base.transform.position, base.transform.rotation).GetComponent<FlyingDoor>();
		door.gameObject.SetActive(value: false);
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	public void Reset()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Break()
	{
		door.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
		door.gameObject.SetActive(value: true);
		QuickEffectsPool.Get("Wooden Debris", PlayerController.instance.tHead.position + PlayerController.instance.tHead.forward * 2f, PlayerController.instance.tHead.rotation).Play();
		base.gameObject.SetActive(value: false);
	}

	public void Damage(DamageData dmg)
	{
	}

	public void Kick(Vector3 dir)
	{
		if (type == DoorTypes.Kick)
		{
			Break();
		}
	}
}
