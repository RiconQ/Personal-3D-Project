using UnityEngine;

public class GrapplingSwing : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private LineRenderer _lr;
    public LineRenderer Lr => _lr;
    [SerializeField] private Transform _cameraTransform;
    private Transform _gunTip;
    public Transform GunTip => _gunTip;

    [Header("Grappling Swing")]
    [SerializeField] private LayerMask _whatIsGrappable;
    [SerializeField] private float _grappleDetectionSize = 3f;
    [SerializeField] private float _maxGrappleDistance = 100f;
    [SerializeField] private float _swingDelayTime = 0.25f;
    //[SerializeField] private float _swingCooldownTime = 0.25f;
    [SerializeField] private float _swingForce = 1f;

    //reference
    private PlayerCharacter _pm;
    
    // grapllingSwing
    private Vector3 _swingPoint;
    //private float _swingCooldownTimer;
    private Vector3 _characterToSwingPoint; //Vector character to swingPoint

    private bool _isGrappling = false;
    public bool IsGrappling => _isGrappling;

    private bool _isSwinging = false;
    public bool IsSwing => _isSwinging;

    public void Initialize(PlayerCharacter pm)
    {
        _pm = pm;
        _lr.enabled = false;
        _gunTip = _pm.GunTip;
    }

    public void StartGrapplingSwing()
    {
        RaycastHit hit;
        _isGrappling = true;

        if (Physics.SphereCast(_cameraTransform.position, _grappleDetectionSize, _cameraTransform.forward, out hit, _maxGrappleDistance))
        {
            //GrappleHit
            _swingPoint = hit.point;

            //Grapple Animation

            Invoke(nameof(ExecuteSwing), _swingDelayTime);
        }
        else
        {
            //Grapple Miss
            _swingPoint = _cameraTransform.position + _cameraTransform.forward * _maxGrappleDistance;

            //Grapple Animation;

            Invoke(nameof(StopGrapplingSwing), _swingDelayTime);
        }

        //If grapple Animation Implement -> modify this
        _lr.enabled = true;
        _lr.SetPosition(1, _swingPoint);
    }

    public void SwingMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        _characterToSwingPoint = transform.position - _swingPoint;

        currentVelocity = Vector3.ProjectOnPlane(currentVelocity, _characterToSwingPoint.normalized);

        Vector3 nextPos = transform.position + (currentVelocity * deltaTime);

        Vector3 anchorPointToNextPos = nextPos - _swingPoint;

        float distanceToAnchorPoint = anchorPointToNextPos.magnitude;

        if (distanceToAnchorPoint > _maxGrappleDistance)
        {
            Vector3 nextposCorrected = _swingPoint + (anchorPointToNextPos.normalized * _maxGrappleDistance);

            currentVelocity = (nextposCorrected - transform.position) / deltaTime;
            currentVelocity = Vector3.Lerp(currentVelocity, (nextposCorrected - transform.position) / deltaTime, 0.1f);
        }
        else
        {
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, anchorPointToNextPos.normalized);
        }
    }

    private void ExecuteSwing()
    {
        //DoFov
        _isSwinging = true;
        //Reset JumpCount
        _pm.ResetJumpCount();
    }

    public void StopGrapplingSwing()
    {
        //DoFov
        _isSwinging = false;
        _isGrappling = false;

        _lr.enabled = false;
    }

    public void DrawRope(int index, Vector3 position)
    {
        _lr.SetPosition(index, position);
    }
}
