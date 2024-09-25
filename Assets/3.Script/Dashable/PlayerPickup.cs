using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    public List<IK_Dashable> _pickupable = new List<IK_Dashable>();

    public void TryPickup()
    {

    }
}
