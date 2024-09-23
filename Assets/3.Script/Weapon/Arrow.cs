using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    [Header("Arrow")]
    [SerializeField] private float _remainTime = 5f;
    [SerializeField] private float _shootForce = 120f;
    [SerializeField] private float _shootUpwardForce = 0f;
    [SerializeField] private float _maxDistance = 500f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    public void Shoot()
    {

        _rb.isKinematic = false;

        this.transform.position = ArrowPooling.Instance.ShootPosition.position;
        this.transform.rotation = ArrowPooling.Instance.CameraTrans.rotation;

        Vector3 forceDirection = ArrowPooling.Instance.CameraTrans.forward;

        //RaycastHit hit;
        //if(Physics.Raycast(_cameraTrans.position, _cameraTrans.forward, out hit, _maxDistance))
        //{
        //    forceDirection = (hit.point - _shootPosition.position).normalized;
        //}
        //else
        //{
        //    forceDirection = _cameraTrans.forward.normalized;
        //}

        Vector3 forceToAdd = ArrowPooling.Instance.CameraTrans.forward * _shootForce + transform.up * _shootUpwardForce;

        _rb.AddForce(forceToAdd, ForceMode.Impulse);
        Invoke(nameof(Deactivate), _remainTime);
    }

    public void Deactivate()
    {
        ArrowPooling.ReturnObject(this);
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"CollisonEnter {other.gameObject.name}");
        _rb.isKinematic = true;
        //_rb.velocity = Vector3.zero;

        //Make sure projectile moves with target
        transform.SetParent(other.transform);
    }
}
