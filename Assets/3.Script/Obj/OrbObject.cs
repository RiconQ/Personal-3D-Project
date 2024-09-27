using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbObject : MonoBehaviour, K_IDamageable
{
    public void Damage()
    {
        GameManager.instance.ChangeScene("Main Hub");
    }
}
