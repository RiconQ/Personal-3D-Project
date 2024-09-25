using UnityEngine;

public class K_WeaponHolder : MonoBehaviour
{
    public static K_WeaponHolder instance = null;

    public Transform currentWeapon;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
        currentWeapon = null;
    }

    public K_WeaponController[] weaponArray;

    public bool isCharging = false;
    public void Initialize()
    {
        foreach (var weapon in weaponArray)
        {
            weapon.Initialize();
        }
    }

    public void UpdateController(float deltaTime)
    {
        foreach (var weapon in weaponArray)
        {
            if (weapon.gameObject.activeSelf)
                weapon.UpdateController(deltaTime);
        }
    }

    public void LateUpdateController(float deltaTime)
    {
        foreach (var weapon in weaponArray)
        {
            if (weapon.gameObject.activeSelf)
                weapon.LateUpdateController(deltaTime);
        }
    }

    public void UpdateInput(ControllerInput input, float deltaTime)
    {
        foreach (var weapon in weaponArray)
        {
            if (weapon.gameObject.activeSelf)
                weapon.UpdateInput(input, deltaTime);
        }
    }

    public bool HasWaepon()
    {
        for (int i = 0; i < weaponArray.Length - 1; i++)
        {
            if (weaponArray[i].gameObject.activeSelf) return true;
        }
        return false;
    }

    public int GetCurrentWeapon()
    {
        for (int i = 0; i < weaponArray.Length - 1; i++)
        {
            if (weaponArray[i].gameObject.activeSelf) return i;
        }
        return -1;
    }

    public void PickUpWeapon(int weaponIndex, Transform weaponToPick)
    {
        currentWeapon = weaponToPick;
        weaponArray[weaponIndex].gameObject.SetActive(true);
    }

    public void DropWeapon(int weaponIndex)
    {
        weaponArray[weaponIndex].gameObject.SetActive(false);
    }

    public void DropCurrentWeapon()
    {
        switch (currentWeapon.GetComponent<K_WeaponObj>().weapon)
        {
            case K_WeaponObj.EWeapon.Sword:
                weaponArray[0].gameObject.SetActive(false);
                currentWeapon.GetComponent<K_WeaponObj>().Drop();
                break;
            case K_WeaponObj.EWeapon.Bow:
                break;
        }
    }
}