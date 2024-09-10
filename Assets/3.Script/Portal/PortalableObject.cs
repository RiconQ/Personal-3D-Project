using UnityEngine;

public class PortalableObject : MonoBehaviour
{
    private GameObject cloneObject;

    private int _inPortalCount = 0;

    private Portal _inPortal;
    private Portal _outPortal;

    private new Rigidbody _rigidBody;
    protected new Collider _collider;

    private static readonly Quaternion _halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);

    protected virtual void Awake()
    {
        cloneObject = new GameObject();
        cloneObject.SetActive(false);
        var meshFilter = cloneObject.AddComponent<MeshFilter>();
        var meshRenderer = cloneObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = GetComponentInChildren<MeshFilter>().mesh;
        meshRenderer.material = GetComponentInChildren<MeshRenderer>().material;
        cloneObject.transform.localScale = transform.localScale;

        _rigidBody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void LateUpdate()
    {
        if(_inPortal == null || _outPortal == null)
        {
            return;
        }

        //object between portals
        if(cloneObject.activeSelf && _inPortal.isPlaced && _outPortal.isPlaced)
        {
            var inTransform = _inPortal.transform;
            var outTransform = _outPortal.transform;

            //Update position of clone
            Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
            relativePos = _halfTurn * relativePos;
            cloneObject.transform.position = outTransform.TransformPoint(relativePos);

            //Update rotation of clone
            Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
            relativeRot = _halfTurn * relativeRot;
            cloneObject.transform.rotation = outTransform.rotation * relativeRot;
        }    
        else
        {
            cloneObject.transform.position = new Vector3(-1000.0f, 1000.0f, -1000.0f);
        }
    }

    public void SetIsInPortal(Portal inPortal, Portal outPortal, Collider wallCollier)
    {
        this._inPortal = inPortal;
        this._outPortal = outPortal;

        Physics.IgnoreCollision(_collider, wallCollier);

        cloneObject.SetActive(false);

        ++_inPortalCount;
    }

    public void ExitPortal(Collider wallCollider)
    {
        Physics.IgnoreCollision(_collider, wallCollider, false);
        --_inPortalCount;

        if(_inPortalCount == 0)
        {
            cloneObject.SetActive(false);
        }
    }

    public virtual void Warp()
    {
        var inTransform = _inPortal.transform;
        var outTransform = _outPortal.transform;

        //Update position of object
        Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
        relativePos = _halfTurn * relativePos;
        transform.position = outTransform.TransformPoint(relativePos);

        //Update rotation of object
        Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
        relativeRot = _halfTurn * relativeRot;
        transform.rotation = outTransform.rotation * relativeRot;

        //Update velocity of rigidbody
        // Need KCC Update
        Vector3 relativeVel = inTransform.InverseTransformDirection(_rigidBody.velocity);
        relativeVel = _halfTurn * relativeVel;
        _rigidBody.velocity = outTransform.TransformDirection(relativeVel);

        var tmp = _inPortal;
        _inPortal = _outPortal;
        _outPortal = tmp;
    }
}
