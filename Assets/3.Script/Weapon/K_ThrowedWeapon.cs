using DG.Tweening;
using UnityEngine;


public class K_ThrowedWeapon : MonoBehaviour
{
    private float speed = 35f;
    [SerializeField] private Transform _meshTrans;
    private Vector3 _meshRotation = new Vector3(1440f, 0f, 0f);
    private float _gravity = 10f;

    private float _dist;
    private int _intDist;
    private Vector3 _lastPos;
    private RaycastHit _hit;


    public void ThrowWeapon(Quaternion rot)
    {
        this.transform.position = Player.instance.PlayerCamera.transform.position;
        this.transform.rotation = rot;

        _lastPos = transform.position;
        _dist = (_intDist = 0);
        Physics.Raycast(transform.position, transform.forward, out _hit, 1.2f);
    }

    private void Update()
    {
        transform.Translate(0f, 0f, speed * Time.deltaTime);
        _meshTrans.Rotate(_meshRotation * Time.deltaTime);
        transform.Rotate(Time.deltaTime * _gravity, 0f, 0f);

        _dist += Time.deltaTime * speed;
    }

    private void FixedUpdate()
    {
        if ((int)_dist * 2 != _intDist)
        {
            _intDist = (int)_dist * 2;
            Physics.Linecast(_lastPos, transform.position, out _hit, 1);
            _lastPos = transform.position;
        }
        if (_dist > 40f || _hit.distance != 0f)
        {
            Debug.Log($"_dist > 40f : {_dist > 40f} || _hit.distance != 0f : {_hit.distance != 0f}");
            Stop();
        }
    }

    private void Stop()
    {
        //Assume Sword Stop on wall

        K_WeaponHolder.instance.currentWeapon.transform.SetPositionAndRotation(transform.position, transform.rotation);
        K_WeaponHolder.instance.currentWeapon.gameObject.SetActive(true);
        K_WeaponHolder.instance.currentWeapon = null;

        K_WeaponHolder.instance.swordPool.Return(this);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent<K_IDamageable>(out var tmp))
            tmp.Damage();
    }
}
