using UnityEngine;

public class K_WeaponHolder : MonoBehaviour
{
    public K_WeaponController[] weaponArray;

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
            weapon.UpdateController(deltaTime);
        }
    }

    public void LateUpdateController(float deltaTime)
    {
        foreach (var weapon in weaponArray)
        {
            weapon.LateUpdateController(deltaTime);
        }
    }

    public void UpdateInput(ControllerInput input, float deltaTime)
    {
        foreach (var weapon in weaponArray)
        {
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
}
