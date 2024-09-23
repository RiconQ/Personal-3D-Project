using System.Collections.Generic;
using UnityEngine;

public class ArrowPooling : MonoBehaviour
{
    public static ArrowPooling Instance;

    [Header("Reference")]
    [SerializeField] private Transform _cameraTrans;
    public Transform CameraTrans => _cameraTrans;
    [SerializeField] private Transform _shootPosition;
    public Transform ShootPosition => _shootPosition;

    [Header("Pooling")]
    [SerializeField] private GameObject _pollingObjectPrefab;
    [SerializeField] private int _poolCount = 30;

    private Queue<Arrow> _poolingObjectQueue = new Queue<Arrow>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
            Destroy(this.gameObject);
    }

    private void Initialize()
    {
        for(int i =0; i < _poolCount; i++)
        {
             _poolingObjectQueue.Enqueue(CreateNewObject());
        }
    }

    private Arrow CreateNewObject()
    {
        var newObj = Instantiate(_pollingObjectPrefab).GetComponent<Arrow>();
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    public static Arrow GetObject()
    {
        if(Instance._poolingObjectQueue.Count > 0)
        {
            var obj = Instance._poolingObjectQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = Instance.CreateNewObject();
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public static void ReturnObject(Arrow obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(Instance.transform) ;
        Instance._poolingObjectQueue.Enqueue(obj);
    }
}
