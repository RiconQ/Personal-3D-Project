using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo Instance;

    public int playerHealth = 3;
    public bool isDead = false;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void Heal()
    {
        playerHealth += 1;
        playerHealth = Mathf.Clamp(playerHealth, 0, 3);
    }

    public void TakeDamage()
    {
        if (isDead) return;
        playerHealth -= 1;
        if (playerHealth <= 0)
        {
            isDead = true;
            Dead();    
        }
    }

    public void Dead()
    {
        GameManager.instance.ReloadScene();
    }
}
