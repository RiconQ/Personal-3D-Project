using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class ChainDagger : MonoBehaviour
{
    public enum ETargetType
    {
        None,
        Pullable,
        Breakable,
        Enemy
    }

    public static Action<GameObject> OnHit = delegate { };

    [Header("Chain Dagger")]
    [SerializeField] private GameObject _chainPrefab;
    [SerializeField] private Transform _chainRoot;
    [SerializeField] private Transform _chainPos;
    public Transform ChainPos => _chainPos;
    [SerializeField] private Transform _meshTrans;
    public Transform MeshTrans => _meshTrans;
    [SerializeField] private float _speed = 25;


    public Collider hoockedCol { get; private set; }
    public Vector3 targetPos { get; private set; }

    private Transform[] _segments;
    private int _chainDist;
    private int _chainClickDist;
    private int _chainLastClickDist;
    private float _dist;
    private int _intDist;
    private Transform _target;
    private Collider _collider;
    private ETargetType _targetType;
    private Vector3 _lastPos;
    private RaycastHit _hit;

    private void Awake()
    {
        _collider = GetComponentInChildren<Collider>();
        CreateChainMesh();
    }

    private void Update()
    {
        if (!CheckState())
        {
            Reset();
        }
        else if (!hoockedCol)
        {
            _meshTrans.Rotate(0f, -1440f * Time.deltaTime, 0f, Space.Self);
            transform.Translate(Vector3.forward * (Time.deltaTime * _speed));

            UpdateTargetPos();
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.position.DirTo(targetPos)), Time.deltaTime * 90f);

            _dist += Time.deltaTime * _speed;
        }
        else
        {
            UpdateTargetPos();
        }
    }

    private void LateUpdate()
    {
        if (hoockedCol)
        {
            transform.position = targetPos;
            transform.rotation = Quaternion.LookRotation(-hoockedCol.transform.forward);
            _chainRoot.rotation = Quaternion.LookRotation(_chainPos.position.DirTo(K_DaggerController.instance.Pivot.position));
            _chainRoot.position = _chainPos.position;
        }
    }

    private void FixedUpdate()
    {
        if (hoockedCol)
            return;

        if((int)_dist * 2 != _intDist)
        {
            Physics.Linecast(_lastPos, transform.position, out _hit, 1);
            if (_hit.distance != 0f)
            {
                HitStop(_hit.point, _hit.normal);
                return;
            }
            _lastPos = transform.position;
        }
        if(_dist >= ((_targetType == ETargetType.None) ? 12 : 18))
        {
            Stop();
        }
    }

    public void Reset()
    {
        hoockedCol = null;
        _chainRoot.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void CreateChainMesh()
    {
        _segments = new Transform[100];

        for (int i = 0; i < 100; i++)
        {
            _segments[i] = Instantiate(_chainPrefab).transform;
            _segments[i].SetParent(_chainRoot);
            _segments[i].localPosition = new Vector3(0f, 0f, (float)i * 0.3f);
            _segments[i].localEulerAngles = new Vector3(0f, 0f, (1 % 2 != 1) ? 90 : 0f);
        }

        _chainRoot.gameObject.SetActive(false);
        _chainRoot.SetParent(null);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals("Player"))
        {
            return;
        }

        _collider.enabled = false;
        hoockedCol = other;

        if (other.gameObject.layer == 8)
        {
            if (OnHit != null)
            {
                OnHit(hoockedCol.gameObject);
            }
            return;
        }
        else
        {
            Stop();
        }
        //switch (other.gameObject.layer)
        //{
        //    case var layer when layer == LayerMask.NameToLayer("Interactable"):
        //        if (OnHit != null)
        //        {
        //            OnHit(hoockedCol.gameObject);
        //        }
        //        break;
        //}
        //Reset();
    }

    public void AlignChainMesh(Transform playerPos)
    {
        float num = Vector3.Distance(playerPos.position, _chainPos.position);
        _chainDist = Mathf.RoundToInt(num / 0.3f);
        _chainClickDist = Mathf.RoundToInt(num / 2f);

        for (int i = 0; i < 100; i++)
        {
            if (_segments[i].gameObject.activeInHierarchy != i < _chainDist)
            {
                _segments[i].gameObject.SetActive(i < _chainDist);
            }
        }

        if (!_chainRoot.gameObject.activeInHierarchy)
        {
            _chainRoot.gameObject.SetActive(true);
        }

        if (_chainClickDist != _chainLastClickDist)
        {
            _chainLastClickDist = _chainClickDist;
        }
    }

    public bool CheckState()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log("!gameObject.activeInHierarchy return false");
            return false;
        }
        if(hoockedCol)
        {
            Debug.Log($"hoockedCol.gameObject.activeInHierarchy : {hoockedCol.gameObject.activeInHierarchy}");
            return hoockedCol.gameObject.activeInHierarchy;
        }
        Debug.Log("Check State Return true");
        return true;
    }

    public void Pull()
    {
        _chainRoot.gameObject.SetActive(false);
    }
    private void HitStop(Vector3 pos, Vector3 normal)
    {
        Reset();
    }

    private void Stop()
    {
        Reset();
    }

    public void Activate(Transform position)
    {
        _target = null;
        _targetType = ETargetType.None;

        transform.SetPositionAndRotation(position.position, position.rotation);
        _lastPos = transform.position;
        gameObject.SetActive(true);
        hoockedCol = null;
        _collider.enabled = true;
        _dist = (_intDist = -1);
        _speed = 45f;
        K_PullableControl.GetCurrent(out _target);
        if ((bool)_target)
        {
            _targetType = ETargetType.Pullable;
        }
        if ((bool)_target)
        {
            UpdateTargetPos();
            transform.rotation = Quaternion.LookRotation(transform.position.DirTo(targetPos));
        }
    }

    public void UpdateTargetPos()
    {
        if (hoockedCol)
        {
            targetPos = hoockedCol.bounds.center;
        }

        switch (_targetType)
        {
            case ETargetType.None:
                break;
            case ETargetType.Pullable:
                targetPos = _target.position;
                break;
            case ETargetType.Breakable:
                break;
            case ETargetType.Enemy:
                break;
        }
    }
}
