using UnityEngine;

public class K_PullPoint : MonoBehaviour
{

    private void Start()
    {
        if(!K_PullableControl.instance.Pullables.Contains(transform))
        {
            K_PullableControl.instance.Pullables.Add(transform);
        }
    }

    private void OnDestroy()
    {
        if(K_PullableControl.instance.Pullables.Contains(transform))
        {
            K_PullableControl.instance.Pullables.Remove(transform);
        }
    }
}
