using UnityEngine;

public class Grappling : MonoBehaviour
{
    private PlayerCharacter _pm;

    [Header("Reference")]
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Transform _gunTip;
    [SerializeField] private Transform _cameraTransform;

    [Header("Grappling")]
    [SerializeField] private float _grappleGravity = -90f;
    [SerializeField] private float _maxGrappleDistance = 50f;
    [SerializeField] private float _detectionRadius = 4f;
    [SerializeField] private LayerMask _whatIsGrappable;
    [SerializeField] private float _grappleDelayTime = 0.2f;
    [SerializeField] private float _overshootYAxis = 2f;

    
    private bool _isGrappling = false;
    public bool IsGrappline => _isGrappling;

    private bool _isGrappleExecuting = false;
    public bool IsGrappleExecuting => _isGrappleExecuting;

    private bool _hasGrapplePoint = false;
    public bool HasGrapplePoint => _hasGrapplePoint;

    private Vector3 _grapplePoint;
    private float _highestPointOnArc;
    

    public void Initialize()
    {
        _pm = GetComponent<PlayerCharacter>();
    }

    public void UpdateGrapple()
    {
        //Detect Grapplable Point
    }

    public void LateUpdateGrapple()
    {
        //Update lineRenderer
        if( _isGrappling )
        {
            _lr.SetPosition(0, _gunTip.position);
        }
    }

    public void ShootGrapple()
    {
        _isGrappling = true;
        RaycastHit hit;
        if (Physics.SphereCast(
            _cameraTransform.position,
            _detectionRadius,
            _cameraTransform.forward,
            out hit,
            _maxGrappleDistance,
            _whatIsGrappable))
        {
            //Grapple Point Found
            Debug.Log("Grapple Point Found");
            _grapplePoint = hit.transform.position;
            _hasGrapplePoint = true;

            Invoke(nameof(ExecuteGrapple), _grappleDelayTime);
        }
        else
        {
            //Grapple Point Not Found
            Debug.Log("Grapple Point Not Found");
            _hasGrapplePoint = false;
            _grapplePoint = _cameraTransform.position + _cameraTransform.forward * _maxGrappleDistance;
            Invoke(nameof(StopGrappling), 1f);
        }

        _lr.enabled = true;
        _lr.positionCount = 2;
        _lr.SetPosition(0, _gunTip.transform.position);
        _lr.SetPosition(1, _grapplePoint);
    }

    private Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = _grappleGravity;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    public void JumpToPosition(ref Vector3 currentVelocity)
    {
        if(_pm.Motor.GroundingStatus.IsStableOnGround)
        {
            _pm.Motor.ForceUnground(time: 0f);
        }
        currentVelocity = velocityToSet;
        _isGrappleExecuting = false;
    }
    private Vector3 velocityToSet;

    private void ExecuteGrapple()
    {
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = _grapplePoint.y - lowestPoint.y;
        _highestPointOnArc = grapplePointRelativeYPos + _overshootYAxis;

        if (grapplePointRelativeYPos < 0)
            _highestPointOnArc = _overshootYAxis;

        _isGrappleExecuting = true;
        velocityToSet = CalculateJumpVelocity(transform.position, _grapplePoint, _highestPointOnArc);

        Invoke(nameof(StopGrappling), 1f);
    }

    public void StopGrappling()
    {
        Debug.Log("Stop Grappling");

        _lr.enabled = false;

        _isGrappleExecuting = false;
        _isGrappling = false;
        _hasGrapplePoint = false;
    }
}
