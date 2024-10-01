using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class BulletPooling : MonoBehaviour
{
    public static BulletPooling instance;

    public GameObject poolingObj;
    public int poolCount = 40;

    private Queue<TurretBullet> pool = new Queue<TurretBullet> ();

    private void Awake()
    {
        if(instance  == null)
        {
            instance = this;
            Initialize();
        }

    }

    private void Initialize()
    {
        for(int i = 0; i <  poolCount; i++)
        {
            pool.Enqueue(CreateObj());
        }
    }

    private TurretBullet CreateObj()
    {
        var newObj = Instantiate(poolingObj).GetComponent<TurretBullet>();
        newObj.gameObject.SetActive(false);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    public static TurretBullet GetObj()
    {
        if(instance.pool.Count > 0)
        {
            var obj = instance.pool.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            var newObj = instance.CreateObj();
            newObj.gameObject.SetActive(true);
            return newObj;
        }
    }

    public static void ReturnObj(TurretBullet obj)
    {
        obj.gameObject.SetActive (false);
        obj.transform.SetParent(instance.transform);
        instance.pool.Enqueue(obj);
    }
}
