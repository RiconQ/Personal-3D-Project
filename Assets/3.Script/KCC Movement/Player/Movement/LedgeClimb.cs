using System.Collections;
using UnityEngine;

public class LedgeClimb : MonoBehaviour
{
    /*
    //private PlayerCharacter _pm;
    //
    //[Header("Ledge Climb")]
    //[SerializeField] private LayerMask _ledgeLayer;
    //[SerializeField] private float _climbDuration;
    //[SerializeField] private Transform _playerCamera;
    //[SerializeField] private float _ledgeDetectionDistance = 2f;
    //
    //private bool _isInLedge = false;
    //public bool IsInLedge => _isInLedge;
    //private bool _isLedgeClimbing = false;
    //public bool IsLedgeClimbing => _isLedgeClimbing;
    //private RaycastHit _ledgeHit;
    //public void Initialize()
    //{
    //    _pm = GetComponent<PlayerCharacter>();
    //}
    //
    //public void UpdateLedgeClimb()
    //{
    //    CheckLedge();
    //}
    //
    //private void CheckLedge()
    //{
    //    var radius = _pm.Motor.Capsule.radius;
    //    var height = _pm.Motor.Capsule.height;
    //    
    //    if(Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out _ledgeHit, _ledgeDetectionDistance, _ledgeLayer))
    //    {
    //        Debug.Log("Ledge Found");
    //        _isInLedge = true;
    //    }
    //    else
    //    {
    //        _isInLedge = false;
    //    }
    //}
    //
    //public void StartLedgeClimb()
    //{
    //    if (Physics.Raycast(
    //        _ledgeHit.point +
    //        (_playerCamera.forward * _pm.Motor.Capsule.radius) +
    //        (Vector3.up * 0.6f * _pm.Motor.Capsule.height),
    //        Vector3.down, out var groundHit, _pm.Motor.Capsule.height))
    //    {
    //        StartCoroutine(LedgeClimb_co(groundHit.point));
    //    }
    //}
    //
    //private IEnumerator LedgeClimb_co(Vector3 targetPosition)
    //{
    //    _isLedgeClimbing = true;
    //    float time = 0;
    //    Vector3 startPosition = transform.position;
    //
    //    while (time < _climbDuration)
    //    {
    //        _pm.SetPosition(Vector3.Lerp(startPosition, targetPosition, time / _climbDuration));
    //        time += Time.deltaTime;
    //        yield return null;
    //    }
    //
    //    _pm.SetPosition(targetPosition);
    //    _isLedgeClimbing = false;
    //}
    */

    public enum EClimbState
    {
        None,
        Start,
        Climbing,
        End
    }

    private Player _pm;
    private RaycastHit _hit;
    private Vector3 _startPos;
    private Vector3 _startDir;
    private Vector3 _targetPos;
    [SerializeField]private EClimbState _state;
    private float _checkTimer;
    private int _checkCount = -1;
    public float timer { get; private set; }
    public bool isClimbing { get; private set; }

    private void Start()
    {
        _pm = Player.instance;
        _state = EClimbState.None;
        isClimbing = false;
    }

    public void TryClimb()
    {
        Debug.Log("TryClimb");
        if (_state != EClimbState.None) // || Attacking
        {
            Debug.Log("_state != EClimbState.None");
            return;
        }
        _checkTimer += Time.deltaTime;
        if (Mathf.FloorToInt(_checkTimer * 20f) != _checkCount)
        {
            _checkCount++;

            Vector3 position = _pm.PlayerCamera.transform.position;
            position.y += 1.5f;
            Vector3 vector = position + _pm.PlayerCamera.transform.forward.With(null, 0f).normalized * 2f;

            if (Physics.Linecast(_pm.PlayerCharacter.transform.position, position))
            {
                Debug.Log("Obstacle1");
                return;
            }
            if (Physics.Linecast(position, vector))
            {
                Debug.Log("Obstacle2");
                return;
            }
            if (Physics.Raycast(vector, Vector3.down, out _hit, 4f) &&
                _hit.normal.y.InDegrees() < 30f &&
                _hit.point.y + 1f > _pm.PlayerCharacter.transform.position.y &&
                !Physics.Raycast(
                    _pm.PlayerCharacter.transform.position.With(null, _hit.point.y + 1f),
                    _pm.PlayerCamera.transform.forward.With(null, 0f).normalized, 2f))
            {
                if (Physics.Raycast(_pm.PlayerCharacter.transform.position, Vector3.down, 1.5f))
                {
                    Debug.Log("Obstacle3");
                    return;
                }
                _targetPos = _hit.point + _hit.normal;
                _state = EClimbState.Start;
                _checkTimer = 0f;
                _checkCount = -1;
                isClimbing = true;
                return;
            }
            Debug.Log("return 1");
            return;
        }
        Debug.Log("return 2");
        return;
    }

    public void Reset()
    {
        if (_state != EClimbState.None)
        {
            _state = EClimbState.None;

        }
        _checkTimer = 0f;
        _checkCount = -1;
    }

    public void ClimbUpdate(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (_state)
        {
            case EClimbState.Start:
                currentVelocity = Vector3.zero;
                _startPos = _pm.PlayerCharacter.transform.position;
                float num = _targetPos.y - _startPos.y;
                _startDir = Vector3.Lerp((_startPos.DirTo(_targetPos).With(null, 0f) + Vector3.up).normalized,
                    Vector3.up * num, 0.7f);
                timer = 0f;
                _state = EClimbState.Climbing;
                break;
            case EClimbState.Climbing:
                timer = Mathf.MoveTowards(timer, 1f, deltaTime * 3f);
                var moveTo = CalculateCubicBezierPoint(timer, _startPos, _startPos + _startDir, _targetPos, _targetPos);
                _pm.PlayerCharacter.SetPosition(moveTo);
                if (timer == 1f)
                {
                    _state = EClimbState.End;
                }
                break;
            case EClimbState.End:
                isClimbing = false;
                _state = EClimbState.None;
                break;
        }
    }

    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float num = 1f - t;
        float num2 = t * t;
        float num3 = num * num;
        float num4 = num3 * num;
        float num5 = num2 * t;
        return num4 * p0 + 3f * num3 * t * p1 + 3f * num * num2 * p2 + num5 * p3;
    }
}