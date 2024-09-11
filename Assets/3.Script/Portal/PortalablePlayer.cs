using Unity.Mathematics;
using UnityEngine;

public class PortalablePlayer : PortalableObject
{
    private PlayerCharacter_Portal _pm;
    public PlayerCharacter_Portal PM => _pm;
    [SerializeField] private bool _isPortal;
    public bool IsPortal => _isPortal;

    private bool _isWarp = false;
    public bool IsWarp => _isWarp;

    public void Initialize(PlayerCharacter_Portal pm)
    {
        _pm = pm;
    }

    public void StartPortal()
    {
        _isPortal = true;
    }

    //public void WarpPosition()
    //{
    //
    //}
    //public void WarpRotation(ref Quaternion currentRotation)
    //{
    //
    //}
    //public void WarpMovement(ref Vector3 currentVelocity)
    //{
    //}

    public void StopPortal()
    {
        _isPortal = false;
        _isWarp = false;
    }

    private new void Awake()
    {
        base.Awake();
    }
    public void StartWarp()
    {
       _isWarp= true;
    }

    public override void SetIsInPortal(Portal inPortal, Portal outPortal, Collider wallCollider)
    {
        this._inPortal = inPortal;
        this._outPortal = outPortal;

        _pm.AddIgnoreColl(wallCollider);
        

        cloneObject.SetActive(false);

        ++_inPortalCount;
    }

    public override void ExitPortal(Collider wallCollider)
    {

        _pm.RemoveIgnoreColl(wallCollider);
        --_inPortalCount;

        if (_inPortalCount == 0)
        {
            cloneObject.SetActive(false);
        }
    }


    public void WarpRotation(ref Quaternion currentRotation)
    {
        var inTransform = _inPortal.transform;
        var outTransform = _outPortal.transform;

        //Update position of object
        Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
        relativePos = _halfTurn * relativePos;
        _pm.SetPosition(outTransform.TransformPoint(relativePos));

        //Update rotation of object
        
        //Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
        //relativeRot = _halfTurn * relativeRot;
        //currentRotation = outTransform.rotation * relativeRot;
        
    }

    public void WarpVelocity(ref Vector3 currentVelocity)
    {
        //Debug.Log("Warp");
        var inTransform = _inPortal.transform;
        var outTransform = _outPortal.transform;

        //Update velocity of KCC
        //Debug.Log($"motor velo {_pm.Motor.Velocity}");
        Vector3 relativeVel = inTransform.InverseTransformDirection(_pm.Motor.Velocity);
        //Debug.Log($"relative Vel {relativeVel}");
        relativeVel = _halfTurn * relativeVel;
        currentVelocity = outTransform.TransformDirection(relativeVel);


        //Debug.Log($"outTransform.TransformDirection(relativeVel) {outTransform.TransformDirection(relativeVel)}\n" +
        //    $"current Velocity {currentVelocity}");

        var tmp = _inPortal;
        _inPortal = _outPortal;
        _outPortal = tmp;
    }
}
