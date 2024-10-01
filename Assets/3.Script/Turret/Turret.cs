using UnityEngine;

public class Turret : MonoBehaviour, K_IDamageable
{
    public Transform bodyTransform;
    public Transform playerTransform;
    public Transform shootingTrans;

    public float rotationSpeed = 10f;
    public float shootDuration = 0.05f;

    public float range = 20f;

    private float shootTimer;

    private void Start()
    {
        playerTransform = Player.instance.PlayerCharacter.transform;
    }

    private void Update()
    {
        if (Vector3.Distance(bodyTransform.position, playerTransform.position) <= range)
        {
            bodyTransform.LookAt(playerTransform.position);
            if(shootTimer <= 0)
            {
                Shoot();
            }
            else
            {
                shootTimer -= Time.deltaTime;
            }
        }
    }

    private void Shoot()
    {
        //shoot fx
        Debug.Log("Shoot");
        shootTimer = shootDuration;
        var bullet = BulletPooling.GetObj();
        bullet.transform.position = shootingTrans.position;
        bullet.transform.rotation = shootingTrans.rotation;
    }

    private void RotateBody()
    {
        var direction = playerTransform.position - bodyTransform.position;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            bodyTransform.rotation = Quaternion.Slerp(bodyTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void Damage()
    {
        Destroy(gameObject);
    }
}
