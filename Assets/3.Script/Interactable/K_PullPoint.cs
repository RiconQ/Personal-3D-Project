using UnityEngine;

public class K_PullPoint : MonoBehaviour, K_IDamageable
{
    public bool horizontal;
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

    public void Damage()
    {
        if(horizontal)
        {
            //PullInDir
            Player.instance.PlayerCharacter.PullInDir(transform.position - Vector3.up);
        }
        else
        {
            //PullTo
            Player.instance.PlayerCharacter.PullTo(transform.position);
        }
    }
}
