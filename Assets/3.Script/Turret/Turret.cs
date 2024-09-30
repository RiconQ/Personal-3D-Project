using UnityEngine;

public class Turret : MonoBehaviour, K_IDamageable
{
    public Transform bodyTransform;
    public Transform tempTransfomr;
    public Transform playerTransform;

    public float rotationSpeed = 5f;
    public float shootDuration = 1f; 


    public float range = 20f;

    private float shootTimer;

    private void Start()
    {
        playerTransform = Player.instance.PlayerCharacter.transform;
    }

    private void Update()
    {
        var start = new Vector3(tempTransfomr.position.x, 1, tempTransfomr.position.z);
        Debug.DrawRay(start, tempTransfomr.forward * 20);
        if (Vector3.Distance(bodyTransform.position, playerTransform.position) <= range)
        {
            RotateBody(); 
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

        var start = new Vector3(tempTransfomr.position.x, 1, tempTransfomr.position.z);
        Ray ray = new Ray(start, tempTransfomr.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 20f))
        {
            Debug.Log("hit" + hit.transform.gameObject.name);
            if (hit.collider.gameObject.layer == 6)
            {
                Debug.Log("Player Hit");
            }
        }
        shootTimer = shootDuration;
    }

    private void RotateBody()
    {
        var direction = playerTransform.position - bodyTransform.position;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            bodyTransform.rotation = Quaternion.Slerp(bodyTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            tempTransfomr.rotation = Quaternion.Slerp(bodyTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void Damage()
    {
        Destroy(gameObject);
    }
}
