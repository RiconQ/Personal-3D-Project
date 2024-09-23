using UnityEngine;

public class ChainDagger : MonoBehaviour
{
    [Header("Chain Dagger")]
    [SerializeField] private GameObject _chainPrefab;
    [SerializeField] private Transform _chainRoot;
    [SerializeField] private Transform _chainPos;
    public Transform ChainPos => _chainPos;
    [SerializeField] private Transform _meshTrans;
    public Transform MeshTrans => _meshTrans;
    [SerializeField] private float _speed = 25;


    private Transform[] _segments;
    private int _chainDist;
    private int _chainClickDist;
    private int _chainLastClickDist;
    private float _dist;
    private int _intDist;

    private void Awake()
    {
        CreateChainMesh();
    }

    private void Update()
    {
        if(!CheckState())
        {
            Reset();
        }
        else
        {
            _meshTrans.Rotate(0f, -1440f * Time.deltaTime, 0f, Space.Self);
            transform.Translate(Vector3.forward * (Time.deltaTime * _speed));

            _dist += Time.deltaTime * _speed;
        }

    }

    public void Reset()
    {
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

    public void AlignChainMesh(Transform playerPos)
    {
        float num = Vector3.Distance(playerPos.position, _chainPos.position);
        _chainDist = Mathf.RoundToInt(num / 0.3f);
        _chainClickDist = Mathf.RoundToInt(num / 2f);

        for(int i = 0; i < 100; i++)
        {
            if (_segments[i].gameObject.activeInHierarchy != i < _chainDist)
            {
                _segments[i].gameObject.SetActive(i < _chainDist);
            }
        }

        if(!_chainRoot.gameObject.activeInHierarchy)
        {
            _chainRoot.gameObject.SetActive(true);
        }
        if(_chainClickDist != _chainLastClickDist)
        {
            _chainLastClickDist = _chainClickDist;
        }
    }

    public bool CheckState()
    {
        if(!gameObject.activeInHierarchy)
        {
            return false;
        }
        //hookedCollider
        return true;
    }

    public void Activate(Transform position)
    {
        transform.SetPositionAndRotation(position.position, position.rotation);
        
        gameObject.SetActive(true);
        _dist = (_intDist = -1);
        _speed = 45f;
    }
}
