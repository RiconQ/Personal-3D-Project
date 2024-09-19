using UnityEngine;

public class KnightBody : Body
{
	public string weaponPrefabName = "";

	public GameObject weaponRootObj;

	private Vector3 dir;

	private RaycastHit hit;

	private PooledWeapon weapon;

	private Vector3 force = new Vector3(0f, 7f, 0f);

	private Vector3 torque = new Vector3(90f, 90f, 90f);

	public override void DropSomething()
	{
		base.DropSomething();
		if (weaponPrefabName.Length > 0 && lastDamage.newType != Game.style.basicMill)
		{
			dir = force + base.rb.position.DirTo(PlayerController.instance.t.position.With(null, base.rb.position.y)) * 2f;
			weapon = QuickPool.instance.Get(weaponPrefabName, base.rb.position + Vector3.up, dir.Quaternionise()) as PooledWeapon;
			Physics.Raycast(base.rb.position, -lastDamage.dir, out hit, 4f, 1);
			if (hit.distance == 0f)
			{
				weapon.rb.AddForceAndTorque(dir, torque);
			}
			else
			{
				weapon.rb.AddForceAndTorque(hit.normal * 7f, torque);
				Debug.DrawRay(hit.point, hit.normal * 3f, Color.magenta, 3f);
			}
			weapon.SetHotTimer(1f);
			if (weaponRootObj.activeInHierarchy)
			{
				weaponRootObj.SetActive(value: false);
			}
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (!weaponRootObj.activeInHierarchy)
		{
			weaponRootObj.SetActive(value: true);
		}
	}
}
