using UnityEngine;

public class K_BowPickup : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("teat");
        collision.transform.GetComponentInChildren<K_BowController>().gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger");
        FindObjectOfType<K_WeaponHolder>().weaponArray[1].gameObject.SetActive(true);
    }
}
