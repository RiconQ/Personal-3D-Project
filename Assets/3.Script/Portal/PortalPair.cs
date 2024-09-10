using UnityEngine;

public class PortalPair : MonoBehaviour
{
    public Portal[] portals {  get; private set; }

    private void Awake()
    {
        portals = GetComponentsInChildren<Portal>();
        if(portals.Length != 2 )
        {
            Debug.LogError("PortalPair children must contain exactly two Portal components in total.");
        }
    }
}
