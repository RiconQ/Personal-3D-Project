using UnityEngine;
using System.Collections.Generic;

public class K_WeaponObj : K_Dashable
{
    public enum EWeapon
    {
        Sword,
        Bow
    }

    [SerializeField] public EWeapon weapon;
    private void Start()
    {
        if (!K_WeaponControl.instance.allWeapon.Contains(this.transform))
        {
            K_WeaponControl.instance.allWeapon.Add(this.transform);
        }
        Collider[] col = GetComponents<Collider>();
        foreach (Collider c in col)
        {
            if (!Player.instance.PlayerCharacter.ignoredCollider.Contains(c))
            {
                Player.instance.PlayerCharacter.ignoredCollider.Add(c);
            }
        }
        //if(Player.instance.PlayerCharacter.ignoredCollider.Add())
    }
    public override void Dash()
    {
        gameObject.SetActive(false);
        switch (weapon)
        {
            case EWeapon.Sword:
                K_WeaponHolder.instance.PickUpWeapon(0, this.transform);
                break;
            case EWeapon.Bow:
                K_WeaponHolder.instance.PickUpWeapon(1, this.transform);
                break;
        }
    }

    public void Drop()
    {
        var dropForce = (Vector3.up + Player.instance.PlayerCamera.transform.forward.With(null, 0f) / 2f).normalized;
        dropForce = (dropForce + Vector3.up / 4f).normalized * 14f;
        this.transform.position = Player.instance.PlayerCamera.transform.position;
        this.transform.rotation = Quaternion.LookRotation(Player.instance.PlayerCamera.transform.right);
        gameObject.SetActive(true);
        this.GetComponent<Rigidbody>().AddForce(dropForce, ForceMode.Impulse);
        this.GetComponent<Rigidbody>().AddTorque(-transform.right * -1f, ForceMode.Impulse);
    }
}