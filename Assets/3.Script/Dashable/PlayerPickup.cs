using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    public List<K_Dashable> _pickupable = new List<K_Dashable>();

    public void TryPickup()
    {

    }
}
