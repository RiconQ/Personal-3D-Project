using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalGun : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PortalPair _portals;
    [SerializeField] private PlayerCamera_Portal _camera;

    [Header("PortalGun")]
    [SerializeField] private LayerMask _layerMask;

    public void InputFire(int mouseIndex)
    {
        FirePortal(mouseIndex, _camera.transform.position, _camera.transform.forward, 250.0f);
    }

    private void FirePortal(int portalID, Vector3 pos, Vector3 dir, float distance)
    {
        //Debug.Log($"Fire Portal {portalID}");

        RaycastHit hit;
        Physics.Raycast(pos, dir, out hit, distance, _layerMask);

        if(hit.collider != null)
        {
            //Orient the portal according to camera look direction and surface direction
            var cameraRotation = _camera.transform.rotation;
            var portalRight = cameraRotation * Vector3.right;

            if(Mathf.Abs(portalRight.x) >= Mathf.Abs(portalRight.z))
            {
                portalRight = (portalRight.x >= 0) ? Vector3.right : -Vector3.right;
            }
            else
            {
                portalRight = (portalRight.z >= 0) ? Vector3.forward : -Vector3.forward;
            }

            var portalForward = -hit.normal;
            var portalUp = -Vector3.Cross(portalRight, portalForward);

            var portalRotation = Quaternion.LookRotation(portalForward, portalUp);

            //Attempt to place the portal
            bool wasPlaced = _portals.portals[portalID].PlacePortal(hit.collider, hit.point, portalRotation);
        }
    }
}
