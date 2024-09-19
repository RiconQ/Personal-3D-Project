using UnityEngine;

public class ObjectThrow : MonoBehaviour
{
    private PlayerCharacter _pm;

    [Header("Reference")]
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private GameObject _objectToThrow;
    public GameObject ObjectToThrow => _objectToThrow;

    [Header("Detection")]
    [SerializeField] private float _detectionDistance = 2f;
    [SerializeField] private float _detectionRadius = 1f;
    [SerializeField] private LayerMask _whatIsThrowable;

    [Header("Throwing")]
    [SerializeField] private float _throwForce = 20f;
    [SerializeField] private float _throwUpwardForce = 10f;
    [SerializeField] private float _throwCooldown = 0.1f;

    private bool _readyToThrow;
    private bool _canPickUp;
    public bool ReadyToThrow => _readyToThrow;

    public void Initialize()
    {
        _pm = GetComponent<PlayerCharacter>();
        _readyToThrow = false;
        _canPickUp = false;
    }

    public void LateUpdateThrow()
    {
        if (_readyToThrow)
        {
            //Holding Object
            _objectToThrow.transform.position = _attackPoint.position;
            _objectToThrow.transform.rotation = _camera.rotation;
        }
        else
        {
            //Detect Pickup
            RaycastHit hit;
            if (Physics.SphereCast(
                _camera.position, _detectionRadius, _camera.forward, out hit,
                _detectionDistance, _whatIsThrowable))
            {
                //_objectToThrow =  hit.transform.gameObject;
                Debug.Log(hit.transform.name);
                _canPickUp = true;
            }
            else
            {
                _canPickUp = false;
            }
        }
    }

    public void PickUpObject()
    {
        Debug.Log("TryToPickup");
        if (_canPickUp)
        {
            RaycastHit hit;
            if (Physics.SphereCast(
                _camera.position, _detectionRadius, _camera.forward, out hit,
                _detectionDistance, _whatIsThrowable))
            {
                _objectToThrow = hit.transform.gameObject;
            }
            _canPickUp = false;
            _readyToThrow = true;

            _pm.ignoredCollider.Add(_objectToThrow.GetComponent<Collider>());
        }
    }

    public void Throw()
    {
        _readyToThrow = false;
       // GameObject projectile = Instantiate(_objectToThrow, _attackPoint.position, _camera.rotation);

        Rigidbody projectileRb = _objectToThrow.GetComponent<Rigidbody>();

        Vector3 forceDirection = _camera.forward;

        RaycastHit hit;
        if (Physics.Raycast(_camera.position, _camera.forward, out hit, 500f))
        {
            forceDirection = (hit.point - _attackPoint.position).normalized;
        }

        Vector3 forceToAdd = _camera.forward * _throwForce
            + transform.up * _throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        Invoke(nameof(ResetThrow), _throwCooldown);
    }

    private void ResetThrow()
    {
        _pm.ignoredCollider.Remove(_objectToThrow.GetComponent<Collider>());
        _objectToThrow = null;
    }
}
