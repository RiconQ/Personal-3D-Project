using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrapplingGun : MonoBehaviour
{
    [Header("Assign")]
    [SerializeField] private Transform _gunTip; // Transform where grappling start
    [SerializeField] private GameObject _gunObject; // grappling gun object

    private LineRenderer _lineRenderer;

    private Vector3 _grapplePoint;
    private Vector3 _currentGrapplePosition;
    private bool _isGrappling =false;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();    
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    public void StartGrappling(Vector3 targetPoint)
    {
        Debug.Log("Start Grappling");
        _isGrappling = true;

        //Debug.DrawLine(_gunTip.position, targetPoint);
        //_currentGrapplePosition = _gunTip.position;
        _grapplePoint = targetPoint;
        
        _lineRenderer.positionCount = 2;
    }

    public void DrawRope()
    {
        if (!_isGrappling) return;

        //_currentGrapplePosition = Vector3.Lerp(_currentGrapplePosition, _grapplePoint, Time.deltaTime);

        //Draw Grappling when _isGrappling == true
        _lineRenderer.SetPosition(0, _gunTip.position);
        _lineRenderer.SetPosition(1, _grapplePoint);
    }

    public void StopGrappling()
    {
        Debug.Log("Stop Grappling");
        _isGrappling = false;

        _lineRenderer.positionCount = 0;
    }
}
