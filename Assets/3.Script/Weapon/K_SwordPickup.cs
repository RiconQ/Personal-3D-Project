using UnityEngine;

public class K_SwordPickup : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("teat");
        collision.transform.GetComponentInChildren<K_SwordController>().gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger");
        FindObjectOfType<K_WeaponHolder>().weaponArray[0].gameObject.SetActive(true);
    }
}
