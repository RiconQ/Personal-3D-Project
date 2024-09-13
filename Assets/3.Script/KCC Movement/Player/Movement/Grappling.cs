using UnityEngine;

public class Grappling : MonoBehaviour
{
    private PlayerCharacter _pm;

    [Header("Reference")]
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private Transform _gunTip;
    [SerializeField] private Transform _cameraTransform;

    [Header("Grappling")]
    [SerializeField] private float _maxGrappleDistance = 50f;
    [SerializeField] private float _detectionRadius = 4f;
    [SerializeField] private LayerMask _whatIsGrappable;

    private bool _isGrappling = false;
    public bool IsGrappline => _isGrappling;
    private bool _hasGrapplePoint = false;
    public bool HasGrapplePoint => _hasGrapplePoint;
    private Vector3 _grapplePoint;

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
        }
        else
        {
            //Grapple Point Not Found
            Debug.Log("Grapple Point Not Found");
            _hasGrapplePoint = false;
            _grapplePoint = _cameraTransform.position + _cameraTransform.forward * _maxGrappleDistance;
            Invoke("StopGrappling", 1f);
        }

        _lr.enabled = true;
        _lr.positionCount = 2;
        _lr.SetPosition(0, _gunTip.transform.position);
        _lr.SetPosition(1, _grapplePoint);
    }

    public void StopGrappling()
    {
        Debug.Log("Stop Grappling");

        _lr.enabled = false;

        _isGrappling = false;
        _hasGrapplePoint = false;
    }
}
