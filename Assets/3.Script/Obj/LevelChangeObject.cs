using UnityEngine;

public class LevelChangeObject : MonoBehaviour
{
    public string sceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 6) return;
        GameManager.instance.ChangeScene(sceneName);
    }
}
