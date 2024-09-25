using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class K_ThrowedWeaponPooling : MonoBehaviour
{
    [SerializeField] private GameObject _poolingObjectPrefab;
    [SerializeField] private int _poolCount = 10;

    private Queue<K_ThrowedWeapon> _poolQueue = new Queue<K_ThrowedWeapon>();

    private void Start()
    {
        for(int i=0;i < _poolCount;i++)
        {
            _poolQueue.Enqueue(CreateNewObject());
        }
    }

    private K_ThrowedWeapon CreateNewObject()
    {
        var newObj = Instantiate(_poolingObjectPrefab).GetComponent<K_ThrowedWeapon>();
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    public K_ThrowedWeapon GetObject()
    {
        if(_poolQueue.Count > 0)
        {
            var obj = _poolQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = CreateNewObject();
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public void Return(K_ThrowedWeapon obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        _poolQueue.Enqueue(obj);
    }
}
