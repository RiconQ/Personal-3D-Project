using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : MonoBehaviour
{

    public float speed = 3f;

    private void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);           
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 6)
        {
            PlayerInfo.Instance.TakeDamage();
        }

        BulletPooling.ReturnObj(this);
    }
}
