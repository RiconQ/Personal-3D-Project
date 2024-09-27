using UnityEngine;

public class JumperObject : MonoBehaviour
{
    public Vector3 direction;
    public float speed;

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<K_Jumper>().StartJumper(direction, speed);
    }
}
